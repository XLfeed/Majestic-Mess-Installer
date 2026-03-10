#version 330 core

// Uniforms
uniform vec4 u_EntityID;  // Entity ID encoded as RGBA color

// Output color
out vec4 FragColor;

void main() {
    FragColor = u_EntityID;
}
