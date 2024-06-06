#[compute]
#version 450

layout(local_size_x = 8, local_size_y = 8, local_size_z = 1) in;

layout(rgba8, set = 0, binding = 0) uniform image2D color_image; // Color of the screen pre post-processing
layout(set = 1, binding = 0) uniform sampler2D depth_image; // Depth buffer of the screen
layout(r16f, set = 2, binding = 0) uniform image2D water_map_image; // Water Map that was rendered in the Render Shader

layout(push_constant, std430) uniform Params {
	vec2 render_size; // Size of the Screen
} params;


void main() {
	ivec2 uv = ivec2(gl_GlobalInvocationID.xy);
	vec2 depth_uv = vec2(uv) / params.render_size;

	vec4 color = imageLoad(color_image, uv);
	vec4 water_map = imageLoad(water_map_image, uv);
	float depth = texture(depth_image, depth_uv).r;

	// The amount of water we are looking through is either the end of the water volume (water_map.z) or the closest surface (depth)
	float max_depth = max(water_map.z, depth);

	// Actual water color calculation
	vec4 water_color = mix(color, vec4(0, 0.1, 0.75, 1), 1 - max_depth);

	imageStore(color_image, uv, mix(color, water_color, water_map.a));
}