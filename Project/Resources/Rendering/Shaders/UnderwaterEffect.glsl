#[compute]
#version 450 core

struct WaterParams {
	vec3 shallow_color;
	vec3 deep_color;
	float thickness;
};

layout(local_size_x = 8, local_size_y = 8, local_size_z = 1) in;

layout(rgba8, set = 0, binding = 0) uniform image2D color_image; // Color of the screen pre post-processing
layout(set = 1, binding = 0) uniform sampler2D depth_image; // Depth buffer of the screen
layout(rgba32f, set = 2, binding = 0) uniform restrict readonly image2D water_map_image; // Water Map that was rendered in the Render Shader
layout(set = 3, binding = 0) buffer InputBuffer { // Buffer for WaterMesh-specific shader information
	WaterParams water_params[];
};

layout(push_constant, std430) uniform Params {
	restrict readonly ivec2 screen_size; // x: screen width, y: screen height
	restrict readonly vec2 clipping_planes; // x: near plane, y: far plane
	restrict readonly float fog_start;
	restrict readonly float fog_end;
};


void main()
{
	ivec2 uv = ivec2(gl_GlobalInvocationID.xy);

	// r: underwater mask, underwater if > 0, out of water if 0
	// g: id of the water mesh (unpack as a uint using floatBitsToUint)
	// b: depth of the water (z)
	// a: W value of the water fragment (w)
	vec4 water_map = imageLoad(water_map_image, uv);

	float water_mask = water_map.r;
	if (water_mask == 0) return; // Stop if there is no water to render

	WaterParams water_parameters = water_params[floatBitsToUint(water_map.g)];


	float water_depth = water_map.z;
	vec2 depth_uv = vec2(uv) / screen_size;
	float depth = texture(depth_image, depth_uv).r;

	// The amount of water we are looking through is either the end of the water volume (water_depth) or the closest surface (depth)
	float max_depth = max(depth, water_depth);

	float depth_factor = clamp(max_depth * fog_end / water_parameters.thickness, 0, 1);
	vec3 water_color = mix(water_parameters.deep_color, water_parameters.shallow_color, depth_factor);


	// Actual water color calculation
	float transparency_factor = clamp(max_depth * fog_start / water_parameters.thickness, 0, 1) * depth_factor;

	vec3 color = imageLoad(color_image, uv).rgb;
	vec3 final_color = mix(water_color, color, transparency_factor);

	imageStore(color_image, uv, vec4(final_color, 1));
}