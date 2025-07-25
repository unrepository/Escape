#version 450

layout(set = 1, binding = 0, std430) readonly buffer CameraBuffer {
	mat4 projection;
	mat4 inverse_projection;
	mat4 view;
	mat4 inverse_view;
	vec3 position;
	float aspect_ratio;
} cameraData;

struct Vertex {
	vec3 position;
	vec3 normal;
	vec2 uv;
};

layout(set = 2, binding = 1, std430) readonly buffer VertexBuffer {
	Vertex vertices[];
};

layout(set = 3, binding = 2, std430) readonly buffer IndexBuffer {
	uint indices[];
};

struct Material {
	vec4 albedo;
	float roughness;
	float metallic;
	float ior;
};

layout(set = 4, binding = 3, std430) readonly buffer MaterialBuffer {
	Material materials[];
};

layout(set = 5, binding = 4, std430) readonly buffer MatrixBuffer {
	mat4 matrices[];
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

layout(location = 1) out Vertex outVertex;
layout(location = 10) flat out Material outMaterial;
layout(location = 20) out vec3 worldPos;
layout(location = 21) out vec3 normal;

void main() {
	uint index = indices[pc.indexOffset + gl_VertexIndex];
	Vertex v = vertices[pc.vertexOffset + index];

	mat4 matrix = matrices[pc.matrixOffset];
	Material mat = materials[pc.materialOffset];
	
	gl_Position = cameraData.projection * cameraData.view * matrix * vec4(v.position, 1.0);

	outVertex = v;
	outMaterial = mat;
	
	worldPos = vec3(matrix * vec4(v.position, 1.0));
	normal = v.normal * transpose(inverse(mat3(matrix)));
}
