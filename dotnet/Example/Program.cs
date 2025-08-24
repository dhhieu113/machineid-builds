using MachineId;
using System;
using System.IO;
using System.Reflection;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("MachineId Example");
                Console.WriteLine("----------------");
                Console.WriteLine($"Current Directory: {Environment.CurrentDirectory}");
                Console.WriteLine($"Assembly Location: {typeof(Program).Assembly.Location}");
                Console.WriteLine($"Assembly Directory: {Path.GetDirectoryName(typeof(Program).Assembly.Location)}");
                Console.WriteLine($"OS: {Environment.OSVersion.Platform}");
                Console.WriteLine($"Architecture: {System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}");
                Console.WriteLine($".NET Version: {Environment.Version}");
                
                Console.WriteLine("\nSearching for native libraries...");
                ListFiles(Path.GetDirectoryName(typeof(Program).Assembly.Location), "*.dll", "*.so", "*.dylib");
                
                Console.WriteLine("\nGetting machine hash...");
                // Get and print the machine hash
                string machineHash = MachineIdentifier.GetMachineHash();
                Console.WriteLine($"Machine Hash: {machineHash}");
                Console.WriteLine("Success!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
                if (ex.InnerException != null)
                    Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
                
                Console.WriteLine("\nStack Trace:");
                Console.WriteLine(ex.StackTrace);
                
                Console.WriteLine("\nLibrary Load Error (if any):");
                Console.WriteLine(MachineIdentifier.LibraryLoadError ?? "None");
                
                Console.WriteLine("\nAvailable DLLs:");
                ListFiles(Path.GetDirectoryName(typeof(Program).Assembly.Location), "*.dll");
            }
        }
        
        static void ListFiles(string directory, params string[] patterns)
        {
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
            {
                Console.WriteLine($"  Directory not found: {directory}");
                return;
            }
            
            Console.WriteLine($"  Contents of {directory}:");
            
            try
            {
                foreach (var pattern in patterns)
                {
                    foreach (var file in Directory.GetFiles(directory, pattern, SearchOption.AllDirectories))
                    {
                        Console.WriteLine($"    {Path.GetRelativePath(directory, file)}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Error listing files: {ex.Message}");
            }
        }
    }
}
}
