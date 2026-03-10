#version 330 core

// Input from vertex shader
in vec3 v_FragPos;
in vec3 v_Normal;
in vec2 v_TexCoord;
in vec3 v_ViewPos;

// Texture samplers
uniform sampler2D u_DiffuseMap;
uniform sampler2D u_SpecularMap;
uniform sampler2D u_NormalMap;
uniform sampler2D u_EmissiveMap;
uniform sampler2D u_OpacityMap;

// Texture flags
uniform int u_HasDiffuseMap;
uniform int u_HasSpecularMap;
uniform int u_HasNormalMap;
uniform int u_HasEmissiveMap;
uniform int u_HasOpacityMap;

// Material colors
uniform vec3 u_DiffuseColor;
uniform vec3 u_SpecularColor;
uniform vec3 u_EmissiveColor;

// Material properties
uniform float u_Shininess;  // 0-1 (normalized from MTL's 0-1000)
uniform float u_Opacity;    // 0-1

// Camera
uniform vec3 u_CameraPos;

// Ambient lighting
uniform vec3 u_AmbientColor;
uniform float u_AmbientIntensity;

// Light structures (matches skinned_lit.frag)
struct DirectionalLight {
    vec3 direction;
    vec3 color;
    float intensity;
    float range;
};

struct PointLight {
    vec3 position;
    vec3 color;
    float intensity;
    float range;
};

struct SpotLight {
    vec3 position;
    vec3 direction;
    vec3 color;
    float intensity;
    float range;
    float innerCutOff;
    float outerCutOff;
};

// Light arrays
uniform DirectionalLight u_DirectionalLights[16];
uniform PointLight u_PointLights[16];
uniform SpotLight u_SpotLights[16];

uniform int u_NumDirectionalLights;
uniform int u_NumPointLights;
uniform int u_NumSpotLights;

// Output color
out vec4 FragColor;

// Blinn-Phong lighting calculation for directional light
vec3 CalculateDirectionalLight(DirectionalLight light, vec3 normal, vec3 viewDir, vec3 diffuseColor, vec3 specularColor) {
    vec3 lightDir = normalize(-light.direction);

    // Diffuse
    float diff = max(dot(normal, lightDir), 0.0);
    vec3 diffuse = light.color * diff * diffuseColor;

    // Specular (Blinn-Phong) - only if shininess > 0.01
    vec3 specular = vec3(0.0);
    if (diff > 0.0 && u_Shininess > 0.01) {
        vec3 halfwayDir = normalize(lightDir + viewDir);
        float specularExponent = max(u_Shininess * 128.0, 1.0);
        float spec = pow(max(dot(normal, halfwayDir), 0.0), specularExponent);
        // Scale specular by shininess to reduce washout at low shininess values
        specular = light.color * spec * specularColor * smoothstep(0.0, 0.3, u_Shininess);
    }

    return (diffuse + specular) * light.intensity;
}

// Blinn-Phong lighting calculation for point light
vec3 CalculatePointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 diffuseColor, vec3 specularColor) {
    vec3 lightDir = normalize(light.position - fragPos);

    // Diffuse
    float diff = max(dot(normal, lightDir), 0.0);
    vec3 diffuse = light.color * diff * diffuseColor;

    // Specular (Blinn-Phong) - only if shininess > 0.01
    vec3 specular = vec3(0.0);
    if (diff > 0.0 && u_Shininess > 0.01) {
        vec3 halfwayDir = normalize(lightDir + viewDir);
        float specularExponent = max(u_Shininess * 128.0, 1.0);
        float spec = pow(max(dot(normal, halfwayDir), 0.0), specularExponent);
        // Scale specular by shininess to reduce washout at low shininess values
        specular = light.color * spec * specularColor * smoothstep(0.0, 0.3, u_Shininess);
    }

    // Attenuation
    float distance = length(light.position - fragPos);
    float attenuation = 1.0 / (1.0 + 0.09 * distance + 0.032 * (distance * distance));

    // Range cutoff
    if (distance > light.range) {
        attenuation = 0.0;
    }

    return (diffuse + specular) * light.intensity * attenuation;
}

// Blinn-Phong lighting calculation for spot light
vec3 CalculateSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 diffuseColor, vec3 specularColor) {
    vec3 lightDir = normalize(light.position - fragPos);

    // Spot cutoff
    vec3 spotDir = normalize(-light.direction);
    float theta = dot(lightDir, spotDir);
    float epsilon = max(light.innerCutOff - light.outerCutOff, 1e-4);
        float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);

    if (intensity <= 0.0) {
        return vec3(0.0);
    }

    // Diffuse
    float diff = max(dot(normal, lightDir), 0.0);
    vec3 diffuse = light.color * diff * diffuseColor;

    // Specular (Blinn-Phong) - only if shininess > 0.01
    vec3 specular = vec3(0.0);
    if (diff > 0.0 && u_Shininess > 0.01) {
        vec3 halfwayDir = normalize(lightDir + viewDir);
        float specularExponent = max(u_Shininess * 128.0, 1.0);
        float spec = pow(max(dot(normal, halfwayDir), 0.0), specularExponent);
        // Scale specular by shininess to reduce washout at low shininess values
        specular = light.color * spec * specularColor * smoothstep(0.0, 0.3, u_Shininess);
    }

    // Attenuation
    float distance = length(light.position - fragPos);
    float attenuation = 1.0 / (1.0 + 0.09 * distance + 0.032 * (distance * distance));

    return (diffuse + specular) * light.intensity * intensity * attenuation;
}

void main() {
    // === 1. Sample Textures ===
    vec4 diffuseTexColor = u_HasDiffuseMap == 1 ? texture(u_DiffuseMap, v_TexCoord) : vec4(1.0);
    vec3 specularTexColor = u_HasSpecularMap == 1 ? texture(u_SpecularMap, v_TexCoord).rgb : vec3(1.0);
    vec3 emissiveTexColor = u_HasEmissiveMap == 1 ? texture(u_EmissiveMap, v_TexCoord).rgb : vec3(0.0);
    float opacity = u_HasOpacityMap == 1 ? texture(u_OpacityMap, v_TexCoord).r : u_Opacity;

    // Alpha test for fully transparent pixels only
    if (diffuseTexColor.a < 0.01) {
        discard;
    }

    // Early discard for very low opacity to prevent skybox bleed-through
    // Uses a small threshold to make the transition smooth
    if (opacity < 0.01) {
        discard;
    }

    // === 2. Calculate Normal ===
    vec3 normal = normalize(v_Normal);
    // TODO: Normal mapping support (requires tangent space calculation)
    // if (u_HasNormalMap == 1) { ... }

    // === 3. Prepare Colors ===
    vec3 diffuseColor = diffuseTexColor.rgb * u_DiffuseColor;
    vec3 specularColor = u_SpecularColor * specularTexColor;

    // View direction
    vec3 viewDir = normalize(u_CameraPos - v_FragPos);

    // === 4. Lighting Calculation ===
    // Ambient lighting
    vec3 ambient = u_AmbientColor * u_AmbientIntensity * diffuseColor;

    // Accumulate lighting from all lights
    vec3 result = ambient;

    // Directional lights
    for (int i = 0; i < u_NumDirectionalLights; ++i) {
        result += CalculateDirectionalLight(u_DirectionalLights[i], normal, viewDir, diffuseColor, specularColor);
    }

    // Point lights
    for (int i = 0; i < u_NumPointLights; ++i) {
        result += CalculatePointLight(u_PointLights[i], normal, v_FragPos, viewDir, diffuseColor, specularColor);
    }

    // Spot lights
    for (int i = 0; i < u_NumSpotLights; ++i) {
        result += CalculateSpotLight(u_SpotLights[i], normal, v_FragPos, viewDir, diffuseColor, specularColor);
    }

    // Add emissive
    result += emissiveTexColor * u_EmissiveColor;

    // === 5. Output with opacity ===
    FragColor = vec4(result, diffuseTexColor.a * opacity);
}
