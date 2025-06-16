#version 430 core

struct Vertex {
	vec3 position;
	vec3 normal;
};

in Vertex vertex;
in vec4 vColor;

out vec4 FragColor;

void main() {
	FragColor = vec4(vertex.position, 1.0);
}
