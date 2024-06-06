#[vertex]
#version 450 core

layout(location = 0) in vec3 vertex;

layout(push_constant, std430) uniform Params {
	mat4 world_to_clip; // World-space -> Clip-space Matrix to transform the mesh
	vec3 eye_offset; // Eye offset from Multi-view
} params;


void main()
{
	vec4 pos = params.world_to_clip * vec4(vertex, 1.0);
	gl_Position = vec4(vec3(pos.x, -pos.y, pos.z) + params.eye_offset, pos.w);
}


#[fragment]
#version 450 core

layout(location = 0) out vec4 frag_color;


void main() {
	frag_color = vec4(gl_FragCoord.xyw, 1 - float(gl_FrontFacing));
}