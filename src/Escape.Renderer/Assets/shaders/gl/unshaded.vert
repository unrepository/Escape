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
//

void main() {
	vec4 position = c_projection * c_view * modelMatrix * vec4(v_position, 1.0);
	gl_Position = position;
	
	vertex.position = v_position;
	vertex.normal = v_normal;
	vertex.tangent = v_tangent;
	vertex.bitangent = v_bitangent;
	vertex.uv = v_uv;
}
