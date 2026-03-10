#version 330 core

layout(location = 0) in vec3 a_position;
layout(location = 2) in vec2 a_texCoord;

out vec2 v_TexCoord;

void main() {
    // Flip Y: pl_mpeg stores rows top-first; OpenGL textures have origin bottom-left.
    v_TexCoord = vec2(a_texCoord.x, 1.0 - a_texCoord.y);
    gl_Position = vec4(a_position.xy * 2.0, 0.0, 1.0);
}
