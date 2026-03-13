#version 330 core

// Vertex attributes
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec3 aNormal;
layout(location = 2) in vec2 aTexCoord;
layout(location = 3) in vec3 aTangent;
layout(location = 4) in vec4 aBoneIndices;   // 4 bone indices (0-255 as float, cast to int in shader)
layout(location = 5) in vec4 aWeights;       // 4 weights (normalized 0.0-1.0)

// Uniforms
uniform mat4 u_MVP;           // Model-View-Projection matrix
uniform mat4 u_Model;         // Model matrix (for normal transformation)

// Bone matrices (up to 64 bones max)
const int MAX_BONES = 64;
uniform mat4 u_BoneMatrices[MAX_BONES];

// Outputs to fragment shader
out vec3 v_Normal;
out vec2 v_TexCoord;

void main() {
    // Weights are already normalized (0.0-1.0) by OpenGL
    vec4 weights = aWeights;

    // Compute skinned position
    vec4 skinnedPosition = vec4(0.0);
    vec3 skinnedNormal = vec3(0.0);

    for (int i = 0; i < 4; i++) {
        int boneIndex = int(aBoneIndices[i]);  // Explicit cast from uint to int
        float weight = weights[i];

        if (weight > 0.0 && boneIndex >= 0 && boneIndex < MAX_BONES) {
            mat4 boneMatrix = u_BoneMatrices[boneIndex];
            skinnedPosition += boneMatrix * vec4(aPosition, 1.0) * weight;
            skinnedNormal += mat3(boneMatrix) * aNormal * weight;
        }
    }

    // If no weights were applied, use original position (shouldn't happen)
    if (skinnedPosition.w == 0.0) {
        skinnedPosition = vec4(aPosition, 1.0);
        skinnedNormal = aNormal;
    }

    // Transform normal using model matrix
    v_Normal = normalize(mat3(u_Model) * skinnedNormal);

    // Pass through texture coordinates
    v_TexCoord = aTexCoord;

    // Final position
    gl_Position = u_MVP * skinnedPosition;
}
