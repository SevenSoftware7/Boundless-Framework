#[vertex]
#version 450 core

layout(location = 0) in highp vec3 world_vertex;
layout(set = 0, binding = 0) uniform sampler2D water_displacement_image; // Water Displacement Map

layout(push_constant, std430) uniform Params {
	restrict readonly highp mat4 world_to_clip; // World-space -> Clip-space Matrix to transform the mesh
	restrict readonly highp vec2 eye_offset; // Eye offset from Multi-view
	restrict readonly float water_scale;
	restrict readonly float water_intensity;
};


void main()
{
	highp vec3 water_displacement = (texture(water_displacement_image, world_vertex.xz / water_scale).xyz * 2.0 - 1.0) * water_intensity;
	highp vec4 clip_pos = world_to_clip * vec4(world_vertex + water_displacement, 1.0);
	clip_pos.xy += eye_offset;

	gl_Position = clip_pos;
}



#[fragment]
#version 450 core

layout(location = 0) out highp vec4 frag_color;


void main()
{
	frag_color = vec4(1 - float(gl_FrontFacing), 0, gl_FragCoord.z, gl_FragCoord.w);
}