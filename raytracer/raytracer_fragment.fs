#version 330 core

out vec4 FragColor;
in vec2 fragCoord;

struct Ray
{
	vec3 origin;
	vec3 direction;
};

struct Sphere
{
	vec3 center;
	float radius;
};

void main()
{
	vec3 origin = vec3(0.0f, 0.0f, -1.0f);
	Sphere sphere;
	sphere.center = vec3(0.7f, 0.8f, 1.0f);
	//sphere.center = vec3(0.0f, 0.0f, 0.0f);
	sphere.radius = 0.3f;
	Ray ray;
	ray.origin = origin;
    ray.direction = vec3(fragCoord, -1.0f);

	vec3 oc = ray.origin - sphere.center;
    float a = dot(ray.direction, ray.direction);
    float b = 2.0f * dot(oc, ray.direction);
    float c = dot(oc, oc) - sphere.radius * sphere.radius;

	float disc = (b * b) - (4.0f * a * c);

	float t0 = (-b - sqrt(disc)) / (2.0f * a);
	float t1 = (-b + sqrt(disc)) / (2.0f * a);

	if (disc < 0.0f)
	{
		FragColor = vec4(0.0f, 0.0f, 0.0f, 0.0f);
	}
	else
	{
		FragColor = vec4(1.0, 0.0, 0.0, 1.0f);
	}
}