#version 330 core

#define MAX_DIRECTIONAL_LIGHTS 16
#define MAX_POINT_LIGHTS 1024
#define MAX_SPOT_LIGHTS 1024

struct Vertex {
	vec3 position;
	vec3 normal;
	vec3 tangent;
	vec3 bitangent;
	vec2 uv;
};

struct DirectionalLight {
	vec3 color;
	vec3 direction;
};

struct PointLight {
	vec3 color;
	vec3 position;
};

struct SpotLight {
	vec3 color;
	vec3 position;
	vec3 direction;

	float cutoff;
	float cutoffOuter;
};

//= data
uniform CameraData {
	mat4 c_projection;
	mat4 c_inverseProjection;
	mat4 c_view;
	mat4 c_inverseView;
	vec3 c_position;
	float c_aspectRatio;
};

uniform MaterialData {
	vec4 m_albedo;
	float m_roughness;
	float m_metallic;
	float m_ior;

	bool m_pmComplex;
	uint m_pmMinLayers;
	uint m_pmMaxLayers;
	float m_pmHeightScale;
};

uniform sampler2D albedoTexture;
uniform sampler2D normalTexture;
uniform sampler2D metallicTexture;
uniform sampler2D roughnessTexture;
uniform sampler2D heightTexture;

uniform uint availableTextures;

#define HAS_ALBEDO_TEXTURE (availableTextures & (1u << 0u)) != 0u
#define HAS_NORMAL_TEXTURE (availableTextures & (1u << 1u)) != 0u
#define HAS_METALLIC_TEXTURE (availableTextures & (1u << 2u)) != 0u
#define HAS_ROUGHNESS_TEXTURE (availableTextures & (1u << 3u)) != 0u
#define HAS_HEIGHT_TEXTURE (availableTextures & (1u << 4u)) != 0u

uniform LightData {
	uint l_dirCount;
	uint l_pointCount;
	uint l_spotCount;
};

uniform DirectionalLightData {
	DirectionalLight l_directionalLights[MAX_DIRECTIONAL_LIGHTS];
};

uniform PointLightData {
	PointLight l_pointLights[MAX_POINT_LIGHTS];
};

uniform SpotLightData {
	SpotLight l_spotLights[MAX_SPOT_LIGHTS];
};

//

//= i/o
in Vertex vertex;
out vec4 fragColor;
//

//= entry point
void main() {
	vec2 uv = vertex.uv;

	vec3 albedo = m_albedo.rgb;
	float opacity = 1.0;

	//= textures
	if(HAS_ALBEDO_TEXTURE) {
		albedo *= texture(albedoTexture, uv).rgb;
	}

	fragColor = vec4(albedo, opacity); // simple alpha blending
}
