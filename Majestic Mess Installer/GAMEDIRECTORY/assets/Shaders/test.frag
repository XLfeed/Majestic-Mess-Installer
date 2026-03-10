#version 330 core

// Inputs from vertex shader
in vec3 v_FragPos;
in vec3 v_Normal;
in vec2 v_TexCoord;

// Material uniforms
uniform sampler2D u_Texture;
uniform vec3 u_Color;
uniform bool u_HasTexture;

// Camera
uniform vec3 u_CameraPos;

// Ambient Light
uniform vec3 u_AmbientColor;
uniform float u_AmbientIntensity;

// Lighting uniforms
uniform int u_NumDirectionalLights;
uniform int u_NumPointLights;
uniform int u_NumSpotLights;
uniform int u_EnableShadows;

// Directional Light (Sun)
struct DirectionalLight {
    vec3 direction;
    vec3 color;
    float intensity;

// Shadow properties
    int castsShadows;
    mat4 lightSpaceMatrix;
    float shadowBias;
    float shadowStrength;
    // sampler2D shadowMap;

};
uniform DirectionalLight u_DirectionalLights[16];

// Point Light (Chandelier/Bulb)
struct PointLight {
    vec3 position;
    vec3 color;
    float intensity;
    float range;

    // === ADD SHADOW PROPERTIES ===
    int castsShadows;
    mat4 lightSpaceMatrix;
    float shadowBias;
    float shadowStrength;
    // sampler2D shadowMap;

};
uniform PointLight u_PointLights[16];

// Spot Light (Window/Spotlight)
struct SpotLight {

    vec3 position;
    vec3 direction;
    vec3 color;
    float intensity;
    float range;
    float innerCutOff;
    float outerCutOff;

    // === ADD SHADOW PROPERTIES ===
    int castsShadows;
    mat4 lightSpaceMatrix;
    float shadowBias;
    float shadowStrength;
    // sampler2D shadowMap;

};
uniform SpotLight u_SpotLights[16];

// Output
out vec4 FragColor;

float CalculateShadow(vec4 fragPosLightSpace, sampler2D shadowMap, float bias) {
    // Perspective divide
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    
    // Transform to [0,1] range
    projCoords = projCoords * 0.5 + 0.5;
    
    // Outside shadow map bounds = no shadow
    if (projCoords.z > 1.0 || projCoords.x < 0.0 || projCoords.x > 1.0 || 
        projCoords.y < 0.0 || projCoords.y > 1.0) {
        return 0.0;
    }
    
    // Get closest depth from shadow map
    float closestDepth = texture(shadowMap, projCoords.xy).r;
    
    // Get current fragment depth
    float currentDepth = projCoords.z;
    
    // PCF (Percentage Closer Filtering) for soft shadows
    float shadow = 0.0;
    vec2 texelSize = vec2(1.0 / 2048.0);
    for (int x = -1; x <= 1; ++x) {
        for (int y = -1; y <= 1; ++y) {
            float pcfDepth = texture(shadowMap, projCoords.xy + vec2(x, y) * texelSize).r;
            shadow += currentDepth - bias > pcfDepth ? 1.0 : 0.0;
        }
    }
    shadow /= 9.0;  // Average of 9 samples
    
    return shadow;
}


// Calculate directional light contribution
vec3 CalculateDirectionalLight(DirectionalLight light, vec3 normal, vec3 viewDir, vec3 baseColor, vec3 fragPos) {
    vec3 lightDir = normalize(-light.direction);
    
    // Diffuse
    float diff = max(dot(normal, lightDir), 0.0);
    vec3 diffuse = light.color * diff * baseColor;
    
    // Specular (simple Blinn-Phong)
    vec3 halfwayDir = normalize(lightDir + viewDir);
    float spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);
    vec3 specular = light.color * spec * 0.3;
    
    // Calculate shadow
    float shadow = 0.0;
    if (u_EnableShadows == 1 && light.castsShadows == 1) {
        vec4 fragPosLightSpace = light.lightSpaceMatrix * vec4(fragPos, 1.0);
        shadow = 0.0;
        shadow *= light.shadowStrength;
    }
    
    // Apply shadow (1.0 - shadow means 1.0 = lit, 0.0 = shadowed)
    return (diffuse + specular) * light.intensity * (1.0 - shadow);
}
// Calculate point light contribution
vec3 CalculatePointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 baseColor) {
    vec3 lightDir = normalize(light.position - fragPos);
    float distance = length(light.position - fragPos);
    
    // Range check
    if (distance > light.range) {
        return vec3(0.0);
    }
    
    // Attenuation
    float attenuation = 1.0 / (1.0 + 0.09 * distance + 0.032 * (distance * distance));
    
    // Diffuse
    float diff = max(dot(normal, lightDir), 0.0);
    vec3 diffuse = light.color * diff * baseColor;
    
    // Specular
    vec3 halfwayDir = normalize(lightDir + viewDir);
    float spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);
    vec3 specular = light.color * spec * 0.3;
    
    // Shadows
    float shadow = 0.0;
    if (u_EnableShadows == 1 && light.castsShadows == 1) {
        vec4 fragPosLightSpace = light.lightSpaceMatrix * vec4(fragPos, 1.0);
        shadow = 0.0;
        shadow *= light.shadowStrength;
    }

    return (diffuse + specular) * light.intensity * attenuation * (1.0 - shadow);
}

// Calculate spot light contribution
vec3 CalculateSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 baseColor) {
    vec3 lightDir = normalize(light.position - fragPos);
    float distance = length(light.position - fragPos);

    // Range check
    if (distance > light.range) {
        return vec3(0.0);
    }

    // Spot cone check (safe)
  vec3 spotDir = normalize(-light.direction);
  float theta = dot(lightDir, spotDir);

  // Ensure inner > outer (they are cosines), and avoid divide‑by‑zero
  float epsilon = max(light.innerCutOff - light.outerCutOff, 1e-4);
  float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);

  if (intensity <= 0.0) {
      return vec3(0.0);
  }

    // Attenuation
    float attenuation = 1.0 / (1.0 + 0.09 * distance + 0.032 * (distance * distance));

    // Diffuse
    float diff = max(dot(normal, lightDir), 0.0);
    vec3 diffuse = light.color * diff * baseColor;

    // Specular
    vec3 halfwayDir = normalize(lightDir + viewDir);
    float spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);
    vec3 specular = light.color * spec * 0.3;

    // Shadows (disabled in test)
    float shadow = 0.0;

    return (diffuse + specular) * light.intensity * intensity * attenuation * (1.0 - shadow);
}

 vec3 safeNormalize(vec3 v) {
      float len = length(v);
      if (len < 1e-6) return vec3(0.0, 1.0, 0.0);
      return v / len;
  }

void main() {
      vec4 texColor = u_HasTexture ? texture(u_Texture, v_TexCoord) : vec4(1.0);
      vec3 baseColor = texColor.rgb * u_Color;

      vec3 normal = safeNormalize(v_Normal);
      vec3 viewDir = safeNormalize(u_CameraPos - v_FragPos);

      vec3 ambient = u_AmbientColor * u_AmbientIntensity * baseColor;
      vec3 result = ambient;

      int dirCount = clamp(u_NumDirectionalLights, 0, 16);
      int pointCount = clamp(u_NumPointLights, 0, 16);
      int spotCount = clamp(u_NumSpotLights, 0, 16);

      for (int i = 0; i < dirCount; ++i) {
          result += CalculateDirectionalLight(u_DirectionalLights[i], normal, viewDir, baseColor, v_FragPos);
      }
      for (int i = 0; i < pointCount; ++i) {
          result += CalculatePointLight(u_PointLights[i], normal, v_FragPos, viewDir, baseColor);
      }
      for (int i = 0; i < spotCount; ++i) {
          result += CalculateSpotLight(u_SpotLights[i], normal, v_FragPos, viewDir, baseColor);
      }

      FragColor = vec4(result, texColor.a);
  }