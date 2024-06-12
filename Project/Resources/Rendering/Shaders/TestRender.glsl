#[vertex]
#version 450 core

layout(location = 0) in highp vec3 vertex;

layout(push_constant, std430) uniform Params {
	mat4 world_to_clip; // World-space -> Clip-space Matrix to transform the mesh
	vec3 eye_offset; // Eye offset from Multi-view
	vec2 clipping_planes;
} params;



void main()
{
	vec4 clip_pos = params.world_to_clip * vec4(vertex, 1.0);

	gl_Position = vec4(clip_pos.x, -clip_pos.y, clip_pos.z + params.clipping_planes.x, clip_pos.w);
}


#[fragment]
#version 450 core

layout(location = 0) out vec4 frag_color;


void main()
{
	frag_color = vec4(1 - float(gl_FrontFacing), 0, 1 - gl_FragCoord.z, gl_FragCoord.w);
}