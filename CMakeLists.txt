cmake_minimum_required(VERSION 3.10)
project(raytracer)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED True)

include_directories(include ./include)

set(SOURCES
    raytracer/main.cpp
    raytracer/glad.c
)

find_package(glfw3 REQUIRED)
if(WIN32)
    link_directories(${CMAKE_SOURCE_DIR}/lib/Windows)
elseif(APPLE)
    link_directories(${CMAKE_SOURCE_DIR}/lib/MacOS)
elseif(UNIX)
    link_directories(${CMAKE_SOURCE_DIR}/lib/Linux)
endif()

add_executable(raytracer ${SOURCES})

if(WIN32)
    target_link_libraries(raytracer glfw3 opengl32)
elseif(APPLE)
    target_link_libraries(raytracer glfw "-framework OpenGL")
elseif(UNIX)
    target_link_libraries(raytracer glfw GL X11 pthread Xrandr Xi)
endif()

add_custom_command(
    TARGET raytracer POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy_if_different
        ${CMAKE_SOURCE_DIR}/raytracer/raytracer_vertex.vs
        ${CMAKE_SOURCE_DIR}/raytracer/raytracer_fragment.fs
        $<TARGET_FILE_DIR:raytracer>
)