#[vertex]
#version 450 core

layout(location = 0) in highp vec3 world_vertex;
layout(set = 0, binding = 0) uniform sampler2D water_displacement_image; // Water Displacement Map

layout(push_constant, std430) uniform Params {
	restrict readonly mat4 world_to_clip; // World-space -> Clip-space Matrix to transform the mesh
	restrict readonly vec2 eye_offset; // Eye offset from Multi-view
	restrict readonly vec2 clipping_planes; // x: near plane, y: far plane
	restrict readonly float water_scale;
	restrict readonly float water_intensity;
};


void main()
{
	vec3 water_displacement = texture(water_displacement_image, world_vertex.xz / water_scale).rgb * water_intensity;
	vec4 clip_pos = world_to_clip * vec4(world_vertex + water_displacement, 1.0);
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