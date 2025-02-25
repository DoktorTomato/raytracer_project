#version 330 core

out vec4 FragColor;
in vec2 fragCoord;

const int amountOfSpheres = 3;

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

void main()
{
	vec3 origin = vec3(0.0f, 0.0f, -1.0f);

	Sphere sphere1;
	sphere1.center = vec3(0.7f, 0.8f, 1.0f);
	sphere1.radius = 0.3f;
	sphere1.color = vec4(1.0f, 0.0f, 0.0f, 1.0f);

	Sphere sphere2;
	sphere2.center = vec3(0.0f, 0.0f, 0.0f);
	sphere2.radius = 0.3f;
	sphere2.color = vec4(0.0f, 0.0f, 1.0f, 1.0f);

	Sphere sphere3;
	sphere3.center = vec3(0.7f, 1.2f, 1.0f);
	sphere3.radius = 0.3f;
	sphere3.color = vec4(0.0f, 1.0f, 1.0f, 1.0f);

	Sphere spheres[amountOfSpheres] = Sphere[](sphere1, sphere2, sphere3);
	Ray ray;
	ray.origin = origin;
    ray.direction = vec3(fragCoord, -1.0f);

	tracingRay(ray, spheres, amountOfSpheres);
}