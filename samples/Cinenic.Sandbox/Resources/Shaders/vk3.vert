#version 450

layout(location = 0) out vec3 fragColor;

layout(set = 0, binding = 0) buffer readonly PositionBuffer {
	vec2 positions[];
} positionBuffer;

layout(set = 1, binding = 1) buffer readonly ColorBuffer {
	vec3 colors[];
} colorBuffer;

void main() {
	gl_Position = vec4(positionBuffer.positions[gl_VertexIndex], 0.0, 1.0);
	fragColor = colorBuffer.colors[gl_VertexIndex];
}
