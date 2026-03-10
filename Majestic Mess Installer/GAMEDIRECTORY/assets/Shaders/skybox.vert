#version 330 core

/*
   Skybox Vertex Shader
   Removes translation from view matrix to keep skybox centered on camera
*/

layout(location = 0) in vec3 aPosition;

out vec3 TexCoords;

uniform mat4 projection;
uniform mat4 view;

void main()
{
    // Pass texture coordinates directly (no flipping)
    TexCoords = aPosition;

    // Remove translation from view matrix (keep only rotation)
    mat4 rotView = mat4(mat3(view));
    vec4 pos = projection * rotView * vec4(aPosition, 1.0);

    // Set depth to maximum (z = w means depth will be 1.0 after perspective divide)
    gl_Position = vec4(pos.xy, pos.w, pos.w);
}
