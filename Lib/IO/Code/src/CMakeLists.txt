cmake_minimum_required(VERSION 3.0)

set(FREEPIEIO_INCL_DIRS ${CMAKE_CURRENT_LIST_DIR}/../include/)
   
# Source files that are needed for the shared library
file(GLOB FREEPIEIO_SRC
    "${CMAKE_CURRENT_LIST_DIR}/*.cpp"
    "${CMAKE_CURRENT_LIST_DIR}/*.h"
    "${CMAKE_CURRENT_LIST_DIR}/../cfg/freepie_io${ARCH_SUFFIX}.def"
)

# shared library
add_library(freepie_io SHARED ${FREEPIEIO_SRC})
target_include_directories(freepie_io PUBLIC ${FREEPIEIO_INCL_DIRS} ${CMAKE_CURRENT_BINARY_DIR})

# Make sure to append the appropriate architecture suffix to the library
set_target_properties(freepie_io PROPERTIES OUTPUT_NAME "freepie_io${ARCH_SUFFIX}")

# Install
install(TARGETS freepie_io
    CONFIGURATIONS Debug
    RUNTIME DESTINATION ${FREEPIE_DEBUG_INSTALL_PATH}
    LIBRARY DESTINATION ${FREEPIE_DEBUG_INSTALL_PATH}
    ARCHIVE DESTINATION ${FREEPIE_DEBUG_INSTALL_PATH})
install(TARGETS freepie_io
    CONFIGURATIONS Release
    RUNTIME DESTINATION ${FREEPIE_RELEASE_INSTALL_PATH}
    LIBRARY DESTINATION ${FREEPIE_RELEASE_INSTALL_PATH}
    ARCHIVE DESTINATION ${FREEPIE_RELEASE_INSTALL_PATH})