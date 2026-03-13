#version 330 core

// Input vertex attributes (matches SkinVertex from SkinMeshBinaryFormat.h:32-54)
layout(location = 0) in vec3 position;       // Vertex position
layout(location = 1) in vec3 normal;         // Vertex normal
layout(location = 2) in vec2 texCoord;       // Texture coordinates
layout(location = 3) in vec3 tangent;        // Tangent (for normal mapping)
layout(location = 4) in vec4 jointIds;       // Bone indices (0-255 as float, cast to int in shader)
layout(location = 5) in vec4 weights;        // Bone weights (normalized 0-255 to 0.0-1.0)

// Transform uniforms (standard from existing shaders)
uniform mat4 u_MVP;           // Model-View-Projection matrix
uniform mat4 u_Model;         // Model matrix
uniform mat4 u_View;          // View matrix
uniform mat4 u_Projection;    // Projection matrix

// Skinning uniforms
const int MAX_BONES = 128;
uniform mat4 u_BoneMatrices[MAX_BONES];  // Bone transformation matrices
uniform int u_MaxInfluences = 4;         // LOD support (1-4 bone influences)

// Output to fragment shader (matches standard_specular.vert + TBN for normal mapping)
out vec3 v_FragPos;    // Fragment position in world space
out vec3 v_Normal;     // Normal in world space
out vec2 v_TexCoord;   // Texture coordinates
out vec3 v_ViewPos;    // Camera position in world space
out vec3 v_Tangent;    // Tangent in world space (for normal mapping)
out vec3 v_Binormal;   // Binormal in world space (computed as cross product)

void main() {
    // === SKINNING CALCULATION (Optimized version from AnimationSkin slides) ===
    
    // Blend bone matrices based on weights (avoids full loop for LOD)
    float remainingWeight = 1.0;
    mat4 skinMatrix = mat4(0.0);
    
    // Process first (maxInfluences - 1) bones
    for (int i = 0; i < u_MaxInfluences - 1; ++i) {
        int boneIndex = int(jointIds[i]);
        float weight = weights[i];

        remainingWeight -= weight;
        skinMatrix += weight * u_BoneMatrices[boneIndex];
    }

    // Last bone uses remaining weight (ensures sum = 1.0)
    int lastBoneIndex = int(jointIds[u_MaxInfluences - 1]);
    skinMatrix += remainingWeight * u_BoneMatrices[lastBoneIndex];
    
    // Apply skinning to position
    vec4 skinnedPosition = skinMatrix * vec4(position, 1.0);

    // Apply skinning to normal and tangent (rotation only, via mat3)
    mat3 skinRotation = mat3(skinMatrix);
    vec3 skinnedNormal = normalize(skinRotation * normal);
    vec3 skinnedTangent = normalize(skinRotation * tangent);

    // Compute binormal from normal x tangent (Slide 6 optimization - saves 12 bytes per vertex)
    vec3 skinnedBinormal = normalize(cross(skinnedTangent, skinnedNormal));

    // === STANDARD VERTEX SHADER (same as standard_specular.vert) ===

    // Transform position to world space
    vec4 worldPos = u_Model * skinnedPosition;
    v_FragPos = worldPos.xyz;

    // Transform normal, tangent, binormal to world space (using normal matrix)
    mat3 normalMatrix = mat3(transpose(inverse(u_Model)));
    v_Normal = normalize(normalMatrix * skinnedNormal);
    v_Tangent = normalize(normalMatrix * skinnedTangent);
    v_Binormal = normalize(normalMatrix * skinnedBinormal);

    // Pass texture coordinates
    v_TexCoord = texCoord;

    // Extract camera position from view matrix
    v_ViewPos = inverse(u_View)[3].xyz;

    // Final position (clip space)
    gl_Position = u_MVP * skinnedPosition;
}
