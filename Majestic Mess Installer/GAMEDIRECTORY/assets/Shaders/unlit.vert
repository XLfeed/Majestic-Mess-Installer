#version 330 core

// Input vertex attributes
layout(location = 0) in vec3 position;
layout(location = 1) in vec3 normal;
layout(location = 2) in vec2 texCoord;

// Uniform matrices
uniform mat4 u_MVP;

// Output to fragment shader
out vec2 v_TexCoord;

void main() {
    v_TexCoord = texCoord;
    gl_Position = u_MVP * vec4(position, 1.0);
}
