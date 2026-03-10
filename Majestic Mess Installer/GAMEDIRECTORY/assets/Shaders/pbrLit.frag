#version 330 core

// Outputs
out vec4 FragColor;

// Inputs from vertex shader
in vec3 v_FragPos;
in vec3 v_Normal;
in vec2 v_TexCoord;

// Material properties
uniform sampler2D u_DiffuseMap;
uniform sampler2D u_SpecularMap;
uniform sampler2D u_NormalMap;
uniform sampler2D u_EmissiveMap;
uniform sampler2D u_OpacityMap;

uniform vec3 u_DiffuseColor;
uniform vec3 u_SpecularColor;
uniform vec3 u_EmissiveColor;
uniform float u_Shininess;
uniform float u_Opacity;
uniform int u_EnableShadows;

// Camera
uniform vec3 u_CameraPos;

// Ambient light
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
// This version takes the light-space matrix and calculates the transformation
float CalculateShadow(vec3 fragPos, mat4 lightSpaceMatrix, sampler2D shadowMap, float bias) {
    // Transform fragment position to light space
    vec4 fragPosLightSpace = lightSpaceMatrix * vec4(fragPos, 1.0);
    
    // Perspective divide
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    
    // Transform to [0,1] range
    projCoords = projCoords * 0.5 + 0.5;
    
    // Outside shadow map bounds = no shadow
    if (projCoords.z > 1.0 || projCoords.x < 0.0 || projCoords.x > 1.0 || 
        projCoords.y < 0.0 || projCoords.y > 1.0) {
        return 0.0;
    }
    
    // Get current depth
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

// Blinn-Phong lighting calculation for directional light
vec3 CalculateDirectionalLight(DirectionalLight light, vec3 normal, vec3 viewDir, vec3 diffuseColor, vec3 specularColor) {
    vec3 lightDir = normalize(-light.direction);

    // Diffuse
    float diff = max(dot(normal, lightDir), 0.0);
    vec3 diffuse = light.color * diff * diffuseColor;

    // Specular (Blinn-Phong)
    vec3 halfwayDir = normalize(lightDir + viewDir);
    float spec = pow(max(dot(normal, halfwayDir), 0.0), u_Shininess * 128.0);
    vec3 specular = light.color * spec * specularColor;

    // Shadow
    float shadow = 0.0;
    if (u_EnableShadows == 1 && light.castsShadows == 1) {
        shadow = CalculateShadow(v_FragPos, light.lightSpaceMatrix, light.shadowMap, light.shadowBias);
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

    // Specular
    vec3 halfwayDir = normalize(lightDir + viewDir);
    float spec = pow(max(dot(normal, halfwayDir), 0.0), u_Shininess * 128.0);
    vec3 specular = light.color * spec * specularColor;

    // Attenuation
    float distance = length(light.position - fragPos);
    float attenuation = 1.0 / (1.0 + 0.09 * distance + 0.032 * (distance * distance));

    // Range cutoff
    if (distance > light.range) {
        attenuation = 0.0;
    }

    // Shadow calculation for point light
    float shadow = 0.0;
    if (u_EnableShadows == 1 && light.castsShadows == 1) {
        shadow = CalculateShadow(v_FragPos, light.lightSpaceMatrix, light.shadowMap, light.shadowBias);
        shadow *= light.shadowStrength;
    }

    // Combine
    return (diffuse + specular) * light.intensity * attenuation * (1.0 - shadow);
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

    // Specular
    vec3 halfwayDir = normalize(lightDir + viewDir);
    float spec = pow(max(dot(normal, halfwayDir), 0.0), u_Shininess * 128.0);
    vec3 specular = light.color * spec * specularColor;

    // Attenuation
    float distance = length(light.position - fragPos);
    float attenuation = 1.0 / (1.0 + 0.09 * distance + 0.032 * (distance * distance));

    // Shadow calculation for spot light
    float shadow = 0.0;
    if (u_EnableShadows == 1 && light.castsShadows == 1) {
        shadow = CalculateShadow(v_FragPos, light.lightSpaceMatrix, light.shadowMap, light.shadowBias);
        shadow *= light.shadowStrength;
    }

    // Combine
    return (diffuse + specular) * light.intensity * intensity * attenuation * (1.0 - shadow);
}

void main() {
    // Sample textures
    vec3 diffuseColor = texture(u_DiffuseMap, v_TexCoord).rgb * u_DiffuseColor;
    vec3 specularColor = texture(u_SpecularMap, v_TexCoord).rgb * u_SpecularColor;
    vec3 emissiveColor = texture(u_EmissiveMap, v_TexCoord).rgb * u_EmissiveColor;
    float opacity = texture(u_OpacityMap, v_TexCoord).r * u_Opacity;
    
    // Normal
    vec3 normal = normalize(v_Normal);
    
    // View direction
    vec3 viewDir = normalize(u_CameraPos - v_FragPos);
    
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
    result += emissiveColor;
    
    // Output final color
    FragColor = vec4(result, opacity);
}
