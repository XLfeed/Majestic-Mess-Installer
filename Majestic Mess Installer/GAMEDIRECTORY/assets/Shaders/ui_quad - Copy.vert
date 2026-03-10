#version 450

layout(location = 0) in vec2 a_Pos;
layout(location = 1) in vec2 a_TexCoord;

uniform mat4 u_Projection;
uniform mat4 u_Model;

out vec2 v_TexCoord;

void main()
{
    v_TexCoord = a_TexCoord;
    gl_Position = u_Projection * u_Model * vec4(a_Pos, 0.0, 1.0);
}