#version 330 core

out vec4 FragColor;
in vec2 fragCoord;

const int amountOfSpheres = 50;

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
	bool flag = true;
	for (int i=0; i<amountOfSpheres; i++)
	{
		Sphere sphere = spheres[i];
		vec3 oc = ray.origin - sphere.center;
		float a = dot(ray.direction, ray.direction);
		float b = 2.0f * dot(oc, ray.direction);
		float c = dot(oc, oc) - sphere.radius * sphere.radius;

		float disc = (b * b) - (4.0f * a * c);

		float t0 = (-b - sqrt(disc)) / (2.0f * a);
		float t1 = (-b + sqrt(disc)) / (2.0f * a);
		if (disc >= 0.0f)
		{
			FragColor = sphere.color;
			flag = false;
			break;
		}
	}
	if (flag)
	{
		FragColor = vec4(0.0f, 0.0f, 0.0f, 0.0f);
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
	vec3 origin = vec3(0.0f, 0.0f, -1.0f);

	Sphere spheres[amountOfSpheres];

	for (int i = 0; i < amountOfSpheres; i++) {
        spheres[i] = createSphere(i);
    }

	Ray ray;
	ray.origin = origin;
    ray.direction = vec3(fragCoord, -1.0f);

	tracingRay(ray, spheres, amountOfSpheres);
}