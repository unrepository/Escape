#version 450

layout(set = 1, binding = 0, std430) readonly buffer CameraBuffer {
	mat4 projection;
	mat4 inverseProjection;
	mat4 view;
	mat4 inverseView;
	vec3 position;
	float aspectRatio;
} cameraData;

struct Vertex {
	vec3 position;
	vec3 normal;
	vec3 tangent;
	vec3 bitangent;
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
	int displacementTextureIndex;
} pc;

layout(location = 1) out Vertex outVertex;
layout(location = 10) flat out Material outMaterial;
layout(location = 20) out vec3 fragPos;
layout(location = 21) out vec3 viewDir;
layout(location = 22) out vec3 normal;
layout(location = 23) out mat3 TBN;
layout(location = 26) out mat3 tTBN;
layout(location = 29) out vec3 tViewPos;
layout(location = 30) out vec3 tFragPos;
layout(location = 31) out vec3 tViewDir;

void main() {
	uint index = indices[pc.indexOffset + gl_VertexIndex];
	Vertex v = vertices[pc.vertexOffset + index];

	mat4 matrix = matrices[pc.matrixOffset];
	Material mat = materials[pc.materialOffset];
	
	gl_Position = cameraData.projection * cameraData.view * matrix * vec4(v.position, 1.0);
	gl_Position.y *= -1.0; // Vulkan has funky coordinates so we need to flip

	outVertex = v;
	outMaterial = mat;

	vec3 T = normalize(vec3(matrix * vec4(v.tangent, 0.0)));
	vec3 N = normalize(vec3(matrix * vec4(v.normal, 0.0)));
	vec3 B = normalize(vec3(matrix * vec4(v.bitangent, 0.0)));
//	T = normalize(T - dot(T, N) * N);
//	vec3 B = cross(T, N);
	TBN = mat3(T, B, N);
	tTBN = transpose(TBN);
	
	fragPos = vec3(matrix * vec4(v.position, 1.0));
	viewDir = normalize(cameraData.position - fragPos);
	
	tViewPos = tTBN * cameraData.position;
	tFragPos = tTBN * fragPos;
	tViewDir = normalize(tViewPos - tFragPos);
	
	normal = v.normal * transpose(inverse(mat3(matrix)));
}
