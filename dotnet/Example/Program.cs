using MachineId;

// Get and print the machine hash
string machineHash = MachineIdentifier.GetMachineHash();
Console.WriteLine($"Machine Hash: {machineHash}");
