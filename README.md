# Project: Concurent real time GPU Raytracer
Authors (team): [Bykov Danylo](https://github.com/DanyaBykov), [Ivan Shevchuk](https://github.com/DoktorTomato)

Mentor: [Ostap Trush](https://github.com/Adeon18)

## Prerequisites

- OpenGL (at least 4.3)
- CMake
- Make
- g++
- GLFW
- GLAD
- GLM
(GLFW, GLM and GLAD are inlcuded in the project but be aware)

### Compilation (Availble for Windows and MacOS)

1. cd raytraycer
2. mkdir build
3. cd build
4. cmake ..
5. make (or alternatively cmake --build .)

### Usage

In the build folder run executable compiled earlier.
1. cd build
2. ./raytracer

### Results

After running the raytracer you will see a window opened in which some figures will be traced. You can move and look around in this space using your mouse and WASD buttons on your keyboard.
You can also change amount of objects in space by changing values of variables (amountOfSpheres, amountOfCubes, amountOfLights) in raytracer_fragment.fs.

Here are some screenshots in the program:

![](screenshots/scene.png)

![](screenshots/tree.png)

![](screenshots/photo_1_2025-05-13_10-27-44.jpg)

![](screenshots/photo_2_2025-05-13_10-27-44.jpg)

![](screenshots/photo_3_2025-05-13_10-27-44.jpg)

![](screenshots/photo_4_2025-05-13_10-27-44.jpg)

![](screenshots/image-4.png)

![](screenshots/image-5.png)
