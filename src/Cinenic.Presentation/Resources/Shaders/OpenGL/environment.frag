#version 430 core

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

uniform sampler2D diffuseTexture;
uniform sampler2D normalTexture;
uniform sampler2D roughnessTexture;
uniform sampler2D metallicTexture;

smooth in Vertex vertex;
flat in Material material;

out vec4 FragColor;

void main() {
	bool hasDiffuse = (material.useTextures & (1 << 0)) != 0;
	bool hasNormal = (material.useTextures & (1 << 1)) != 0;
	bool hasRoughness = (material.useTextures & (1 << 2)) != 0;
	bool hasMetallic = (material.useTextures & (1 << 3)) != 0;
	
	FragColor = material.albedo;
	
	if(hasDiffuse) {
		FragColor *= texture(diffuseTexture, vertex.uv).rgba;
	}
}
