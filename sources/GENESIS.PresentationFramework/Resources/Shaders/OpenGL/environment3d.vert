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
	//vec4 color;
	vec3 normal;
};

//layout(std430, binding = 1) readonly buffer VertexBuffer {
//	Vertex vertices[];
//};

layout(std430, binding = 1) readonly buffer SSBO1 {
	Vertex objectVertices[][];
};

layout(std430, binding = 2) readonly buffer SSBO2 {
	uint modelIndices[];
};

layout(std430, binding = 3) readonly buffer SSBO3 {
	vec4 objectColors[];
};

layout(std430, binding = 4) readonly buffer SSBO4 {
	mat4 objectMatrices[];
};

out vec4 vColor;

void main() {
	uint modelIndex = gl_InstanceID / 4;
	uint subIndex = gl_InstanceID % 4;
	
	uint model = (modelIndices[modelIndex] >> (8 * (3 - subIndex))) & 0xFFu;
	
	Vertex v = objectVertices[model][gl_VertexID];
	
	gl_Position = cameraData.projection * cameraData.view * objectMatrices[gl_InstanceID]
		* vec4(v.position, 1.0);
	vColor = objectColors[gl_InstanceID];
}
