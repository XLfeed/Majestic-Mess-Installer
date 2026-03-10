#version 330 core

/*
   Skybox Fragment Shader
   Samples cubemap texture with HDR tonemapping
*/

out vec4 FragColor;

in vec3 TexCoords;

uniform samplerCube skybox;
uniform float exposure = 0.3;  // Exposure control (0.3 = bright daytime, 1.0 = neutral, 2.0 = dark/indoor)

// ACES Filmic Tonemapping (Uncharted 2 approximation)
vec3 ACESFilm(vec3 x) {
    float a = 2.51;
    float b = 0.03;
    float c = 2.43;
    float d = 0.59;
    float e = 0.14;
    return clamp((x * (a * x + b)) / (x * (c * x + d) + e), 0.0, 1.0);
}

void main()
{
    // Sample the cubemap (HDR values)
    vec3 color = texture(skybox, TexCoords).rgb;

    // Apply exposure
    color *= exposure;

    // Tonemap HDR to LDR (brings bright sun into displayable range)
    color = ACESFilm(color);

    // Gamma correction
    color = pow(color, vec3(1.0 / 2.2));

    FragColor = vec4(color, 1.0);
}
