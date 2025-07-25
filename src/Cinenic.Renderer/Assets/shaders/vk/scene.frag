#version 450
#define PI radians(180)

const float gamma = 2.2;

struct Vertex {
	vec3 position;
	vec3 normal;
	vec3 tangent;
	vec3 bitangent;
	vec2 uv;
};

struct Material {
	vec4 albedo;
	float roughness;
	float metallic;
	float ior;
};

//= lights
struct DirectionalLight {
	vec3 color;
	vec3 direction;
};

struct PointLight {
	vec3 color;
	vec3 position;
};

struct SpotLight {
	vec3 color;
	vec3 position;
	vec3 direction;
	
	float cutoff;
	float cutoffOuter;
};

//= buffers
layout(set = 1, binding = 0, std430) readonly buffer CameraBuffer {
	mat4 projection;
	mat4 inverseProjection;
	mat4 view;
	mat4 inverseView;
	vec3 position;
	float aspectRatio;
} cameraData;

layout(set = 6, binding = 10) readonly buffer LightCountData {
	uint dirCount;
	uint pointCount;
	uint spotCount;
} lightData;

layout(set = 7, binding = 11) readonly buffer DirectionalLightBuffer {
	DirectionalLight lights[];
} dirLightData;

layout(set = 8, binding = 12) readonly buffer PointLightBuffer {
	PointLight lights[];
} pointLightData;

layout(set = 9, binding = 13) readonly buffer SpotLightBuffer {
	SpotLight lights[];
} spotLightData;

layout(push_constant) uniform PushConstants {
	uint vertexOffset;
	uint indexOffset;
	uint materialOffset;
	uint matrixOffset;
	int albedoTextureIndex;
	int normalTextureIndex;
	int metallicTextureIndex;
	int roughnessTextureIndex;
} pc;

layout(set = 0, binding = 0) uniform sampler2D textures[1024];

layout(location = 0) out vec4 fragColor;
layout(location = 1) in Vertex vertex;
layout(location = 10) flat in Material material;
layout(location = 20) in vec3 worldPos;
layout(location = 21) in vec3 normal;
layout(location = 22) in mat3 TBN;

//= functions
vec3 fresnelSchlick(float cosTheta, vec3 F0) {
	return F0 + (1.0 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}

float distributionGGX(vec3 N, vec3 H, float roughness) {
	float a = roughness * roughness;
	float a2 = a * a;
	float NdotH = max(dot(N, H), 0.0);
	float NdotH2 = NdotH * NdotH;
	
	float num = a2;
	float denom = (NdotH2 * (a2 - 1.0) + 1.0);
	denom = PI * denom * denom;
	
	return num / denom;
}

float geometrySchlickGGX(float NdotV, float roughness) {
	float r = roughness + 1.0;
	float k = (r * r) / 8.0;
	
	float num = NdotV;
	float denom = NdotV * (1.0 - k) + k;
	
	return num / denom;
}

float geometrySmith(vec3 N, vec3 V, vec3 L, float roughness) {
	float NdotV = max(dot(N, V), 0.0);
	float NdotL = max(dot(N, L), 0.0);
	float ggx2  = geometrySchlickGGX(NdotV, roughness);
	float ggx1  = geometrySchlickGGX(NdotL, roughness);

	return ggx1 * ggx2;
}

float calcAttenuation(float distance) {
	return 1.0 / (distance * distance); // inverse-square
}

vec3 brdf(vec3 albedo, float roughness, float metallic, vec3 N, vec3 V, vec3 L, vec3 radiance) {
	vec3 H = normalize(V + L);

	vec3 F0 = mix(
		vec3(pow(material.ior - 1, 2) / pow(material.ior + 1, 2)),
		albedo,
		metallic
	);
	// vec3 F0 = vec3(pow(material.ior - 1, 2) / pow(material.ior + 1, 2));
	
	vec3 F = fresnelSchlick(max(dot(H, V), 0.0), F0);
	float NDF = distributionGGX(N, H, roughness);
	float G = geometrySmith(N, V, L, roughness);
	
	vec3 numerator = NDF * G * F;
	
	float NdotV = max(dot(N, V), 0.0);
	float NdotL = max(dot(N, L), 0.0);
	float denominator = max(4.0 * NdotV * NdotL, 0.001);
	
	vec3 specular = numerator / denominator;
	
	vec3 kS = F;
	vec3 kD = vec3(1.0) - kS;
	kD *= 1.0 - metallic;
	
	return (kD * albedo / PI + specular) * radiance * NdotL;
}

//= light type functions
vec3 solveDirectionalLight(vec3 albedo, float roughness, float metallic, vec3 N, vec3 V, DirectionalLight l) {
	vec3 L = normalize(-l.direction);
	vec3 radiance = l.color;

	return brdf(albedo, roughness, metallic, N, V, L, radiance);
}

vec3 solvePointLight(vec3 albedo, float roughness, float metallic, vec3 N, vec3 V, PointLight l) {
	vec3 L = normalize(l.position - worldPos);
	float distance = length(l.position - worldPos);
	vec3 radiance = l.color * calcAttenuation(distance);
	
	return brdf(albedo, roughness, metallic, N, V, L, radiance);
}

// TODO not sure if this is implemented correctly, but it's very hard to check manually specifying rotations
vec3 solveSpotLight(vec3 albedo, float roughness, float metallic, vec3 N, vec3 V, SpotLight l) {
	vec3 L = normalize(l.position - worldPos);

	float theta = dot(L, normalize(-l.direction));
	float epsilon = (l.cutoff + l.cutoffOuter) - l.cutoff;
	float intensity = clamp((theta - (l.cutoff + l.cutoffOuter)) / epsilon, 0.0, 1.0);

	float distance = length(l.position - worldPos);
	vec3 radiance = l.color * calcAttenuation(distance) * intensity;
	
	return brdf(albedo, roughness, metallic, N, V, L, radiance);
}

//= entry point
void main() {
	vec3 albedo = material.albedo.rgb;
	float roughness = material.roughness;
	float metallic = material.metallic;
	
	if(pc.albedoTextureIndex > 0) {
		albedo *= pow(texture(textures[pc.albedoTextureIndex], vertex.uv).rgb, vec3(gamma));
	}

	if(pc.metallicTextureIndex > 0) {
		metallic = texture(textures[pc.metallicTextureIndex], vertex.uv).r;
	}

	if(pc.roughnessTextureIndex > 0) {
		roughness = texture(textures[pc.roughnessTextureIndex], vertex.uv).r;
	}
	
	//= normal mapping
	vec3 n = normal;
	
	if(pc.normalTextureIndex > 0) {
		n = texture(textures[pc.normalTextureIndex], vertex.uv).rgb;
		n = n * 2.0 - 1.0;
		n = normalize(TBN * n);
	}

	//=
	vec3 N = normalize(n);
	vec3 V = normalize(cameraData.position - worldPos);

	vec3 Lo = vec3(0.0);

	//= solve light sources
	for(int i = 0; i < lightData.dirCount; i++) {
		Lo += solveDirectionalLight(albedo, roughness, metallic, N, V, dirLightData.lights[i]);
	}
	
	for(int i = 0; i < lightData.pointCount; i++) {
		Lo += solvePointLight(albedo, roughness, metallic, N, V, pointLightData.lights[i]);
	}

	for(int i = 0; i < lightData.spotCount; i++) {
		Lo += solveSpotLight(albedo, roughness, metallic, N, V, spotLightData.lights[i]);
	}

	vec3 ambient = vec3(0.0) * albedo; // TODO customizable
	vec3 color = ambient + Lo;
	
	// gamma correction (reinhard)
	color = color / (color + vec3(1.0));
	color = pow(color, vec3(1.0 / gamma));
	
	fragColor = vec4(color, 1.0);
}
