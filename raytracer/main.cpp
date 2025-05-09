#include <glad/glad.h>
#include <GLFW/glfw3.h>
#include <iostream>
#include <fstream>
#include <sstream>
#include <string>
#include <chrono>
#include <vector>
#include <glm/glm.hpp>
#include <glm/gtc/matrix_transform.hpp>
#include <glm/gtc/type_ptr.hpp>
#include <random>

const unsigned int SCR_WIDTH = 1920;
const unsigned int SCR_HEIGHT = 1080;

glm::vec3 cameraPos = glm::vec3(0.0f, 0.0f,  3.0f);
glm::vec3 cameraFront = glm::vec3(0.0f, 0.0f, -1.0f);
glm::vec3 cameraUp = glm::vec3(0.0f, 1.0f,  0.0f);

bool firstMouse = true;
float lastX = SCR_WIDTH / 2.0f;
float lastY = SCR_HEIGHT / 2.0f;
float yaw = -90.0f;
float pitch = 0.0f;
float fov = 45.0f;

float deltaTime = 0.0f;
float lastFrame = 0.0f;

struct alignas(16) Sphere {
	glm::vec3 center;
	float radius;
	glm::vec4 color;
};

struct alignas(16) Triangle
{
    glm::vec3 v1;
    glm::vec3 v2;
    glm::vec3 v3;
    glm::vec4 color;

    Triangle(glm::vec3 vertex1, glm::vec3 vertex2, glm::vec3 vertex3, glm::vec4 col)
        : v1(vertex1), v2(vertex2), v3(vertex3), color(col) {
    }
};

struct alignas(16) Object {
	glm::vec4 color;
	glm::vec3 position;
    float scale;
	int numTriangles;
	int trianglesOffset;
};

Object createCube(glm::vec3 pos, float size, glm::vec4 color, std::vector<Triangle>& triangles) {
    Object cube;
    cube.position = pos;
    cube.scale = size;
    cube.color = color;

    glm::vec3 v0 = pos;
    glm::vec3 v1 = pos + glm::vec3(size, 0, 0);
    glm::vec3 v2 = pos + glm::vec3(size, size, 0);
    glm::vec3 v3 = pos + glm::vec3(0, size, 0);
    glm::vec3 v4 = pos + glm::vec3(0, 0, size);
    glm::vec3 v5 = pos + glm::vec3(size, 0, size);
    glm::vec3 v6 = pos + glm::vec3(size, size, size);
    glm::vec3 v7 = pos + glm::vec3(0, size, size);

    triangles.push_back(Triangle(v0, v1, v3, color));
    //triangles.push_back(Triangle(v1, v2, v3, color));

    /*triangles.push_back(Triangle(v1, v5, v6, color));
    triangles.push_back(Triangle(v1, v6, v2, color));   

    triangles.push_back(Triangle(v5, v4, v7, color));
    triangles.push_back(Triangle(v5, v7, v6, color));

    triangles.push_back(Triangle(v4, v0, v3, color));
    triangles.push_back(Triangle(v4, v3, v7, color));

    triangles.push_back(Triangle(v3, v2, v6, color));
    triangles.push_back(Triangle(v3, v6, v7, color));

    triangles.push_back(Triangle(v4, v5, v1, color));
    triangles.push_back(Triangle(v4, v1, v0, color));*/

    cube.numTriangles = 12;
    cube.trianglesOffset = static_cast<int>(triangles.size()) - 12;

    return cube;
}

std::string loadShaderSource(const char* filepath) {
    std::ifstream file(filepath);
    std::stringstream buffer;
    buffer << file.rdbuf();
    return buffer.str();
}

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

void mouse_callback(GLFWwindow* window, double xpos, double ypos)
{
    if (firstMouse)
    {
        lastX = xpos;
        lastY = ypos;
        firstMouse = false;
    }
  
    float xoffset = xpos - lastX;
    float yoffset = lastY - ypos; 
    lastX = xpos;
    lastY = ypos;

    float sensitivity = 0.1f;
    xoffset *= sensitivity;
    yoffset *= sensitivity;

    yaw += xoffset;
    pitch += yoffset;

    if(pitch > 89.0f)
        pitch = 89.0f;
    if(pitch < -89.0f)
        pitch = -89.0f;

    glm::vec3 direction;
    direction.x = cos(glm::radians(yaw)) * cos(glm::radians(pitch));
    direction.y = sin(glm::radians(pitch));
    direction.z = sin(glm::radians(yaw)) * cos(glm::radians(pitch));
    cameraFront = glm::normalize(direction);
}

void scroll_callback(GLFWwindow* window, double xoffset, double yoffset)
{
    fov -= (float)yoffset;
    if (fov < 1.0f)
        fov = 1.0f;
    if (fov > 45.0f)
        fov = 45.0f; 
}

void processInput(GLFWwindow *window)
{
    if (glfwGetKey(window, GLFW_KEY_ESCAPE) == GLFW_PRESS)
        glfwSetWindowShouldClose(window, true);

    float cameraSpeed = static_cast<float>(2.5 * deltaTime);
    if (glfwGetKey(window, GLFW_KEY_W) == GLFW_PRESS)
        cameraPos += cameraSpeed * cameraFront;
    if (glfwGetKey(window, GLFW_KEY_S) == GLFW_PRESS)
        cameraPos -= cameraSpeed * cameraFront;
    if (glfwGetKey(window, GLFW_KEY_A) == GLFW_PRESS)
        cameraPos -= glm::normalize(glm::cross(cameraFront, cameraUp)) * cameraSpeed;
    if (glfwGetKey(window, GLFW_KEY_D) == GLFW_PRESS)
        cameraPos += glm::normalize(glm::cross(cameraFront, cameraUp)) * cameraSpeed;
}

void checkShaderCompilation(GLuint shader, const std::string& shaderSource, GLFWwindow* window) {
    int success;
    char infoLog[1024];
    glGetShaderiv(shader, GL_COMPILE_STATUS, &success);
    if (!success) {
        glGetShaderInfoLog(shader, 1024, NULL, infoLog);
        std::cout << "ERROR::SHADER::COMPILATION_FAILED\n" << infoLog << std::endl;
        std::cout << "Shader Source:\n" << shaderSource << std::endl; // Print the shader source
        glfwSetWindowShouldClose(window, true);
    }
}


int main()
{
    glfwInit();
    glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 4);
    glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);
    glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);

#ifdef __APPLE__
    glfwWindowHint(GLFW_OPENGL_FORWARD_COMPAT, GL_TRUE);
#endif

    GLFWwindow* window = glfwCreateWindow(SCR_WIDTH, SCR_HEIGHT, "GPURaytracer", NULL, NULL);
    if (window == NULL)
    {
        std::cout << "Failed to create GLFW window" << std::endl;
        glfwTerminate();
        return -1;
    }
    glfwMakeContextCurrent(window);
    glfwSetFramebufferSizeCallback(window, framebuffer_size_callback);

    if (!gladLoadGLLoader((GLADloadproc)glfwGetProcAddress))
    {
        std::cout << "Failed to initialize GLAD" << std::endl;
        return -1;
    }

    glfwSetInputMode(window, GLFW_CURSOR, GLFW_CURSOR_DISABLED);  
    glfwSetCursorPosCallback(window, mouse_callback); 
    glfwSetScrollCallback(window, scroll_callback);

    unsigned int vertexShader = glCreateShader(GL_VERTEX_SHADER);
    std::string vertCode = loadShaderSource("raytracer_vertex.vs");
    const char* vertexShaderSource = vertCode.c_str();
    glShaderSource(vertexShader, 1, &vertexShaderSource, NULL);
    glCompileShader(vertexShader);
    checkShaderCompilation(vertexShader, vertCode, window);

    int success;
    char infoLog[512];
    glGetShaderiv(vertexShader, GL_COMPILE_STATUS, &success);
    if (!success)
    {
        glGetShaderInfoLog(vertexShader, 512, NULL, infoLog);
        std::cout << "ERROR::SHADER::VERTEX::COMPILATION_FAILED\n" << infoLog << std::endl;
    }

    unsigned int fragmentShader = glCreateShader(GL_FRAGMENT_SHADER);
    std::string fragCode = loadShaderSource("raytracer_fragment.fs");
    const char* fragmentShaderSource = fragCode.c_str();
    glShaderSource(fragmentShader, 1, &fragmentShaderSource, NULL);
    glCompileShader(fragmentShader);
    checkShaderCompilation(fragmentShader, fragCode, window);

    glGetShaderiv(fragmentShader, GL_COMPILE_STATUS, &success);
    if (!success)
    {
        glGetShaderInfoLog(fragmentShader, 512, NULL, infoLog);
        std::cout << "ERROR::SHADER::FRAGMENT::COMPILATION_FAILED\n" << infoLog << std::endl;
    }

    unsigned int shaderProgram = glCreateProgram();
    glAttachShader(shaderProgram, vertexShader);
    glAttachShader(shaderProgram, fragmentShader);
    glLinkProgram(shaderProgram);

    glGetProgramiv(shaderProgram, GL_LINK_STATUS, &success);
    if (!success) {
        glGetProgramInfoLog(shaderProgram, 512, NULL, infoLog);
        std::cout << "ERROR::SHADER::PROGRAM::LINKING_FAILED\n" << infoLog << std::endl;
    }
    glDeleteShader(vertexShader);
    glDeleteShader(fragmentShader);


    float vertices[] = {
        -1.0f, -1.0f, 0.0f, // left  
         3.0f, -1.0f, 0.0f, // right 
         -1.0f,  3.0f, 0.0f  // top   
    };

    unsigned int VBO, VAO;
    glGenVertexArrays(1, &VAO);
    glGenBuffers(1, &VBO);

    glBindVertexArray(VAO);
    glBindBuffer(GL_ARRAY_BUFFER, VBO);
    glBufferData(GL_ARRAY_BUFFER, sizeof(vertices), vertices, GL_DYNAMIC_DRAW);

    glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 3 * sizeof(float), (void*)0);
    glEnableVertexAttribArray(0);

    glBindBuffer(GL_ARRAY_BUFFER, 0);
    glBindVertexArray(0);

    auto lastTime = std::chrono::high_resolution_clock::now();
    int frameCount = 0;
    std::vector<float> frameTimes;
    
    std::vector<Triangle> triangles;
	std::vector<Object> objects;
	std::vector<Sphere> spheres;

    int numCubes = 1;
	int numSpheres = 0;

    std::mt19937 rng(std::random_device{}());

	glm::vec3 minPos(-10.0f, -10.0f, -10.0f);
	glm::vec3 maxPos(10.0f, 10.0f, 10.0f);

    std::uniform_real_distribution<float> randX(minPos.x, maxPos.x);
    std::uniform_real_distribution<float> randY(minPos.y, maxPos.y);
    std::uniform_real_distribution<float> randZ(minPos.z, maxPos.z);
    std::uniform_real_distribution<float> randRadius(0.1f, 2.0f);
    std::uniform_real_distribution<float> randColor(0.0f, 1.0f);

    for (int i = 0; i < numSpheres; ++i) {
        Sphere s;
        s.center = glm::vec3(randX(rng), randY(rng), randZ(rng));
        s.radius = randRadius(rng);
        s.color = glm::vec4(randColor(rng), randColor(rng), randColor(rng), 1.0f);
        spheres.push_back(s);
    }

    for (int i = 0; i < numCubes; ++i) {
        int triIndex = static_cast<int>(triangles.size());
        Object cube = createCube(glm::vec3(randX(rng), randY(rng), randZ(rng)), 1, glm::vec4(randColor(rng), randColor(rng), randColor(rng), 1.0f), triangles);
        objects.push_back(cube);
    }

    while (!glfwWindowShouldClose(window))
    {
        auto frameStartTime = std::chrono::high_resolution_clock::now();

        float currentFrame = static_cast<float>(glfwGetTime());
        deltaTime = currentFrame - lastFrame;
        lastFrame = currentFrame;

        escExit(window);
        processInput(window);

        glClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        glClear(GL_COLOR_BUFFER_BIT);

        GLuint triangleSSBO, objectSSBO, sphereSSBO;

        

        glGenBuffers(1, &triangleSSBO);
        glBindBuffer(GL_SHADER_STORAGE_BUFFER, triangleSSBO);
        glBufferData(GL_SHADER_STORAGE_BUFFER, triangles.size() * sizeof(Triangle), triangles.data(), GL_STATIC_DRAW);
        glBindBufferBase(GL_SHADER_STORAGE_BUFFER, 0, triangleSSBO); // Binding 0

        glGenBuffers(1, &objectSSBO);
        glBindBuffer(GL_SHADER_STORAGE_BUFFER, objectSSBO);
        glBufferData(GL_SHADER_STORAGE_BUFFER, objects.size() * sizeof(Object), objects.data(), GL_STATIC_DRAW);
        glBindBufferBase(GL_SHADER_STORAGE_BUFFER, 1, objectSSBO); // Binding 1

        glGenBuffers(1, &sphereSSBO);
        glBindBuffer(GL_SHADER_STORAGE_BUFFER, sphereSSBO);
        glBufferData(GL_SHADER_STORAGE_BUFFER, spheres.size() * sizeof(Sphere), spheres.data(), GL_STATIC_DRAW);
        glBindBufferBase(GL_SHADER_STORAGE_BUFFER, 2, sphereSSBO); // Binding 2

        glUseProgram(shaderProgram);

        GLint locNumCubes = glGetUniformLocation(shaderProgram, "numCubes");
        glUniform1i(locNumCubes, static_cast<GLint>(objects.size()));

        GLint locNumSpheres = glGetUniformLocation(shaderProgram, "numSpheres");
        glUniform1i(locNumSpheres, static_cast<GLint>(spheres.size()));
        
        glm::mat4 view = glm::lookAt(cameraPos, cameraPos + cameraFront, cameraUp);
        glm::mat4 projection = glm::perspective(glm::radians(fov), (float)SCR_WIDTH / (float)SCR_HEIGHT, 0.1f, 100.0f);

        unsigned int viewLoc = glGetUniformLocation(shaderProgram, "view");
        glUniformMatrix4fv(viewLoc, 1, GL_FALSE, glm::value_ptr(view));

        unsigned int projLoc = glGetUniformLocation(shaderProgram, "projection");
        glUniformMatrix4fv(projLoc, 1, GL_FALSE, glm::value_ptr(projection));

        unsigned int camPosLoc = glGetUniformLocation(shaderProgram, "cameraPos");
        glUniform3fv(camPosLoc, 1, glm::value_ptr(cameraPos));

        glBindVertexArray(VAO);
        glDrawArrays(GL_TRIANGLES, 0, 3);

        glfwSwapBuffers(window);
        glfwPollEvents();

        auto frameEndTime = std::chrono::high_resolution_clock::now();
        std::chrono::duration<float> frameTime = frameEndTime - frameStartTime;
        frameTimes.push_back(frameTime.count() * 1000.0f);

        frameCount++;
        auto currentTime = std::chrono::high_resolution_clock::now();
        std::chrono::duration<float> elapsed = currentTime - lastTime;
        if (elapsed.count() >= 1.0f) {
            std::cout << "FPS: " << frameCount << std::endl;
            frameCount = 0;
            lastTime = currentTime;
        }
    }

    //float totalFrameTime = 0.0f;
    //for (const auto& time : frameTimes) {
    //    totalFrameTime += time;
    //}
    //float averageFrameTime = totalFrameTime / frameTimes.size();
    //std::cout << "Average Frame Time (ms): " << averageFrameTime << std::endl;

    //std::ofstream outFile("../performance_metrics_20.csv");
    //outFile << "Frame,FrameTime_ms,Cubes" << std::endl;
    //for (size_t i = 0; i < frameTimes.size(); ++i) {
    //    outFile << i + 1 << "," << frameTimes[i] << "," << 20 << std::endl;
    //}
    //outFile.close();

    glDeleteVertexArrays(1, &VAO);
    glDeleteBuffers(1, &VBO);
    glDeleteProgram(shaderProgram);

    glfwTerminate();
    return 0;
}