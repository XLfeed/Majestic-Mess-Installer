#version 330 core

// Input vertex attributes
layout(location = 0) in vec3 position;
layout(location = 1) in vec3 normal;
layout(location = 2) in vec2 texCoord;

// Uniform matrices
uniform mat4 u_MVP;           // Model-View-Projection matrix
uniform mat4 u_Model;         // Model matrix (for world space position)
uniform mat4 u_View;          // View matrix
uniform mat4 u_Projection;    // Projection matrix
uniform mat4 u_ViewProjection;

// Output to fragment shader
out vec3 v_FragPos;    // Fragment position in world space
out vec3 v_Normal;     // Normal in world space
out vec2 v_TexCoord;   // Texture coordinates
out vec3 v_ViewPos;    // Camera position in world space

void main() {
    // Transform position to world space
    vec4 worldPos = u_Model * vec4(position, 1.0);
    v_FragPos = worldPos.xyz;

    // Transform normal to world space (using normal matrix)
    v_Normal = mat3(transpose(inverse(u_Model))) * normal;

    // Pass texture coordinates
    v_TexCoord = texCoord;

    // Extract camera position from view matrix
    v_ViewPos = inverse(u_View)[3].xyz;

    // Final position
    gl_Position = u_MVP * vec4(position, 1.0);
}
