#include <glad/glad.h>
#include <GLFW/glfw3.h>
#include <iostream>

void framebuffer_size_callback(GLFWwindow* window, int width, int height)
{
    //this function is changing viewport when window is resized
    glViewport(0, 0, width, height);
}

void escExit(GLFWwindow* window)
{
    if (glfwGetKey(window, GLFW_KEY_ESCAPE) == GLFW_PRESS)
        glfwSetWindowShouldClose(window, true);
}

int main()
{
    glfwInit(); // initializing glfw
    glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3); //changing GLFW context in this case we specify version of OpenGL
    glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);
    glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE); //specifying that we use core profile

    GLFWwindow* window = glfwCreateWindow(800, 600, "LearnOpenGL", NULL, NULL); // creating a window
    if (window == NULL) //checking if creation was successful
    {
        std::cout << "Failed to create GLFW window" << std::endl;
        glfwTerminate();
        return -1;
    }

    glfwMakeContextCurrent(window);

    if (!gladLoadGLLoader((GLADloadproc)glfwGetProcAddress))//checking if glad is initialized
    {
        std::cout << "Failed to initialize GLAD" << std::endl;
        return -1;
    }
    glViewport(0, 0, 800, 600);//creating viewport in current context
	glfwSetFramebufferSizeCallback(window, framebuffer_size_callback); //we're telling glfw that we use our function to change viewport

	while (!glfwWindowShouldClose(window)) //a loop that keeps window open until it's told to be closed
    {
		escExit(window);

        glClearColor(0.72, 0.45, 0.20, 0);
        glClear(GL_COLOR_BUFFER_BIT);

		glfwSwapBuffers(window);//drawing buffer
		glfwPollEvents();//checking for inputs
    }

	return 0;
}