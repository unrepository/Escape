#version 330 core

//= data
layout (location = 0) in vec3 v_position;
layout (location = 1) in vec3 v_normal;
layout (location = 2) in vec3 v_tangent;
layout (location = 3) in vec3 v_bitangent;
layout (location = 4) in vec2 v_uv;

uniform CameraData {
	mat4 c_projection;
	mat4 c_inverseProjection;
	mat4 c_view;
	mat4 c_inverseView;
	vec3 c_position;
	float c_aspectRatio;
};

uniform mat4 modelMatrix;
//

struct Vertex {
	vec3 position;
	vec3 normal;
	vec3 tangent;
	vec3 bitangent;
	vec2 uv;
};

//= i/o
out Vertex vertex;
out vec3 fragPos;
out vec3 viewPos;
out vec3 normal;
out mat3 TBN;
out vec3 V;
//

void main() {
	vec4 position = c_projection * c_view * modelMatrix * vec4(v_position, 1.0);
	gl_Position = position;
	
	vertex.position = v_position;
	vertex.normal = v_normal;
	vertex.tangent = v_tangent;
	vertex.bitangent = v_bitangent;
	vertex.uv = v_uv;

	vec3 T = normalize(vec3(modelMatrix * vec4(v_tangent, 0.0)));
	vec3 N = normalize(vec3(modelMatrix * vec4(v_normal, 0.0)));
	T = normalize(T - dot(T, N) * N);
	vec3 B = cross(N, T);
	TBN = transpose(mat3(T, B, N));

	fragPos = TBN * vec3(modelMatrix * vec4(v_position, 1.0));
	viewPos = TBN * c_position;
	normal = v_normal * transpose(inverse(mat3(modelMatrix)));

	V = TBN * normalize(c_position - fragPos);
}
