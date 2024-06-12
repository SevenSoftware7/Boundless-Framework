#[vertex]
#version 450 core

layout(location = 0) in highp vec3 vertex;

layout(push_constant, std430) uniform Params {
	mat4 world_to_clip; // World-space -> Clip-space Matrix to transform the mesh
	vec2 eye_offset; // Eye offset from Multi-view
	vec2 clipping_planes; // x: near plane, y: far plane
};



void main()
{
	vec4 clip_pos = world_to_clip * vec4(vertex, 1.0);
	clip_pos.y = -clip_pos.y; // Godot's Y coordinate is flipped in uv-space
	clip_pos.xy += eye_offset;
	clip_pos.z += clipping_planes.x; // Adjust for near clipping plane (might be a godot bug)

	gl_Position = clip_pos;
}


#[fragment]
#version 450 core

layout(location = 0) out vec4 frag_color;


void main()
{
	frag_color = vec4(1 - float(gl_FrontFacing), 0, 1 - gl_FragCoord.z, gl_FragCoord.w);
}