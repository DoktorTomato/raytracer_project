#version 430 core

out vec4 FragColor;
in vec2 fragCoord;

uniform vec3 cameraPos;
uniform mat4 view;
uniform mat4 projection;

uniform int numCubes;
uniform int numSpheres;
const int amountOfLights = 1; // maximum amountOfLights is 6
const int numPointLights = 1;
const int numSpotLights = 1;

struct Ray {
    vec3 origin;
    vec3 direction;
};

struct Sphere {
    vec4 centerRadius;
    vec4 color;
};

struct Triangle {
    vec4 v1;
    vec4 v2;
    vec4 v3;
    vec4 color;
};

struct Object {
    vec4 color;
    vec4 positionScale;   // xyz = position, w = scale
    ivec4 triangleInfo;   // x = numTriangles, y = triangleOffset
};

struct DirLight {
    Ray ray;
    vec4 color;
};

struct PointLight {
    Ray ray;
    vec4 color;

    float constant;
    float linear;
    float quadratic;
};

struct SpotLight {
    Ray ray;
    vec4 color;
    float cutOff;
    float outerCutOff;

    float constant;
    float linear;
    float quadratic;
};

layout(std430, binding = 0) buffer TriangleBuffer {
    Triangle triangles[];
};

layout(std430, binding = 1) buffer ObjectBuffer {
    Object objects[];
};

layout(std430, binding = 2) buffer SphereBuffer {
    Sphere spheres[];
};

bool tracingTriangle(Ray ray, Triangle triangle, out float t, out vec3 hitNormal, inout float tClosest) {
    const float eps = 0.001;

    vec3 edge1 = triangle.v2.xyz - triangle.v1.xyz;
    vec3 edge2 = triangle.v3.xyz - triangle.v1.xyz;
    vec3 ray_cross_e2 = cross(ray.direction, edge2);
    float det = dot(edge1, ray_cross_e2);

    if (det > -eps && det < eps) return false;

    float inv_det = 1.0 / det;
    vec3 ray_to_v1 = ray.origin - triangle.v1.xyz;
    float u = inv_det * dot(ray_to_v1, ray_cross_e2);

    if (u < 0.0 || u > 1.0) return false;

    vec3 ray_cross_e1 = cross(ray_to_v1, edge1);
    float v = inv_det * dot(ray.direction, ray_cross_e1);

    if (v < 0.0 || u + v > 1.0) return false;

    float tCandidate = inv_det * dot(edge2, ray_cross_e1);

    if (tCandidate > eps && tCandidate < tClosest) {
        t = tCandidate;
        hitNormal = normalize(cross(edge1, edge2));
        if (dot(hitNormal, ray.direction) > 0.0)
            hitNormal = -hitNormal;
        tClosest = tCandidate;
        return true;
    }
    return false;
}

bool tracingSphere(Ray ray, Sphere sphere, out float t, out vec3 hitNormal, inout float tClosest) {
    vec3 center = sphere.centerRadius.xyz;
    float radius = sphere.centerRadius.w;

    vec3 oc = ray.origin - center;
    float a = dot(ray.direction, ray.direction);
    float b = 2.0f * dot(oc, ray.direction);
    float c = dot(oc, oc) - radius * radius;

    float disc = b * b - 4.0f * a * c;
    if (disc < 0.0) return false;

    float sqrtDisc = sqrt(disc);
    float t0 = (-b - sqrtDisc) / (2.0 * a);
    float t1 = (-b + sqrtDisc) / (2.0 * a);
    float tCandidate = (t0 > 0.001) ? t0 : t1;

    if (tCandidate > 0.001 && tCandidate < tClosest) {
        t = tCandidate;
        vec3 pos = ray.origin + tCandidate * ray.direction;
        hitNormal = normalize(pos - center);
        tClosest = tCandidate;
        return true;
    }
    return false;
}

bool intersectGround(Ray ray, out float t, out vec3 hitNormal, inout float tClosest) {
    vec3 n  = vec3(0.0, 1.0, 0.0);
    vec3 p0 = vec3(0.0, -1.0, 0.0);

    float denom = dot(n, ray.direction);
    if (denom >= -1e-6) 
        return false;

    float tCandidate = dot(p0 - ray.origin, n) / denom;
    if (tCandidate > 0.001 && tCandidate < tClosest) {
        t          = tCandidate;
        hitNormal  = n;
        tClosest   = tCandidate;
        return true;
    }
    return false;
}

float computeSoftShadow(vec3 fragPos, vec3 lightPos, float lightRadius) {
    const int numSamples = 16;
    float shadow = 0.0;

    for (int i = 0; i < numSamples; i++) {
        vec3 randomOffset = lightRadius * vec3(
            fract(sin(dot(vec3(i,fragPos.x,fragPos.y),
                          vec3(12.9898,78.233,45.164))) * 43758.5453),
            fract(sin(dot(vec3(i,fragPos.y,fragPos.z),
                          vec3(93.9898,67.345,21.123))) * 43758.5453),
            fract(sin(dot(vec3(i,fragPos.z,fragPos.x),
                          vec3(45.123,12.345,78.456))) * 43758.5453)
        );

        vec3 samplePos = lightPos + randomOffset;
        vec3 Ldir = normalize(samplePos - fragPos);
        float maxDist = length(samplePos - fragPos);

        Ray shadowRay;
        shadowRay.origin = fragPos + 0.001 * Ldir;
        shadowRay.direction = Ldir;

        float tClosest = maxDist;
        float t;
        vec3 nrm;
        bool inShadow = false;

        for (int j = 0; j < numSpheres; j++) {
            if (tracingSphere(shadowRay, spheres[j], t, nrm, tClosest)) {
                inShadow = true;
                break;
            }
        }

        for (int j = 0; j < numCubes && !inShadow; j++) {
            Object obj = objects[j];
            int cnt = obj.triangleInfo.x;
            int off = obj.triangleInfo.y;
            for (int k = 0; k < cnt; k++) {
                if (tracingTriangle(shadowRay,
                                    triangles[off + k],
                                    t, nrm, tClosest)) {
                    inShadow = true;
                    break;
                }
            }
        }

        if (!inShadow)
            shadow += 1.0;
    }
    return shadow / float(numSamples);
}

vec4 calculateDirLight(DirLight light, vec3 normal, vec3 fragPos, vec3 viewDir, vec4 objectColor) {
    vec3 lightDir = normalize(light.ray.origin - fragPos);
    vec3 reflectDir = reflect(-lightDir, normal);

    float shadow = computeSoftShadow(fragPos, light.ray.origin, 0.5);
    
    vec4 ambient = 0.1 * objectColor * light.color;
    vec4 diffuse = max(dot(normal, lightDir), 0.0) * objectColor * light.color;
    vec4 specular = pow(max(dot(viewDir, reflectDir), 0.0), 32.0) * light.color;

    return ambient + shadow * (diffuse + specular);
}

vec4 calculatePointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir, vec4 objectColor) {
    vec3 lightDir = normalize(light.ray.origin - fragPos);
    float distance = length(light.ray.origin - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));

    float shadow = computeSoftShadow(fragPos, light.ray.origin, 0.5);

    vec4 ambient = 0.1 * objectColor * light.color;
    vec4 diffuse = max(dot(normal, lightDir), 0.0) * objectColor * light.color;
    vec4 specular = pow(max(dot(viewDir, reflect(-lightDir, normal)), 0.0), 32.0) * light.color;

    return (ambient + shadow * (diffuse + specular)) * attenuation;
}

vec4 calculateSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir, vec4 objectColor) {
    vec3 lightDir = normalize(light.ray.origin - fragPos);
    float distance = length(light.ray.origin - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));

    float theta = dot(lightDir, normalize(-light.ray.direction));
    float epsilon = max(light.cutOff - light.outerCutOff, 0.001);
    float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);

    float shadow = computeSoftShadow(fragPos, light.ray.origin, 0.5);

    vec4 ambient = 0.1 * objectColor * light.color;
    vec4 diffuse = max(dot(normal, lightDir), 0.0) * objectColor * light.color;
    vec4 specular = pow(max(dot(viewDir, reflect(-lightDir, normal)), 0.0), 32.0) * light.color;

    return (ambient + shadow * (diffuse + specular)) * intensity * attenuation;
}

void rayTrace(Ray ray, DirLight lights[amountOfLights], int amountOfLight, PointLight pointLights[numPointLights], int numPointLight, SpotLight spotLights[numSpotLights], int numSpotLight) {
    float tClosest = 1e8;
    vec4 objColor = vec4(0.0);
    vec3 hitNormal = vec3(0.0);
    float t;
    bool hitSomething = false;

    if (intersectGround(ray, t, hitNormal, tClosest)) {
        hitSomething = true;
        objColor = vec4(1.75, 0.39, 2.45, 0.8);
    }

    for (int i = 0; i < numSpheres; i++) {
        vec3 n;
        if (tracingSphere(ray, spheres[i], t, n, tClosest)) {
            hitSomething = true;
            objColor = spheres[i].color;
            hitNormal = n;
        }
    }

    for (int i = 0; i < numCubes; ++i) {
        Object obj = objects[i];
        int numTris = obj.triangleInfo.x;
        int offset = obj.triangleInfo.y;
        for (int j = 0; j < numTris; ++j) {
            Triangle tri = triangles[offset + j];
            vec3 n;
            if (tracingTriangle(ray, tri, t, n, tClosest)) {
                hitSomething = true;
                objColor = tri.color;
                hitNormal = n;
            }
        }
    }

    if (hitSomething) {
        vec3 pos = ray.origin + tClosest * ray.direction;
        vec3 viewDir = normalize(ray.origin - pos);
        vec4 finColor = vec4(0.0);
        for (int i = 0; i < amountOfLight; i++) {
            finColor += calculateDirLight(lights[i], hitNormal, pos, viewDir, objColor);
        }
        for (int i = 0; i < numPointLight; i++) {
            finColor += calculatePointLight(pointLights[i], hitNormal, pos, viewDir, objColor);
        }
        for (int i = 0; i < numSpotLight; i++) {
            finColor += calculateSpotLight(spotLights[i], hitNormal, pos, viewDir, objColor);
        }

        FragColor = clamp(finColor, 0.0, 1.0);
    } else {
        FragColor = vec4(0.0, 0.0, 0.0, 1.0);
    }
}

void generateLight(int num, out DirLight res[amountOfLights], int numPointLights, out PointLight pointLights[numPointLights], int numSpotLights, out SpotLight spotLights[numSpotLights]) {
    vec3 origins[6] = {
        vec3(0, 0, 0), vec3(-1, -1, -1), vec3(1, -1, -1),
        vec3(1, 1, -1), vec3(-1, -1, 1), vec3(-1, 1, 1)
    };
    vec4 colors[6] = {
        vec4(1.0), vec4(1.0, 0.0, 0.0, 1.0), vec4(0.0, 1.0, 0.0, 1.0),
        vec4(0.0, 0.0, 1.0, 1.0), vec4(1.0, 1.0, 0.0, 1.0), vec4(0.0, 1.0, 1.0, 1.0)
    };

    for (int i = 0; i < amountOfLights; i++) {
        res[i].ray.origin = origins[i];
        res[i].ray.direction = vec3(0.0);
        res[i].color = colors[i];
    }

    for (int i = 0; i < numPointLights; i++) {
        pointLights[i].ray.origin = vec3(0.0, 5.0, 0.0);
        pointLights[i].ray.direction = vec3(0.0);
        pointLights[i].color = vec4(1.0);
        pointLights[i].constant = 1.0;
        pointLights[i].linear = 0.014;
        pointLights[i].quadratic = 0.0007;
    }

    for (int i = 0; i < numSpotLights; i++) {
        spotLights[i].ray.origin = vec3(0.0, 7.0, 0.0);
        spotLights[i].ray.direction = normalize(vec3(0.0, -1.0, 0.0));
        spotLights[i].color = colors[4];
        spotLights[i].cutOff = cos(radians(12.5));
        spotLights[i].outerCutOff = cos(radians(27.5));
        spotLights[i].constant = 1.0;
        spotLights[i].linear = 0.09;
        spotLights[i].quadratic = 0.032;
    }
}

void main() {
    vec2 ndc = fragCoord * 2.0 - 1.0;
    vec4 clipSpace = vec4(ndc, -1.0, 1.0);
    vec4 eyeSpace = inverse(projection) * clipSpace;
    eyeSpace = vec4(eyeSpace.xy, -1.0, 0.0);

    vec3 rayDirWorld = normalize((inverse(view) * eyeSpace).xyz);

    Ray ray;
    ray.origin = cameraPos;
    ray.direction = rayDirWorld;

    DirLight lights[amountOfLights];
    PointLight pointLights[numPointLights];
    SpotLight spotLights[numSpotLights];
    
    generateLight(amountOfLights, lights, numPointLights, pointLights, numSpotLights, spotLights);

    rayTrace(ray, lights, 0, pointLights, numPointLights, spotLights, numSpotLights);
}
