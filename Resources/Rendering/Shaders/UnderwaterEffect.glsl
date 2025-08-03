#[compute]
#version 450 core

struct WaterParams {
	vec3 shallow_color;
	float fog_distance;
	vec3 deep_color;
	float fog_fade;
	float transparency_distance;
	float transparency_fade;
};

layout(local_size_x = 8, local_size_y = 8, local_size_z = 1) in;

layout(rgba32f, set = 0, binding = 0) uniform image2D color_image; // Color of the screen pre post-processing
layout(set = 1, binding = 0) uniform sampler2D depth_image; // Depth buffer of the screen
layout(rgba32f, set = 2, binding = 0) uniform restrict readonly image2D water_map_image; // Water Map that was rendered in the Render Shader
layout(set = 3, binding = 0) buffer InputBuffer { // Buffer for WaterMesh-specific shader information
	WaterParams water_params[];
};

layout(push_constant, std430) uniform Params {
	restrict readonly mat4 inv_projection_matrix;
	restrict readonly float near_plane;
	restrict readonly float far_plane;
	restrict readonly ivec2 screen_size; // x: screen width, y: screen height
};


void main()
{
	ivec2 uv = ivec2(gl_GlobalInvocationID.xy);

	// r: underwater mask, underwater if > 0, out of water if 0
	// g: id of the water mesh (unpack as a uint using floatBitsToUint)
	// b: depth of the water (z)
	// a: W value of the water fragment (w)
	vec4 water_map = imageLoad(water_map_image, uv);
	if (water_map.r == 0) return; // Stop if the fragment is not under water


	vec2 screen_uv = vec2(uv) / screen_size;
	vec2 ndc = screen_uv * 2.0 - 1.0;



	highp float water_depth = water_map.z;
	vec4 water = inv_projection_matrix * vec4(ndc, water_depth, 1.0);
	water.xyz /= water.w;
	water_depth = water.z;

	highp float world_depth = textureLod(depth_image, screen_uv, 0.0).r;
	vec4 world = inv_projection_matrix * vec4(ndc, world_depth, 1.0);
	world.xyz /= world.w;
	world_depth = world.z;


	// The amount of water we are looking through is either the end of the water volume (water_depth) or the closest surface (depth)
	highp float water_thickness = -min(-world_depth, -water_depth);

	WaterParams water_parameters = water_params[floatBitsToUint(water_map.g)];


	float fog_blend = smoothstep(water_thickness, water_thickness + water_parameters.fog_distance, near_plane);
	fog_blend = clamp(exp(fog_blend * -water_parameters.fog_fade), 0.0, 1.0);

	float alpha_blend = smoothstep(water_thickness, water_thickness + water_parameters.transparency_distance, near_plane);
	alpha_blend = clamp(exp(alpha_blend * -water_parameters.transparency_fade), 0.0, 1.0);

	vec3 world_color = imageLoad(color_image, uv).rgb;
	world_color = mix(water_parameters.shallow_color, world_color, alpha_blend);
	world_color = mix(water_parameters.deep_color, world_color, fog_blend);

	vec4 final_color = vec4(world_color, 1.0);


	imageStore(color_image, uv, final_color);
}