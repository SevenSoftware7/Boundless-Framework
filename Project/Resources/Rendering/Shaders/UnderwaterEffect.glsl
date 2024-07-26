#[compute]
#version 450

layout(local_size_x = 8, local_size_y = 8, local_size_z = 1) in;

layout(rgba8, set = 0, binding = 0) uniform image2D color_image; // Color of the screen pre post-processing
layout(set = 1, binding = 0) uniform sampler2D depth_image; // Depth buffer of the screen
layout(rgba32f, set = 2, binding = 0) uniform restrict readonly image2D water_map_image; // Water Map that was rendered in the Render Shader

layout(push_constant, std430) uniform Params {
	restrict readonly vec2 screen_size; // x: screen width, y: screen height
	restrict readonly vec2 clipping_planes; // x: near plane, y: far plane
	restrict readonly vec3 water_color; // Color of the Water
};


float linearize_depth(float depth, float zNear, float zFar)
{
	float z_ndc = depth * 2.0 - 1.0; // Convert [0, 1] depth to [-1, 1] NDC space
	float linear = (2.0 * zNear * zFar) / (zFar + zNear - z_ndc * (zFar - zNear));
	return (linear - zNear) / (zFar - zNear); // Normalize to [0, 1] range
}


vec3 apply_underwater_fog(vec3 color, vec3 fog_color, float depth, float fog_density, float fog_distance)
{
	float fog_factor = clamp(depth * fog_distance / fog_density, 0, 1);
	return mix(fog_color, color, fog_factor);
}

void main()
{
	ivec2 uv = ivec2(gl_GlobalInvocationID.xy);

	// r: underwater mask, underwater if > 0, out of water if 0
	// g: unused (feel free to use it for something)
	// b: depth of the water (z)
	// a: W value of the water fragment (w)
	vec4 water_map = imageLoad(water_map_image, uv);

	if (water_map.r == 0) return; // Stop if there is no water to render
	float water_depth = water_map.z;

	vec2 depth_uv = vec2(uv) / screen_size;
	float depth = texture(depth_image, depth_uv).r;

	// The amount of water we are looking through is either the end of the water volume (water_depth) or the closest surface (depth)
	float max_depth = max(depth, water_depth);
	// max_depth = clamp(linearize_depth(max_depth, clipping_planes.x, clipping_planes.y), 0, 1);

	// Actual water color calculation
	vec3 color = imageLoad(color_image, uv).rgb;
	vec3 water_color = apply_underwater_fog(color, water_color, max_depth, 0.3, 7.0);

	imageStore(color_image, uv, vec4(mix(color, water_color, water_map.r), 1));
}