#version 430 core

struct Data {
	float time;
};

layout(std430, binding = 0) readonly buffer DataSSBO {
	Data data;
};

in vec2 vPos;
in vec2 fbCoords;

out vec4 FragColor;

void main() {
	vec2 uv = vPos * 0.5 + 0.5;
	vec3 color = vec3(uv, 1.0 - (uv.x + uv.y) * 0.5 * data.time);

	FragColor = vec4(color, 1.0);
}
