#version 330 core

in vec2 v_TexCoord;
out vec4 FragColor;

uniform sampler2D u_ScreenTexture;
uniform float u_Gamma;

void main() {
    vec4 color = texture(u_ScreenTexture, v_TexCoord);
    color.rgb = pow(color.rgb, vec3(1.0 / u_Gamma));
    FragColor = color;
}
