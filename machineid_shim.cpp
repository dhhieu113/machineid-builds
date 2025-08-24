// machineid_shim.cpp
// Build as part of your native project and link against the existing machineid library.

#include <cstring>
#include <cstdlib>

extern "C" {

// If your original library already has a C-style function, call it here.
// Adjust the symbol name below (machineid_get_string) to your project's function.
// Example patterns you may need to change:
//   - const char* get_machine_id();
//   - std::string machineid::get();  -> then convert to c string.

// ---- Example 1: if original exports `const char* machine_id()` (C style) ----
// extern const char* machine_id();
// const char* machineid_get_string() { return machine_id(); }

// ---- Example 2: if original uses C++ API returning std::string (more common) ----
// Suppose original has:
//    namespace machineid { std::string get(); }
// Then convert and return heap-allocated C string below.

#ifdef HAS_CPP_API
#include "machineid_cpp_header.h" // replace with actual header

const char* machineid_get_string() {
    try {
        std::string s = machineid::get();
        char* out = static_cast<char*>(std::malloc(s.size() + 1));
        if (!out) return nullptr;
        std::memcpy(out, s.c_str(), s.size()+1);
        return out;
    } catch(...) {
        return nullptr;
    }
}
#else
// Generic fallback: attempt to call known symbol names via weak linking is complex.
// If unknown, provide a stub to avoid crash — replace with real call.
const char* machineid_get_string() {
    const char* stub = "unknown-machine-id";
    char* out = static_cast<char*>(std::malloc(std::strlen(stub) + 1));
    if (!out) return nullptr;
    std::strcpy(out, stub);
    return out;
}
#endif

// Free method that C# will call to free heap memory allocated above.
void machineid_free_string(char* p) {
    if (p) std::free(p);
}

} // extern "C"
