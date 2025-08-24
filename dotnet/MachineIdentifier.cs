using System;
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
