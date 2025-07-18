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

layout(location = 0) out vec4 fragColor;
layout(location = 1) in Vertex inVertex;
layout(location = 10) flat in Material inMaterial;

void main() {
	fragColor = inMaterial.albedo;
}
