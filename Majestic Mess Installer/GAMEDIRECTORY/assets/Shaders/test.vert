#version 330 core

// Vertex attributes
layout(location = 0) in vec3 a_position;
layout(location = 1) in vec3 a_normal;
layout(location = 2) in vec2 a_texCoord;

// Uniforms
uniform mat4 u_MVP;
uniform mat4 u_Model;
uniform mat4 u_ViewProjection;
uniform mat4 u_View;
uniform mat4 u_Projection;

// Outputs
out vec3 v_FragPos;    // World position
out vec3 v_Normal;     // World normal
out vec2 v_TexCoord;

void main() {
    // World space position
    vec4 worldPos = u_Model * vec4(a_position, 1.0);
    v_FragPos = worldPos.xyz;
    
    // Transform normal to world space
    v_Normal = mat3(transpose(inverse(u_Model))) * a_normal;
    
    // Pass texture coordinates
    v_TexCoord = a_texCoord;
    
    // Final position
    gl_Position = u_MVP * vec4(a_position, 1.0);
}
