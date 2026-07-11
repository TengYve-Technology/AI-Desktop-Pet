cmake_minimum_required(VERSION 3.20)

project(PetNative LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${PROJECT_BINARY_DIR}/bin)
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${PROJECT_BINARY_DIR}/bin)
set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY ${PROJECT_BINARY_DIR}/lib)

include_directories(${PROJECT_SOURCE_DIR}/include)

add_library(PetNative SHARED
    src/exports.cpp
)

target_compile_definitions(PetNative PRIVATE PET_NATIVE_EXPORTS)

if(WIN32)
    target_link_libraries(PetNative PRIVATE user32.lib dwmapi.lib shcore.lib)
endif()

if(MSVC)
    set_target_properties(PetNative PROPERTIES
        COMPILE_OPTIONS "/W4"
        LINK_FLAGS "/SUBSYSTEM:WINDOWS"
    )
endif()