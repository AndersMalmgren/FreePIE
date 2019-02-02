cmake_minimum_required(VERSION 3.0)
project(FreePieIO VERSION 0.0.1)

LIST(APPEND CMAKE_MODULE_PATH "${CMAKE_CURRENT_LIST_DIR}/cmake")
IF (NOT MSVC)
    set(CMAKE_CXX_FLAGS "-std=c++11")
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -Wno-switch")
ENDIF()

set_property(GLOBAL PROPERTY USE_FOLDERS ON)

# Shared architecture label used for install folder locations
if (${CMAKE_C_SIZEOF_DATA_PTR} EQUAL 8)
    set(ARCH_LABEL "Win64")   
    set(FREEPIE_DEBUG_INSTALL_PATH "${CMAKE_CURRENT_LIST_DIR}/Binaries/Debug-DllMultiThreadRuntime-x64")
    set(FREEPIE_RELEASE_INSTALL_PATH "${CMAKE_CURRENT_LIST_DIR}/Binaries/Release-DllMultiThreadRuntime-x64")
else()
    set(ARCH_LABEL "Win32")
    set(FREEPIE_DEBUG_INSTALL_PATH "${CMAKE_CURRENT_LIST_DIR}/Binaries/Debug-DllMultiThreadRuntime")
    set(FREEPIE_RELEASE_INSTALL_PATH "${CMAKE_CURRENT_LIST_DIR}/Binaries/Release-DllMultiThreadRuntime")
endif()

# Step into the subdirectories
add_subdirectory(Code)