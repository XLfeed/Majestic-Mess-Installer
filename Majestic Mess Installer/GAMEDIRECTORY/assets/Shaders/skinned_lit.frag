#version 330 core

// Outputs
out vec4 FragColor;

// Inputs from vertex shader
in vec3 v_FragPos;
in vec3 v_Normal;
in vec2 v_TexCoord;
in vec4 v_FragPosLightSpace;

// Material properties
uniform sampler2D u_DiffuseMap;
uniform bool u_HasDiffuseMap;

uniform vec3 u_Color;  // Base color/tint
uniform vec3 u_DiffuseColor;
uniform vec3 u_SpecularColor;
uniform vec3 u_EmissiveColor;
uniform float u_Shininess;
uniform float u_Opacity;
uniform int u_EnableShadows;

// Camera
uniform vec3 u_CameraPos;

// Ambient lighting
uniform vec3 u_AmbientColor;
uniform float u_AmbientIntensity;

// Light structure (matches Light.h)
struct DirectionalLight {
    int type;  // 0 = Directional
    vec3 direction;
    vec3 color;
    float intensity;

    // Shadow properties
    int castsShadows;
    mat4 lightSpaceMatrix;
    float shadowBias;
    float shadowStrength;
    sampler2D shadowMap;
};

struct PointLight {
    int type;  // 1 = Point
    vec3 position;
    vec3 color;
    float intensity;
    float range;

    // Shadow properties
    int castsShadows;
    mat4 lightSpaceMatrix;
    float shadowBias;
    float shadowStrength;
    sampler2D shadowMap;
};

struct SpotLight {
    int type;  // 2 = Spot
    vec3 position;
    vec3 direction;
    vec3 color;
    float intensity;
    float range;
    float innerCutOff;
    float outerCutOff;

    // Shadow properties
    int castsShadows;
    mat4 lightSpaceMatrix;
    float shadowBias;
    float shadowStrength;
    sampler2D shadowMap;
};

// Light arrays (reduced from 50 to 16 to avoid register limit errors)
uniform DirectionalLight u_DirectionalLights[16];
uniform PointLight u_PointLights[16];
uniform SpotLight u_SpotLights[16];

uniform int u_NumDirectionalLights;
uniform int u_NumPointLights;
uniform int u_NumSpotLights;

// Shadow calculation (PCF for soft shadows)
float CalculateShadow(vec4 fragPosLightSpace, sampler2D shadowMap, float bias) {
    // Perspective divide
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;

    // Transform to [0,1] range
    projCoords = projCoords * 0.5 + 0.5;

    // Outside shadow map bounds = no shadow
    if (projCoords.z > 1.0) {
        return 0.0;
    }

    // Get depth from shadow map
    float closestDepth = texture(shadowMap, projCoords.xy).r;
    float currentDepth = projCoords.z;

    // PCF (Percentage Closer Filtering) for soft shadows
    float shadow = 0.0;
    vec2 texelSize = 1.0 / textureSize(shadowMap, 0);
    for (int x = -1; x <= 1; ++x) {
        for (int y = -1; y <= 1; ++y) {
            float pcfDepth = texture(shadowMap, projCoords.xy + vec2(x, y) * texelSize).r;
            shadow += currentDepth - bias > pcfDepth ? 1.0 : 0.0;
        }
    }
    shadow /= 9.0;

    return shadow;
}

// Blinn-Phong lighting calculation
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

    // Shadow
    float shadow = 0.0;
    if (u_EnableShadows == 1 && light.castsShadows == 1) {
        shadow = CalculateShadow(v_FragPosLightSpace, light.shadowMap, light.shadowBias);
        shadow *= light.shadowStrength;
    }

    // Combine
    return (diffuse + specular) * light.intensity * (1.0 - shadow);
}

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

    // Combine
    return (diffuse + specular) * light.intensity * attenuation;
}

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

    // Combine
    return (diffuse + specular) * light.intensity * intensity * attenuation;
}

void main() {
    // === BASE COLOR: Texture or Solid Color ===
    vec4 baseColor;

    if (u_HasDiffuseMap) {
        // Sample diffuse texture
        baseColor = texture(u_DiffuseMap, v_TexCoord);

        // Multiply by u_Color for tinting (supports legacy u_Color uniform)
        baseColor.rgb *= u_Color;
    } else {
        // Use solid color (no texture)
        baseColor = vec4(u_Color, 1.0);
    }

    // Apply diffuse color (supports pbrLit-style uniforms)
    vec3 diffuseColor = baseColor.rgb * u_DiffuseColor;
    vec3 specularColor = u_SpecularColor;

    // Normal
    vec3 normal = normalize(v_Normal);

    // View direction
    vec3 viewDir = normalize(u_CameraPos - v_FragPos);

    // Ambient lighting (use uniform values from LightManager)
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
    result += u_EmissiveColor;

    // Early discard for very low opacity to prevent skybox bleed-through
    // Uses a small threshold to make the transition smooth
    if (u_Opacity < 0.01) {
        discard;
    }

    // Output final color
    FragColor = vec4(result, baseColor.a * u_Opacity);
}
