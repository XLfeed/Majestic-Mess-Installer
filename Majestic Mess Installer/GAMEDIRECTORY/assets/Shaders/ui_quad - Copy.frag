#version 450

in vec2 v_TexCoord;
out vec4 FragColor;

uniform sampler2D u_Texture;
uniform vec4 u_Color;

void main()
{
    vec4 tex = texture(u_Texture, v_TexCoord);
    FragColor = tex * u_Color;

    // If no texture, output just color
    if (tex.a == 0.0)
        FragColor = u_Color;
}
