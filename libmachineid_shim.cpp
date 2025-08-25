#include "machineid/machineid.h"
#include <cstring>

#ifdef _WIN32
#define EXPORT __declspec(dllexport)
#else
#define EXPORT
#endif

extern "C" {
    // This function returns a C-compatible string pointer that can be marshaled to .NET
    EXPORT const char* MachineHash() {
        std::string hash = machineid::machineHash();
#ifdef _WIN32
        // Allocate memory for the string that will be returned to .NET
        char* result = new char[hash.length() + 1];
        strcpy_s(result, hash.length() + 1, hash.c_str());
        return result;
#else
        // Allocate memory for the string that will be returned to .NET
        char* result = new char[hash.length() + 1];
        strcpy(result, hash.c_str());
        return result;
#endif
    }

    // This function frees the memory allocated by MachineHash
    EXPORT void FreeMachineHashString(const char* ptr) {
        delete[] ptr;
    }
}
