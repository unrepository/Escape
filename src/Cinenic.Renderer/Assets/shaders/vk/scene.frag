#version 450
#define PI radians(180)
#define MIN_SAFE_VALUE 0.000001

const float gamma = 2.2;

struct Vertex {
	vec3 position;
	vec3 normal;
	vec3 tangent;
	vec3 bitangent;
	vec2 uv;
};

struct Material {
	vec4 albedo;
	float roughness;
	float metallic;
	float ior;
};

//= lights
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

//= buffers
layout(set = 1, binding = 0, std430) readonly buffer CameraBuffer {
	mat4 projection;
	mat4 inverseProjection;
	mat4 view;
	mat4 inverseView;
	vec3 position;
	float aspectRatio;
} cameraData;

layout(set = 6, binding = 10) readonly buffer LightCountData {
	uint dirCount;
	uint pointCount;
	uint spotCount;
} lightData;

layout(set = 7, binding = 11) readonly buffer DirectionalLightBuffer {
	DirectionalLight lights[];
} dirLightData;

layout(set = 8, binding = 12) readonly buffer PointLightBuffer {
	PointLight lights[];
} pointLightData;

layout(set = 9, binding = 13) readonly buffer SpotLightBuffer {
	SpotLight lights[];
} spotLightData;

layout(push_constant) uniform PushConstants {
	uint vertexOffset;
	uint indexOffset;
	uint materialOffset;
	uint matrixOffset;
	int albedoTextureIndex;
	int normalTextureIndex;
	int metallicTextureIndex;
	int roughnessTextureIndex;
	int displacementTextureIndex;
} pc;

layout(set = 0, binding = 0) uniform sampler2D textures[1024];

layout(location = 0) out vec4 fragColor;
layout(location = 1) in Vertex vertex;
layout(location = 10) flat in Material material;
layout(location = 20) in vec3 fragPos;
layout(location = 21) in vec3 viewPos;
layout(location = 22) in vec3 normal;
layout(location = 23) in mat3 TBN;
layout(location = 26) in vec3 V;

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
// steep parallax mapping with occlusion mapping
vec2 spm_o(vec2 uv, vec3 viewDir, sampler2D heightMap, bool opm) {
	const float heightScale = -0.07; // TODO
	
	const float minLayers = 8;
	const float maxLayers = 64;
	
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
	if(opm) {
		vec2 prevUV = currentUV + deltaUV;

		float heightAfter = currentHeight - currentLayerHeight;
		float heightBefore = texture(heightMap, prevUV).r - currentLayerHeight + layerHeight;

		// interpolate UVs
		float weight = heightAfter / (heightAfter - heightBefore);
		vec2 finalUV = prevUV * weight + currentUV * (1.0 - weight);
		
		return finalUV;
	}
	
	return currentUV;
}

//= entry point
void main() {
	vec2 uv = vertex.uv;

	vec3 albedo = material.albedo.rgb;
	float opacity = material.albedo.a;
	float roughness = material.roughness;
	float metallic = material.metallic;

	//= parallax mapping
	if(pc.displacementTextureIndex > 0) {
		uv = spm_o(uv, normalize(viewPos - fragPos), textures[pc.displacementTextureIndex], true);
	}

	//= textures
	if(pc.albedoTextureIndex > 0) {
		albedo *= pow(texture(textures[pc.albedoTextureIndex], uv).rgb, vec3(gamma));
	}

	if(pc.metallicTextureIndex > 0) {
		metallic = texture(textures[pc.metallicTextureIndex], uv).r;
	}

	if(pc.roughnessTextureIndex > 0) {
		roughness = texture(textures[pc.roughnessTextureIndex], uv).r;
	}
	
	//= normal mapping
	vec3 n = normal;
	
	if(pc.normalTextureIndex > 0) {
		n = texture(textures[pc.normalTextureIndex], uv).rgb;
		n = normalize((n * 2.0 - 1.0));
	}

	//= lights
	float alpha = roughness * roughness;
	
	vec3 N = normalize(n);

	vec3 F0 = vec3(0.04);
	F0 = mix(F0, albedo, metallic);

	vec3 Lo = vec3(0.0);

	for(int i = 0; i < lightData.dirCount; i++) {
		DirectionalLight l = dirLightData.lights[i];

		vec3 L = TBN * normalize(-l.direction);
		Lo += brdf(albedo, roughness, metallic, alpha, l.color, N, V, L, F0);
	}

	for(int i = 0; i < lightData.pointCount; i++) {
		PointLight l = pointLightData.lights[i];

		vec3 L = TBN * normalize(l.position - fragPos);
		vec3 radiance = attenuation_radiance(l.position, l.color);

		Lo += brdf(albedo, roughness, metallic, alpha, radiance, N, V, L, F0);
	}

	// TODO broken and I don't know how to fix it
	for(int i = 0; i < lightData.spotCount; i++) {
		SpotLight l = spotLightData.lights[i];

		vec3 L = TBN * normalize(l.position - fragPos);
		
		//float inner = min(l.cutoff, l.cutoffOuter);
		//float outer = max(l.cutoff, l.cutoffOuter);
		float inner = l.cutoff;
		float outer = l.cutoffOuter;
		
		float theta = dot(L, normalize(-l.direction));
		
		if(theta > inner) {
			float epsilon = inner - outer;
			float intensity = clamp((theta - outer) / epsilon, 0.0, 1.0);
			
			vec3 radiance = attenuation_radiance(l.position, l.color) * intensity;
			Lo += brdf(albedo, roughness, metallic, alpha, radiance, N, V, L, F0);
		}
	}
	
	//= final
	vec3 ambient = vec3(0.004);
	vec3 color = ambient * albedo + Lo;

	// gamma correction (reinhard)
	color = color / (color + vec3(1.0));
	color = pow(color, vec3(1.0 / gamma));

	fragColor = vec4(color, opacity); // simple alpha blending
}
