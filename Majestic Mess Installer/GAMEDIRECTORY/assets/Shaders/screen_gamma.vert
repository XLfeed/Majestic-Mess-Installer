#version 330 core

layout(location = 0) in vec3 a_position;
layout(location = 2) in vec2 a_texCoord;

out vec2 v_TexCoord;

void main() {
    v_TexCoord = a_texCoord;
    vec2 pos = a_position.xy * 2.0; // quad is [-0.5,0.5], scale to [-1,1]
    gl_Position = vec4(pos, 0.0, 1.0);
}
