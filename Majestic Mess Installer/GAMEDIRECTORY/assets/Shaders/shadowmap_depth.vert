#version 330 core
layout(location = 0) in vec3 a_Position;

uniform mat4 u_LightSpaceMatrix;  // Light's view-projection matrix
uniform mat4 u_Model;              // Model matrix

void main() {
    gl_Position = u_LightSpaceMatrix * u_Model * vec4(a_Position, 1.0);
}
