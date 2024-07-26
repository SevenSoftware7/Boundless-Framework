#[compute]
#version 450

// Number of work groups
layout(local_size_x = 1) in;

// Input buffer containing vec2 UV coordinates
layout(set = 0, binding = 0) buffer InputBuffer {
	vec4 locations[];
};

// The texture to sample
layout(set = 1, binding = 0) uniform sampler2D displacement_image;



layout(push_constant, std430) uniform Params {
	restrict readonly highp float water_scale;
	restrict readonly highp float water_intensity;
};



void main() {
	uint index = gl_GlobalInvocationID.x;

	// Read UV coordinates from the buffer
	highp vec3 location = locations[index].xyz;

	// Sample the texture at the given UV
	highp vec3 displacement = (texture(displacement_image, location.xz / water_scale).xyz * 2.0 - 1.0) * water_intensity;

	// Write the displacement to the buffer
	locations[index] = vec4(displacement, 0);
}
