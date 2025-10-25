#version 330 core
#define PI radians(180)
#define MIN_SAFE_VALUE 0.000001

#define MAX_DIRECTIONAL_LIGHTS 16
#define MAX_POINT_LIGHTS 1024
#define MAX_SPOT_LIGHTS 1024

const float gamma = 2.2;

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
in vec3 fragPos;
in vec3 viewPos;
in vec3 normal;
in mat3 TBN;
in vec3 V;
out vec4 fragColor;
//

//= functions
// GGX/Trowbridge-Reitz
float normalDistribution(float alpha, vec3 N, vec3 H) {
	float numerator = pow(alpha, 2.0);

	float NdotH = max(dot(N, H), 0.0);
	float denominator = PI * pow(pow(NdotH, 2.0) * (pow(alpha, 2.0) - 1.0) + 1.0, 2.0);
	denominator = max(denominator, MIN_SAFE_VALUE);

	return numerator / denominator;
}

// Schlick-Beckmann
float geometryShadowing1(float alpha, vec3 N, vec3 X) {
	float numerator = max(dot(N, X), 0.0);

	float k = alpha / 2.0;
	float denominator = max(dot(N, X), 0.0) * (1.0 - k) + k;
	denominator = max(denominator, MIN_SAFE_VALUE);

	return numerator / denominator;
}

// Smith
float geometryShadowing(float alpha, vec3 N, vec3 V, vec3 L) {
	return geometryShadowing1(alpha, N, V) * geometryShadowing1(alpha, N, L);
}

// Fresnel-Schlick
vec3 fresnel(vec3 F0, vec3 V, vec3 H) {
	return F0 + (vec3(1.0) - F0) * pow(1 - max(dot(V, H), 0.0), 5.0);
}

vec3 brdf(vec3 albedo, float roughness, float metallic, float alpha, vec3 Lc, vec3 N, vec3 V, vec3 L, vec3 F0) {
	vec3 H = normalize(V + L);

	vec3 kS = fresnel(F0, V, H);
	vec3 kD = vec3(1.0) - kS;

	vec3 lambert = albedo / PI;

	// Cook-Torrance
	vec3 ctNumerator = normalDistribution(alpha, N, H) * geometryShadowing(alpha, N, V, L) * kS;
	float ctDenominator = 4.0 * max(dot(V, N), 0.0) * max(dot(L, N), 0.0);
	ctDenominator = max(ctDenominator, MIN_SAFE_VALUE);
	vec3 ct = ctNumerator / ctDenominator;

	vec3 BRDF = kD * lambert + ct;
	vec3 Lo = BRDF * Lc * max(dot(L, N), 0.0); // outgoing light

	return Lo;
}

vec3 attenuation_radiance(vec3 position, vec3 color) {
	float distance = length(position - fragPos);
	float attenuation = 1.0 / (distance * distance);
	return color * attenuation;
}

//= parallax mapping
// simple
vec2 pm(vec2 uv, vec3 viewDir, sampler2D heightMap) {
	float height = texture(heightMap, uv).r;
	return uv - viewDir.xy * (height * m_pmHeightScale);
}

// steep parallax mapping with occlusion mapping
vec2 spm_o(vec2 uv, vec3 viewDir, sampler2D heightMap) {
	float heightScale = m_pmHeightScale;

	float minLayers = m_pmMinLayers;
	float maxLayers = m_pmMaxLayers;

	// steep parallax mapping
	float NdotV = clamp(dot(normal, viewDir), 0.0, 1.0);

	float layers = mix(maxLayers, minLayers, NdotV * NdotV);
	float layerHeight = 1.0 / layers;
	float currentLayerHeight = 0.0;

	vec2 P = viewDir.xy * heightScale;
	vec2 deltaUV = P / layers;

	vec2 currentUV = uv;
	float currentHeight = texture(heightMap, currentUV).r;

	while(currentLayerHeight < currentHeight) {
		currentUV -= deltaUV;
		currentHeight = texture(heightMap, currentUV).r;
		currentLayerHeight += layerHeight;
	}

	// occlusion parallax mapping
	vec2 prevUV = currentUV + deltaUV;

	float heightAfter = currentHeight - currentLayerHeight;
	float heightBefore = texture(heightMap, prevUV).r - currentLayerHeight + layerHeight;

	// interpolate UVs
	float weight = heightAfter / (heightAfter - heightBefore);
	vec2 finalUV = prevUV * weight + currentUV * (1.0 - weight);

	return finalUV;
}

//= entry point
void main() {
	vec2 uv = vertex.uv;

	vec3 albedo = m_albedo.rgb;
	float opacity = 1.0;
	float roughness = m_roughness;
	float metallic = m_metallic;

	//= parallax mapping
	if(HAS_HEIGHT_TEXTURE) {
		vec3 viewDir = normalize(viewPos - fragPos);

		if(m_pmComplex) {
			uv = spm_o(uv, viewDir, heightTexture);
		} else {
			uv = pm(uv, viewDir, heightTexture);
		}
	}

	//= textures
	if(HAS_ALBEDO_TEXTURE) {
		albedo = pow(texture(albedoTexture, uv).rgb, vec3(gamma));
	}

	if(HAS_METALLIC_TEXTURE) {
		metallic = texture(metallicTexture, uv).r;
	}

	if(HAS_ROUGHNESS_TEXTURE) {
		roughness = texture(roughnessTexture, uv).r;
	}

	//= normal mapping
	vec3 n = normal;

	if(HAS_NORMAL_TEXTURE) {
		n = texture(normalTexture, uv).rgb;
		n = normalize(n * 2.0 - 1.0);
	}

	//= lights
	float alpha = roughness * roughness;
	vec3 N = normalize(n);

	//vec3 F0 = mix(vec3(0.04), albedo, metallic);
	vec3 F0 = vec3(pow((m_ior - 1) / (m_ior + 1), 2));
	F0 = mix(F0, albedo, metallic);

	vec3 Lo = vec3(0.0);

	for(uint i = 0u; i < l_dirCount; i++) {
		DirectionalLight l = l_directionalLights[i];

		vec3 L = TBN * normalize(-l.direction);
		Lo += brdf(albedo, roughness, metallic, alpha, l.color, N, V, L, F0);
	}

	for(uint i = 0u; i < l_pointCount; i++) {
		PointLight l = l_pointLights[i];

		vec3 L = TBN * normalize(l.position - fragPos);
		vec3 radiance = attenuation_radiance(l.position, l.color);

		Lo += brdf(albedo, roughness, metallic, alpha, radiance, N, V, L, F0);
	}

	// TODO (probably) broken and I don't know how to fix it
	for(uint i = 0u; i < l_spotCount; i++) {
		SpotLight l = l_spotLights[i];

		vec3 L = TBN * normalize(l.position - fragPos);
		float theta = dot(L, normalize(TBN * -l.direction));

		// apparently calculating cos on the CPU breaks everything shrug
		float inner = cos(l.cutoff);
		float outer = cos(l.cutoffOuter);

		if(theta > outer) {
			float epsilon = inner - outer;
			float intensity = clamp((theta - outer) / epsilon, 0.0, 1.0);

			vec3 radiance = attenuation_radiance(l.position, l.color) * intensity;
			Lo += brdf(albedo, roughness, metallic, alpha, radiance, N, V, L, F0);
		}
	}

	//= final
	vec3 ambient = vec3(0.0);
	vec3 color = ambient * albedo + Lo;

	// gamma correction (reinhard)
	color = color / (color + vec3(1.0));
	color = pow(color, vec3(1.0 / gamma));

	fragColor = vec4(color, opacity); // simple alpha blending
}
