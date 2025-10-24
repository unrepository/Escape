#version 330 core

layout (location = 0) in vec3 v_position;
layout (location = 1) in vec3 v_normal;
layout (location = 2) in vec3 v_tangent;
layout (location = 3) in vec3 v_bitangent;
layout (location = 4) in vec2 v_uv;

layout(std140) uniform CameraUniform {
	mat4 c_projection;
	mat4 c_inverseProjection;
	mat4 c_view;
	mat4 c_inverseView;
	vec3 c_position;
	float c_aspectRatio;
};

uniform mat4 model_matrix;

void main() {
	vec4 position = c_projection * c_view * model_matrix * vec4(v_position, 1.0);
	gl_Position = position;
}
