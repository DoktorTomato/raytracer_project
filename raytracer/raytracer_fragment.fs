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

void tracingRay(Ray ray, Sphere[amountOfSpheres] spheres, int amountOfSpheres)
{
	float tClosest = 1e8;
    bool hit = false;
    vec4 hitColor = vec4(0.0);
	bool flag = true;
	for (int i=0; i<amountOfSpheres; i++)
	{
		Sphere sphere = spheres[i];
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
                hit = true;
            }
        }
    }
    if (hit) {
        FragColor = hitColor;
    } else {
        FragColor = vec4(0.0, 0.0, 0.0, 0.0);
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

    tracingRay(ray, spheres, amountOfSpheres);
}