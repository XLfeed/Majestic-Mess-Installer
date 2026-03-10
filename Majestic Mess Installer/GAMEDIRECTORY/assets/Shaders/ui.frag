#version 330 core

// Inputs from vertex shader
in vec2 v_TexCoord;

// Output color
out vec4 FragColor;

// Uniforms
uniform sampler2D u_Texture;    // Texture sampler
uniform vec4 u_Color;           // Tint color (RGBA)
uniform bool u_UseTexture;      // Whether to use texture

void main() {
    // Sample texture if enabled, otherwise use white
    vec4 texColor = vec4(1.0);
    if (u_UseTexture) {
        texColor = texture(u_Texture, v_TexCoord);
    }

    // Multiply by tint color
    FragColor = texColor * u_Color;
}
