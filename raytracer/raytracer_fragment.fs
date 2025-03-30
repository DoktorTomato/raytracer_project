#version 330 core

out vec4 FragColor;
in vec2 fragCoord;

uniform vec3 cameraPos;
uniform mat4 view;
uniform mat4 projection;

const int amountOfSpheres = 10;

struct Ray
{
	vec3 origin;
	vec3 direction;
};

struct Sphere
{
	vec3 center;
	float radius;
	vec4 color;
};

struct Triangle
{
    vec3 v1;
    vec3 v2;
    vec3 v3;
    vec4 color;
};

struct Cube {
    vec3 center;
    float size;
    vec4 color;
    Triangle triangles[12];
};

Cube createCube(vec3 center, float size, vec4 color) {
    Cube cube;
    cube.center = center;
    cube.size = size;
    cube.color = color;

    float hs = size * 0.5;

    vec3 v0 = center + vec3(-hs, -hs, -hs);
    vec3 v1 = center + vec3( hs, -hs, -hs);
    vec3 v2 = center + vec3( hs,  hs, -hs);
    vec3 v3 = center + vec3(-hs,  hs, -hs);
    vec3 v4 = center + vec3(-hs, -hs,  hs);
    vec3 v5 = center + vec3( hs, -hs,  hs);
    vec3 v6 = center + vec3( hs,  hs,  hs);
    vec3 v7 = center + vec3(-hs,  hs,  hs);

    cube.triangles[0]  = Triangle(v0, v1, v2, color);
    cube.triangles[1]  = Triangle(v0, v2, v3, color);
    
    cube.triangles[2]  = Triangle(v1, v5, v6, color);
    cube.triangles[3]  = Triangle(v1, v6, v2, color);
    
    cube.triangles[4]  = Triangle(v5, v4, v7, color);
    cube.triangles[5]  = Triangle(v5, v7, v6, color);
    
    cube.triangles[6]  = Triangle(v4, v0, v3, color);
    cube.triangles[7]  = Triangle(v4, v3, v7, color);
    
    cube.triangles[8]  = Triangle(v3, v2, v6, color);
    cube.triangles[9]  = Triangle(v3, v6, v7, color);
    
    cube.triangles[10] = Triangle(v4, v5, v1, color);
    cube.triangles[11] = Triangle(v4, v1, v0, color);

    return cube;
}

bool tracingTriagle(Ray ray, Triangle triangle,out float t, out vec4 hitColor, out float tClosest) {
    const float eps = 0.001;

    vec3 edge1 = triangle.v2 - triangle.v1;
    vec3 edge2 = triangle.v3 - triangle.v1;
    vec3 ray_cross_e2 = cross(ray.direction, edge2);
    float det = dot(edge1, ray_cross_e2);

    if (det > -eps && det < eps) return false;

    float inv_det = 1.0 / det;
    vec3 ray_to_v1 = ray.origin - triangle.v1;
    float u = inv_det * dot(ray_to_v1, ray_cross_e2);

    if (u < 0.0 || u > 1.0) return false;

    vec3 ray_cross_e1 = cross(ray_to_v1, edge1);
    float v = inv_det * dot(ray.direction, ray_cross_e1);

    if (v < 0.0 || u + v > 1.0) return false;

    t = inv_det * dot(edge2, ray_cross_e1);
    
    if (t > eps && t < tClosest) {
        tClosest = t;
        hitColor = triangle.color;
        return true;
    }
    return false;
}

bool tracingSphere(Ray ray, Sphere sphere, out float t, out vec4 hitColor, out float tClosest)
{
    vec3 oc = ray.origin - sphere.center;
    float a = dot(ray.direction, ray.direction);
    float b = 2.0f * dot(oc, ray.direction);
    float c = dot(oc, oc) - sphere.radius * sphere.radius;

    float disc = (b * b) - (4.0f * a * c);

    if (disc >= 0.0) {
        float sqrtDisc = sqrt(disc);
        float t0 = (-b - sqrtDisc) / (2.0 * a);
        float t1 = (-b + sqrtDisc) / (2.0 * a);
        float t = (t0 > 0.0) ? t0 : t1;
        if(t > 0.0 && t < tClosest) {
            tClosest = t;
            hitColor = sphere.color;
            return true;
        }
    }
    return false;
}

void rayTrace(Ray ray, Sphere[amountOfSpheres] spheres, int amountOfSpheres, Triangle[1] triangles, int amountOfTri, Cube[1] cubes, int amountOfCubes) {
    float tClosest = 1e8;
    vec4 hitColor = vec4(0.0);
    float t;

    for (int i=0; i<amountOfSpheres; i++)
	{
		if (tracingSphere(ray, spheres[i], t, hitColor, tClosest) && t < tClosest) {
            FragColor = hitColor;
        }
    }

    for (int i=0; i<amountOfTri; i++)
	{
		if (tracingTriagle(ray, triangles[i], t, hitColor, tClosest) && t < tClosest) {
            FragColor = hitColor;
        }
    }

    for (int i=0; i<amountOfCubes; i++)
	{
        for (int j=0; j<12; j++)
        {
            if (tracingTriagle(ray, cubes[i].triangles[j], t, hitColor, tClosest) && t < tClosest) {
                FragColor = hitColor;
            }
        }
    }
    
    if (tClosest < 1e8) {
        FragColor = hitColor;
    } else {
        FragColor = vec4(0.0, 0.0, 0.0, 1.0);
    }
}

float rand(float seed) {
    return fract(sin(seed * 12.9898));
}

Sphere createSphere(int index) {
    float seed = float(index) * 1.1234;
    
    float x = rand(seed) * 2.0 - 1.0;
    float y = rand(seed + 1.0) - rand(seed*1.5);
    float z = rand(seed + 3.0) * rand(seed*1.5);
    float radius = 0.2 + rand(seed + 5.0) * 0.1;

    vec4 color = vec4(rand(seed + 4.0), rand(seed + 5.0), rand(seed + 6.0), 1.0);

    Sphere s;
    s.center = vec3(x, y, z);
    s.radius = radius;
    s.color = color;
    return s;
}

void main()
{
    vec2 ndc = fragCoord * 2.0 - 1.0;

    vec4 clipSpace = vec4(ndc, -1.0, 1.0);
    vec4 eyeSpace = inverse(projection) * clipSpace;
    eyeSpace = vec4(eyeSpace.xy, -1.0, 0.0); 

    vec3 rayDirWorld = normalize((inverse(view) * eyeSpace).xyz);

    Ray ray;
    ray.origin = cameraPos;
    ray.direction = rayDirWorld;

    Sphere spheres[amountOfSpheres];
    for (int i = 0; i < amountOfSpheres; i++) {
        spheres[i] = createSphere(i);
    }

    Triangle tri;
    tri.v1 = vec3(0.5, 0.5, 0.5);
    tri.v2 = vec3(0.5, 0.7, 0.6);
    tri.v3 = vec3(0.1, 0.5, 0.4);
    tri.color = vec4(1.0, 0.0, 0.0, 1.0);

    Triangle triangles[1];
    triangles[0] = tri;

    Cube cube = createCube(vec3(0.0, 0.0, 0.0), 0.5, vec4(0.0, 1.0, 0.0, 0.3));

    Cube cubes[1];
    cubes[0] = cube;

    rayTrace(ray, spheres, amountOfSpheres, triangles, 1, cubes, 1);
}
