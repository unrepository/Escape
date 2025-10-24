#version 330 core

//layout(location = 0) in vec3 v_position;
//layout(location = 1) in vec3 v_normal;
//layout(location = 2) in vec3 v_tangent;
//layout(location = 3) in vec3 v_bitangent;
//layout(location = 4) in vec2 v_uv;

//= data
uniform sampler2D albedoTexture;
uniform sampler2D normalTexture;
uniform sampler2D metallicTexture;
uniform sampler2D roughnessTexture;
uniform sampler2D heightTexture;

out vec4 fragColor;

//= entry point
void main() {
	fragColor = vec4(1.0);
}
