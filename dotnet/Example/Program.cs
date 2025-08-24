using MachineId;
using System;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            // Get and print the machine hash
            string machineHash = MachineIdentifier.GetMachineHash();
            Console.WriteLine($"Machine Hash: {machineHash}");
        }
    }
}
