#version 330 core

// Inputs from vertex shader
in vec2 v_TexCoord;
in vec4 v_Color;

// Output color
out vec4 FragColor;

// Uniforms
uniform sampler2D u_FontAtlas;   // Font atlas texture (single channel)

void main() {
    // Sample the font atlas (grayscale/alpha)
    float alpha = texture(u_FontAtlas, v_TexCoord).r;

    // Apply vertex color with alpha from font atlas
    FragColor = vec4(v_Color.rgb, v_Color.a * alpha);

    // Discard fully transparent fragments for better performance
    if (FragColor.a < 0.01) {
        discard;
    }
}
