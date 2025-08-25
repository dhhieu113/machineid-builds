using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MachineId
{
    /// <summary>
    /// Provides access to machine identification information.
    /// </summary>
    public static class MachineIdentifier
    {
        // Define the path to the native library
        private const string LibraryName = "machineid_dotnet";

        static MachineIdentifier()
        {
            NativeLibrary.SetDllImportResolver(typeof(MachineIdentifier).Assembly, ResolveDllImport);
        }

        private static IntPtr ResolveDllImport(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            // Only handle our specific library
            if (libraryName != LibraryName)
                return IntPtr.Zero;

            string libName = LibraryName;
            
            // Apply platform-specific naming conventions
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                libName = $"libmachineid.dotnet.so";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                libName = $"libmachineid.dotnet.dylib";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                libName = $"{LibraryName}.dll";

            // Try to load from the runtimes folder
            string arch = RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X64 => "x64",
                Architecture.X86 => "x86",
                Architecture.Arm64 => "arm64",
                Architecture.Arm => "arm",
                _ => "x64"  // Default to x64
            };

            string platform = string.Empty;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                platform = $"win-{arch}";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                platform = $"linux-{arch}";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                platform = $"osx-{arch}";

            try
            {
                // Check for runtimes folder structure from NuGet package
                var assemblyLocation = assembly.Location;
                var assemblyDir = Path.GetDirectoryName(assemblyLocation);
                
                if (assemblyDir != null && platform != string.Empty)
                {
                    var runtimesPath = Path.Combine(assemblyDir, "runtimes", platform, "native", libName);
                    if (File.Exists(runtimesPath))
                        return NativeLibrary.Load(runtimesPath);
                }
            }
            catch
            {
                // Fallback to default loading if any error occurs
            }

            // Let the OS handle it
            return NativeLibrary.Load(libName);
        }

        /// <summary>
        /// Gets a unique hash identifier for the current machine.
        /// </summary>
        /// <returns>A string containing a hash uniquely identifying this machine.</returns>
        public static string GetMachineHash()
        {
            IntPtr resultPtr = MachineHash();
            if (resultPtr == IntPtr.Zero)
                return string.Empty;

            string result = Marshal.PtrToStringAnsi(resultPtr);
            FreeMachineHashString(resultPtr);
            return result;
        }

        // Import the native function from the C++ library
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr MachineHash();

        // Import the function to free the string allocated by the C++ code
        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FreeMachineHashString(IntPtr ptr);
    }
}
