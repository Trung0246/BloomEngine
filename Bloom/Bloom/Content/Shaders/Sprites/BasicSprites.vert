#version 450

const int MAX_LISTS = 32;

layout (location = 0) in mat4 in_InstanceTransform;
layout (location = 4) in vec4 in_InstanceRectangle;
layout (location = 0) out vec2 out_TexCoord;

layout (std140, binding = 0) uniform struct_Camera
{
	mat4 view;
	mat4 projection;
} block_Camera;

layout (std140, binding = 2) uniform struct_Time
{
	float listTime[MAX_LISTS];
} time;

out gl_PerVertex
{
    vec4 gl_Position;
};

void main()
{
    vec2 pos[4] = vec2[4](vec2(-0.5, 0.5), vec2(0.5, 0.5), vec2(0.5, -0.5), vec2(-0.5, -0.5));
    vec2 texCoord[4] = vec2[4](vec2(0, 0), vec2(1, 0), vec2(1, 1), vec2(0, 1));
    int index[6] = int[6](0, 1, 2, 2, 3, 0);
    vec4 vPos = in_InstanceTransform * vec4(pos[index[gl_VertexIndex]], 0.0, 1.0);

    out_TexCoord = in_InstanceRectangle.xy + in_InstanceRectangle.zw * texCoord[index[gl_VertexIndex]];
    gl_Position = block_Camera.projection * block_Camera.view * vPos;
}
