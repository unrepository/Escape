#version 450

struct Vertex {
	vec3 position;
	vec3 normal;
	vec2 uv;
};

struct Material {
	vec4 albedo;
	float roughness;
	float metallic;
	int useTextures;
};

layout(set = 0, binding = 1) uniform sampler2D albedoTexture;
layout(set = 0, binding = 2) uniform sampler2D normalTexture;
layout(set = 0, binding = 4) uniform sampler2D metallicTexture;
layout(set = 0, binding = 8) uniform sampler2D roughnessTexture;

layout(location = 0) out vec4 fragColor;
layout(location = 1) in Vertex vertex;
layout(location = 10) flat in Material material;

void main() {
//	bool hasAlbedo = (material.useTextures & (1 << 0)) != 0;
//	bool hasNormal = (material.useTextures & (1 << 1)) != 0;
//	bool hasMetallic = (material.useTextures & (1 << 2)) != 0;
//	bool hasRoughness = (material.useTextures & (1 << 3)) != 0;

	fragColor = material.albedo;

//	if(hasAlbedo) {
//		fragColor *= texture(albedoTexture, vertex.uv).rgba;
//	}
}
