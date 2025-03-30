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

bool tracingTriagle(Ray ray, Triangle triangle,out float t, out vec4 hitColor) {
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
    
    if (t > eps) {
        hitColor = triangle.color;
        return true;
    }
    return false;
}

bool tracingSphere(Ray ray, Sphere sphere, out float t, out vec4 hitColor)
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
        hitColor = sphere.color;
        return true;
    }
    return false;
}

void rayTrace(Ray ray, Sphere[amountOfSpheres] spheres, int amountOfSpheres, Triangle[1] triangles, int amountOfTri) {
    float tClosest = 1e8;
    vec4 hitColor = vec4(0.0);
    float t;

    for (int i=0; i<amountOfSpheres; i++)
	{
		if (tracingSphere(ray, spheres[i], t, hitColor) && t < tClosest) {
            tClosest = t;
            FragColor = hitColor;
        }
    }

    for (int i=0; i<amountOfTri; i++)
	{
		if (tracingTriagle(ray, triangles[i], t, hitColor) && t < tClosest) {
            tClosest = t;
            FragColor = hitColor;
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

    rayTrace(ray, spheres, amountOfSpheres, triangles, 1);
}