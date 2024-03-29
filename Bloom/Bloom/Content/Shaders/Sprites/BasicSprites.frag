#version 450

layout (location = 0) in vec2 in_TexCoord;
layout (location = 0) out vec4 out_Color;

layout (binding = 1) uniform sampler2D sampler_Texture;

void main()
{
	// Flip Y axis of coords
	vec2 texCoord = vec2(in_TexCoord.x, 1.0 - in_TexCoord.y);
	// Sample texture
    out_Color = texture(sampler_Texture, texCoord);
}
