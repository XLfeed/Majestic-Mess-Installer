#version 330 core

in vec2 v_TexCoord;
out vec4 FragColor;

uniform sampler2D u_Texture;
uniform vec4 u_Color;

void main()
{
    vec4 texColor = texture(u_Texture, v_TexCoord);

    // Apply tint color (your SpriteRendererComponent::color)
    FragColor = texColor * u_Color;

    // Optional: discard fully transparent pixels
    // if (FragColor.a < 0.01) discard;
}
