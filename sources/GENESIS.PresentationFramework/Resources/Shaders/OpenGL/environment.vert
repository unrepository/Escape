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

layout(std430, binding = 10) readonly buffer SSBO1 {
	Vertex vertices[];
};

struct Material {
	vec4 albedo;
	float roughness;
	float metallic;
	int useTextures;
};

layout(std430, binding = 11) readonly buffer SSBO3 {
	Material materials[];
};

layout(std430, binding = 12) readonly buffer SSBO4 {
	mat4 objectMatrices[];
};

smooth out Vertex vertex;
flat out Material material;

void main() {
	Vertex v = vertices[gl_VertexID];
	
	gl_Position = cameraData.projection * cameraData.view * objectMatrices[gl_InstanceID]
		* vec4(v.position, 1.0);

	vertex = v;
	material = materials[gl_InstanceID];
}
