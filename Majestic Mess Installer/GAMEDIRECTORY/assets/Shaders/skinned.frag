#version 330 core

// Input from vertex shader
in vec3 v_Normal;
in vec2 v_TexCoord;

// Texture uniforms (matches Material.cpp texture slot system)
uniform sampler2D u_DiffuseMap;     // Diffuse/Albedo texture (slot 0)
uniform bool u_HasDiffuseMap;       // Flag: does material have diffuse texture?

// Color uniforms (fallback when no texture)
uniform vec3 u_Color;               // Material color (default white)

// Output color
out vec4 FragColor;

void main() {
    // === BASE COLOR: Texture or Solid Color ===
    vec4 baseColor;

    if (u_HasDiffuseMap) {
        // Sample diffuse texture
        baseColor = texture(u_DiffuseMap, v_TexCoord);

        // Optional: Multiply by u_Color for tinting
        baseColor.rgb *= u_Color;
    } else {
        // Use solid color (no texture)
        baseColor = vec4(u_Color, 1.0);
    }

    // === SIMPLE DIRECTIONAL LIGHTING ===
    vec3 lightDir = normalize(vec3(0.5, 1.0, 0.3));
    float diff = max(dot(normalize(v_Normal), lightDir), 0.3);  // Min 0.3 for ambient

    // === FINAL COLOR ===
    vec3 finalColor = baseColor.rgb * diff;
    FragColor = vec4(finalColor, baseColor.a);  // Preserve alpha from texture
}
