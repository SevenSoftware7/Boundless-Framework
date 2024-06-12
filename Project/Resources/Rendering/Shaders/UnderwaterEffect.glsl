#[compute]
#version 450

layout(local_size_x = 8, local_size_y = 8, local_size_z = 1) in;

layout(rgba8, set = 0, binding = 0) uniform image2D color_image; // Color of the screen pre post-processing
layout(set = 1, binding = 0) uniform sampler2D depth_image; // Depth buffer of the screen
layout(rgba16f, set = 2, binding = 0) uniform restrict readonly image2D water_map_image; // Water Map that was rendered in the Render Shader

layout(push_constant, std430) uniform Params {
	restrict readonly vec2 screen_size; // x: screen width, y: screen height
	restrict readonly vec2 clipping_planes; // z: near plane, w: far plane
	restrict readonly vec3 water_color; // Color of the Water
};


float delinearize_depth(float depth, float zNear, float zFar)
{
	depth = 2.0 * depth - 1.0;
	float zLinear = 2.0 * zNear * zFar / (zFar + zNear - depth * (zFar - zNear));
	return zLinear;
}
float linearize_depth(float depth, float zNear, float zFar)
{
	float nonLinearDepth = (zFar + zNear - 2.0 * zNear * zFar / depth) / (zFar - zNear);
	nonLinearDepth = (nonLinearDepth + 1.0) / 2.0;
	return nonLinearDepth;
}

void main()
{
	ivec2 uv = ivec2(gl_GlobalInvocationID.xy);

	vec4 water_map = imageLoad(water_map_image, uv);
	if (water_map.r == 0) return;
	float water_depth = water_map.z;

	vec2 depth_uv = vec2(uv) / screen_size;
	float depth = texture(depth_image, depth_uv).r;

	// The amount of water we are looking through is either the end of the water volume (water_depth) or the closest surface (depth)
	float max_depth = max(depth, water_depth);
	max_depth = clamp(linearize_depth(max_depth, clipping_planes.x, clipping_planes.y), 0, 1);

	// Actual water color calculation
	vec3 color = imageLoad(color_image, uv).rgb;
	vec3 water_color = mix(color, water_color, (1 - max_depth) * 0.7);

	imageStore(color_image, uv, vec4(mix(color, water_color, water_map.r), 1));
}