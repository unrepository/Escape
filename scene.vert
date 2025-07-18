#version 450

struct CameraData {
	mat4 projection;
	mat4 view;
	vec3 position;
};

Buffer<CameraData> cameraData : register(t0);

struct Vertex {
	vec3 position;
	vec3 normal;
	vec2 uv;
};

StructuredBuffer<Vertex> vertexData : register(t1);
StructuredBuffer<uint> indexData : register(t2);

struct Material {
	vec4 albedo;
	float roughness;
	float metallic;
	int useTextures;
};

Buffer<Material> materialData : register(t3);
Buffer<mat4> matrixData : register(t4);

layout(location = 1) out Vertex vertex;
layout(location = 2) out Material material;

void main() {
	uint index = indexData[gl_VertexIndex];
	vertex = vertexData[index];

	gl_Position = cameraData.projection * cameraData.view * matrixData
	* vec4(v.position, 1.0);
	material = materialData;
}
