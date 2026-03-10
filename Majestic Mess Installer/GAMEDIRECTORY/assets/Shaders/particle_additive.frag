#version 330 core

in vec2 v_TexCoord;

uniform sampler2D u_Texture;
uniform sampler2D u_DepthTexture;
uniform int u_HasTexture;
uniform vec3 u_Color;
uniform float u_Opacity;
uniform bool u_SoftParticles;
uniform float u_Softness;
uniform float u_Near;
uniform float u_Far;
uniform vec2 u_ScreenSize;

out vec4 FragColor;

float LinearizeDepth(float z) {
    float ndc = z * 2.0 - 1.0;
    return (2.0 * u_Near * u_Far) / (u_Far + u_Near - ndc * (u_Far - u_Near));
}

void main() {
    vec4 texColor = (u_HasTexture != 0) ? texture(u_Texture, v_TexCoord) : vec4(1.0);
    vec3 rgb = texColor.rgb * u_Color;
    float alpha = texColor.a * u_Opacity;

    if (u_SoftParticles) {
        vec2 screenUV = gl_FragCoord.xy / u_ScreenSize;
        float sceneDepth = texture(u_DepthTexture, screenUV).r;
        float particleDepth = gl_FragCoord.z;

        float sceneLinear = LinearizeDepth(sceneDepth);
        float particleLinear = LinearizeDepth(particleDepth);
        float fade = clamp((sceneLinear - particleLinear) / max(u_Softness, 0.0001), 0.0, 1.0);
        alpha *= fade;
    }

    FragColor = vec4(rgb, alpha);
}
