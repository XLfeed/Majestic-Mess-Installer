#version 330 core

in vec2 v_TexCoord;
out vec4 FragColor;

uniform sampler2D u_TexY;   // slot 0 — full resolution luma
uniform sampler2D u_TexU;   // slot 1 — half resolution Cb
uniform sampler2D u_TexV;   // slot 2 — half resolution Cr

void main() {
    float y = texture(u_TexY, v_TexCoord).r;
    float u = texture(u_TexU, v_TexCoord).r;
    float v = texture(u_TexV, v_TexCoord).r;

    // BT.601 limited-range (studio swing) YCbCr -> RGB
    // Coefficients match the pl_mpeg reference matrix:
    //   gl_FragColor = vec4(y, cb, cr, 1.0) * bt601
    FragColor = vec4(
        clamp(1.16438 * y                   + 1.59603 * v - 0.87079, 0.0, 1.0),
        clamp(1.16438 * y - 0.39176 * u     - 0.81297 * v + 0.52959, 0.0, 1.0),
        clamp(1.16438 * y + 2.01723 * u                   - 1.08139, 0.0, 1.0),
        1.0);
}
