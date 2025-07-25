#version 450
#define PI radians(180)

const float gamma = 2.2;

layout(set = 1, binding = 0, std430) readonly buffer CameraBuffer {
	mat4 projection;
	mat4 inverse_projection;
	mat4 view;
	mat4 inverse_view;
	vec3 position;
	float aspect_ratio;
} cameraData;

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

layout(set = 0, binding = 0) uniform sampler2D textures[1024];

layout(location = 0) out vec4 fragColor;
layout(location = 1) in Vertex vertex;
layout(location = 10) flat in Material material;
layout(location = 20) in vec3 worldPos;
layout(location = 21) in vec3 normal;
layout(location = 22) in mat3 TBN;

//= structs
struct Light {
	vec3 position;
	vec3 color;
};

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

vec3 solveLightSource(vec3 albedo, float roughness, float metallic, vec3 N, vec3 V, Light l) {
	vec3 L = normalize(l.position - worldPos);
	vec3 H = normalize(V + L);
	
	float distance = length(l.position - worldPos);
	float attenuation = 1.0 / (distance * distance); // inverse-square law
	vec3 radiance = l.color * attenuation;

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
	
	float NdotV = dot(N, V);
	float NdotL = dot(N, L);
	float denominator = max(4.0 * NdotV * NdotL, 0.001);
	
	vec3 specular = numerator / denominator;
	
	vec3 kS = F;
	vec3 kD = vec3(1.0) - kS;
	kD *= 1.0 - metallic;
	
	NdotL = max(NdotL, 0.0);
	return (kD * albedo / PI + specular) * radiance * NdotL;
}

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

	//= solve single light source at the center of the scene (for now)
	for(int i = 0; i < 1; i++) {
		Lo += solveLightSource(albedo, roughness, metallic, N, V, Light(vec3(0.0), vec3(100.0)));
	}

	vec3 ambient = vec3(0.03) * albedo; // TODO
	vec3 color = ambient + Lo;
	
	// gamma correction (reinhard)
	color = color / (color + vec3(1.0));
	color = pow(color, vec3(1.0 / gamma));
	
	fragColor = vec4(color, 1.0);
}
