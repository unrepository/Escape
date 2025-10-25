#version 330 core

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

uniform mat4 model_matrix;

struct Vertex {
	vec3 position;
	vec3 normal;
	vec3 tangent;
	vec3 bitangent;
	vec2 uv;
};

vec2 positions[6] = vec2[](
vec2(-1.0f,  1.0f), // Top-left
vec2( 1.0f,  1.0f), // Top-right vertex
vec2( 1.0f, -1.0f),  // Bottom-right vertex
vec2( 1.0f, -1.0f),  // Bottom-right vertex
vec2(-1.0f, -1.0f), // Bottom-left vertex
vec2(-1.0f,  1.0f) // Top-left
);

out Vertex v;

void main() {
	vec4 position = c_projection * c_view * model_matrix * vec4(v_position, 1.0);
	gl_Position = position;
	
	v.position = v_position;
	v.normal = v_normal;
	v.tangent = v_tangent;
	v.bitangent = v_bitangent;
	v.uv = v_uv;
	//gl_Position = vec4(positions[gl_VertexID], 0.0, 1.0);
}
