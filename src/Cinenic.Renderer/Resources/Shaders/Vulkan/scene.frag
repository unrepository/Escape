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

void main() {
//	bool hasAlbedo = (material.useTextures & (1 << 0)) != 0;
//	bool hasNormal = (material.useTextures & (1 << 1)) != 0;
//	bool hasMetallic = (material.useTextures & (1 << 2)) != 0;
//	bool hasRoughness = (material.useTextures & (1 << 3)) != 0;

	fragColor = material.albedo;

	if(pc.albedoTextureIndex > 0) {
		fragColor *= texture(textures[pc.albedoTextureIndex], vertex.uv).rgba;
	}
}
