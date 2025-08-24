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
        private static class NativeLibrary
        {
            private const string LibraryName = "machineid_dotnet";

            public static string GetLibraryName()
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return $"{LibraryName}.dll";
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return $"lib{LibraryName}.so";
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    return $"lib{LibraryName}.dylib";
                else
                    throw new PlatformNotSupportedException("Unsupported platform");
            }

            public static string GetPlatformName()
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return $"win-{GetArchitecture()}";
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return $"linux-{GetArchitecture()}";
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    return $"osx-{GetArchitecture()}";
                else
                    throw new PlatformNotSupportedException("Unsupported platform");
            }

            private static string GetArchitecture()
            {
                return RuntimeInformation.ProcessArchitecture switch
                {
                    Architecture.X64 => "x64",
                    Architecture.X86 => "x86",
                    Architecture.Arm64 => "arm64",
                    Architecture.Arm => "arm",
                    _ => throw new PlatformNotSupportedException($"Unsupported architecture: {RuntimeInformation.ProcessArchitecture}")
                };
            }

            public static string GetNativeLibraryPath()
            {
                // First, try to find the library from the runtimes folder relative to this assembly
                var assembly = typeof(MachineIdentifier).Assembly;
                var assemblyDir = Path.GetDirectoryName(assembly.Location);
                
                if (assemblyDir != null)
                {
                    // Try the standard runtime-specific path
                    var platform = GetPlatformName();
                    var libraryName = GetLibraryName();
                    var runtimePath = Path.Combine(assemblyDir, "runtimes", platform, "native", libraryName);
                    
                    if (File.Exists(runtimePath))
                        return runtimePath;
                    
                    // Try direct in output directory
                    var directPath = Path.Combine(assemblyDir, libraryName);
                    if (File.Exists(directPath))
                        return directPath;
                }
                
                // Fall back to just the library name and let the OS find it
                return GetLibraryName();
            }
        }
        
        static MachineIdentifier()
        {
            try
            {
                // On .NET Core 3.0+, we can use NativeLibrary.Load
                if (Environment.Version.Major >= 3)
                {
                    var nativeLibraryPath = NativeLibrary.GetNativeLibraryPath();
                    // Try to preload the native library (optional, for troubleshooting)
                    var handle = IntPtr.Zero;
                    var loadMethod = typeof(System.Runtime.InteropServices.NativeLibrary).GetMethod("Load", 
                        BindingFlags.Public | BindingFlags.Static, 
                        null, 
                        new Type[] { typeof(string), typeof(Assembly), typeof(DllImportSearchPath?) }, 
                        null);
                    
                    if (loadMethod != null)
                    {
                        handle = (IntPtr)loadMethod.Invoke(null, new object[] 
                        { 
                            nativeLibraryPath, 
                            typeof(MachineIdentifier).Assembly,
                            DllImportSearchPath.AssemblyDirectory | DllImportSearchPath.UserDirectories
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error or store it for diagnostics
                LibraryLoadError = ex.ToString();
            }
        }
        
        /// <summary>
        /// If there was an error loading the native library, this will contain the error message.
        /// </summary>
        public static string LibraryLoadError { get; private set; }

        /// <summary>
        /// Gets a unique hash identifier for the current machine.
        /// </summary>
        /// <returns>A string containing a hash uniquely identifying this machine.</returns>
        public static string GetMachineHash()
        {
            try
            {
                IntPtr resultPtr = MachineHash();
                if (resultPtr == IntPtr.Zero)
                    return string.Empty;

                string result = Marshal.PtrToStringAnsi(resultPtr);
                FreeMachineHashString(resultPtr);
                return result;
            }
            catch (DllNotFoundException ex)
            {
                throw new DllNotFoundException(
                    $"Unable to load native library for your platform ({NativeLibrary.GetPlatformName()}). " +
                    $"Looking for {NativeLibrary.GetLibraryName()} in {NativeLibrary.GetNativeLibraryPath()}. " +
                    $"Previous load error (if any): {LibraryLoadError}", ex);
            }
        }

        // Import the native function from the C++ library
        [DllImport("machineid_dotnet", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr MachineHash();

        // Import the function to free the string allocated by the C++ code
        [DllImport("machineid_dotnet", CallingConvention = CallingConvention.Cdecl)]
        private static extern void FreeMachineHashString(IntPtr ptr);
    }
}
