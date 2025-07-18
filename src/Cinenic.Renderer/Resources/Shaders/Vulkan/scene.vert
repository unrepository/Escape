#version 450

layout(set = 0, binding = 0, std430) readonly buffer CameraBuffer {
	mat4 projection;
	mat4 view;
	vec3 position;
} cameraData;

struct Vertex {
	vec3 position;
	vec3 normal;
	vec2 uv;
};

layout(set = 1, binding = 1, std430) readonly buffer VertexBuffer {
	Vertex vertices[];
};

layout(set = 2, binding = 2, std430) readonly buffer IndexBuffer {
	uint indices[];
};

struct Material {
	vec4 albedo;
	float roughness;
	float metallic;
	int useTextures;
};

layout(set = 3, binding = 3, std430) readonly buffer MaterialBuffer {
	Material material;
};

layout(set = 4, binding = 4, std430) readonly buffer MatrixBuffer {
	mat4 matrix;
};

layout(location = 1) out Vertex outVertex;
layout(location = 10) flat out Material outMaterial;

void main() {
	uint index = indices[gl_VertexIndex];
	Vertex v = vertices[index];

	gl_Position = cameraData.projection * cameraData.view * matrix * vec4(v.position, 1.0);

	outVertex = v;
	outMaterial = material;
}
