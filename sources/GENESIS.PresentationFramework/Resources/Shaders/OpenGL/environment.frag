#version 430 core
#extension GL_ARB_bindless_texture : require

struct Vertex {
	vec3 position;
	vec3 normal;
	vec2 texCoords;
};

struct Material {
	vec4 albedoColor;
	int hasTextures;
	sampler2D diffuseTexture;
};

smooth in Vertex vertex;
flat in Material material;

out vec4 FragColor;

void main() {
	if(material.hasTextures > 0) {
		FragColor = /*material.albedoColor * */vec4(texture(material.diffuseTexture, vertex.texCoords).rgb, 1.0) + vec4(0.5, 0.5, 0.5, 0.0);
	} else {
		FragColor = vec4(vertex.texCoords, 1.0, 1.0);
	}
}
