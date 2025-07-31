#version 430 core

out vec2 vPos;
out vec2 fbCoords;

void main() {
	const vec2 vertices[6] = vec2[](
		vec2(-1, -1), // bottom-left
		vec2(1, -1), // bottom-right
		vec2(-1, 1), // top-left
		vec2(-1, 1),
		vec2(1, -1), // bottom-right
		vec2(1, 1) // top-right
	);
	
	vec2 pos = vertices[gl_VertexID];
	vPos = pos;
	fbCoords = (pos + 1.0) * 0.5;
	
	gl_Position = vec4(pos, 0, 1);
}
