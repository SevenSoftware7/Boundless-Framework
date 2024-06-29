#[compute]
#version 450

layout(local_size_x = 8, local_size_y = 8, local_size_z = 1) in;

layout(rgba32f, set = 0, binding = 0) uniform image2D color_image; // Color to output the computed displacement to

layout(push_constant, std430) uniform Params {
	restrict readonly ivec2 screen_size; // x: screen width, y: screen height
};

void main()
{
	ivec2 uv = ivec2(gl_GlobalInvocationID.xy);
	vec2 scaled_uv = vec2(uv) / screen_size;

	imageStore(color_image, uv, vec4(scaled_uv, 0, 0));
}