#version 430 core

struct CameraData {
	mat4 projection;
	mat4 view;
	vec3 position;
};

layout(std430, binding = 0) readonly buffer CameraBuffer {
	CameraData cameraData;
};

struct Vertex {
	vec3 position;
	vec3 normal;
	vec2 uv;
};

layout(std430, binding = 11) readonly buffer SSBO1 {
	Vertex vertices[];
};

layout(std430, binding = 12) readonly buffer SSBO2 {
	uint indices[];
};

struct Material {
	vec4 albedo;
	float roughness;
	float metallic;
	int useTextures;
};

layout(std430, binding = 13) readonly buffer SSBO3 {
	Material b_material/*[]*/;
};

layout(std430, binding = 14) readonly buffer SSBO4 {
	mat4 b_matrix/*[]*/;
};

smooth out Vertex vertex;
flat out Material material;

void main() {
	uint index = indices[gl_VertexID];
	Vertex v = vertices[index];
	
	gl_Position = cameraData.projection * cameraData.view * b_matrix
		* vec4(v.position, 1.0);

	vertex = v;
	material = b_material;
}
