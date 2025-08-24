[![CI](https://github.com/dhhieu113/machineid-builds/actions/workflows/ci.yml/badge.svg)](https://github.com/dhhieu113/machineid-builds/actions/workflows/ci.yml)

[![Dhhieu113.MachineId ](https://img.shields.io/nuget/v/Dhhieu113.MachineId)](https://www.nuget.org/packages/Dhhieu113.MachineId)

# MachineId .NET Binding

This is a .NET binding for the machineid C++ library. It allows you to get a unique machine identifier in .NET applications.

## Prerequisites

- .NET SDK 6.0 or higher
- C++ build tools (for building the native library)

## Building

1. First, build the native library using CMake:

```bash
mkdir build && cd build
cmake ..
cmake --build . --config Release
```

2. Build the .NET binding:

```bash
cd dotnet
dotnet build -c Release
```

## Usage

```csharp
using MachineId;

// Get and print the machine hash
string machineHash = MachineIdentifier.GetMachineHash();
Console.WriteLine($"Machine Hash: {machineHash}");
```

## Example

Check the Example project for a complete demonstration.

## NuGet Package

When the project is built, a NuGet package is generated in the output directory. You can use this package in other .NET projects.
