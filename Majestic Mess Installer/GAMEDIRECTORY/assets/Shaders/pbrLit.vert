#version 330 core

layout(location = 0) in vec3 a_position;
layout(location = 1) in vec3 a_normal;
layout(location = 2) in vec2 a_texCoord;

out vec3 v_FragPos;
out vec3 v_Normal;
out vec2 v_TexCoord;

uniform mat4 u_Model;
uniform mat4 u_View;
uniform mat4 u_Projection;
uniform mat4 u_ViewProjection;
uniform mat4 u_MVP;

void main() {
    // World space position
    v_FragPos = vec3(u_Model * vec4(a_position, 1.0));
    
    // Transform normal to world space (use normal matrix for non-uniform scaling)
    v_Normal = mat3(transpose(inverse(u_Model))) * a_normal;
    
    // Pass texture coordinates
    v_TexCoord = a_texCoord;
    
    // NOTE: Shadow coordinates are now calculated per-light in the fragment shader
    // This is necessary because each light can have different light-space matrices
    
    // Final position
    gl_Position = u_MVP * vec4(a_position, 1.0);
}
