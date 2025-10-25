#version 330 core

//= data
uniform CameraData {
	mat4 c_projection;
	mat4 c_inverseProjection;
	mat4 c_view;
	mat4 c_inverseView;
	vec3 c_position;
	float c_aspectRatio;
};

uniform sampler2D albedoTexture;
uniform sampler2D normalTexture;
uniform sampler2D metallicTexture;
uniform sampler2D roughnessTexture;
uniform sampler2D heightTexture;

uniform mat4 model_matrix;

//

struct Vertex {
	vec3 position;
	vec3 normal;
	vec3 tangent;
	vec3 bitangent;
	vec2 uv;
};

in Vertex v;
out vec4 fragColor;

void main() {
	fragColor = vec4(1.0, 1.0, 1.0, 1.0);
	fragColor *= texture(albedoTexture, v.uv);
}
