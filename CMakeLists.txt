cmake_minimum_required(VERSION 3.5)

project(machineid LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 11)
# Enable PIC by default for shared libraries
set(CMAKE_POSITION_INDEPENDENT_CODE ON)

# ---- STATIC LIBRARY ----
add_library(machineid_static STATIC src/machineid/machineid.cpp)
# Enable Position Independent Code for the static library so it can be used in shared libraries
set_property(TARGET machineid_static PROPERTY POSITION_INDEPENDENT_CODE ON)
target_include_directories(machineid_static
    PRIVATE src
    PUBLIC include)
if (WIN32 OR MINGW OR MSYS)
    target_link_libraries(machineid_static PRIVATE iphlpapi)
endif()

# ---- SHARED LIBRARY ----
add_library(machineid_shared SHARED src/machineid/machineid.cpp)
target_include_directories(machineid_shared
    PRIVATE src
    PUBLIC include)
if (WIN32 OR MINGW OR MSYS)
    target_link_libraries(machineid_shared PRIVATE iphlpapi)
endif()

# Ensure proper output names (so both are "machineid", not suffixed)
set_target_properties(machineid_static PROPERTIES OUTPUT_NAME "machineid")
set_target_properties(machineid_shared PROPERTIES OUTPUT_NAME "machineid")

# On Windows, differentiate import libs and DLLs
if (WIN32)
    set_target_properties(machineid_shared PROPERTIES
        RUNTIME_OUTPUT_NAME "machineid"
        ARCHIVE_OUTPUT_NAME "machineid"
        LIBRARY_OUTPUT_NAME "machineid")
endif()

# ---- TESTS ----
if (BUILD_TESTING)
    add_executable(test tests/test.cpp)
    target_link_libraries(test PRIVATE machineid_static) # link to static by default
endif()

# ---- .NET BINDING SHIM ----
add_library(libmachineid SHARED libmachineid_shim.cpp)
target_include_directories(libmachineid
    PRIVATE src
    PUBLIC include)

# Link against the static library to ensure all symbols are available
target_link_libraries(libmachineid PRIVATE machineid_static)

# Add platform-specific libraries
if (WIN32 OR MINGW OR MSYS)
    target_link_libraries(libmachineid PRIVATE iphlpapi)
endif()
if (APPLE)
    target_link_libraries(libmachineid PRIVATE "-framework CoreFoundation" "-framework IOKit")
endif()
if (UNIX AND NOT APPLE AND NOT MINGW AND NOT MSYS)
    target_link_libraries(libmachineid PRIVATE dl rt)
endif()

# Set output name for the .NET binding
set_target_properties(libmachineid PROPERTIES OUTPUT_NAME "libmachineid")

# On Windows/MinGW, ensure proper naming
if(WIN32 OR MINGW OR MSYS)
    set_target_properties(libmachineid PROPERTIES 
        RUNTIME_OUTPUT_NAME "libmachineid"
        LIBRARY_OUTPUT_NAME "libmachineid"
        ARCHIVE_OUTPUT_NAME "libmachineid")
endif()
