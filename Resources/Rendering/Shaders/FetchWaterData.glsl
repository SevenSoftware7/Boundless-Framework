#[compute]
#version 450

// Number of work groups
layout(local_size_x = 1) in;

struct WaterInput {
	highp vec2 location;
	highp float intensity;
	highp float water_scale;
};

layout(set = 0, binding = 0) uniform sampler2D displacement_image;

// Input buffer containing Subscriber XYZ coordinates
layout(set = 1, binding = 0) readonly buffer InputBuffer {
	WaterInput inputs[];
};

// Output buffer containing the requested XYZ Water displacement
layout(set = 2, binding = 0) writeonly buffer OutputBuffer {
	highp float outputs[];
};



void main() {
	uint index = gl_GlobalInvocationID.x;
	uint outputIndex = index * 3;

	// Read UV coordinates from the buffer
	WaterInput waterInput = inputs[index];

	// Sample the texture at the given UV
	highp vec3 displacement = (texture(displacement_image, waterInput.location / waterInput.water_scale).xyz * 2.0 - 1.0) * waterInput.intensity;

	// Write the displacement to the buffer
	outputs[outputIndex] = displacement.x;
	outputs[outputIndex + 1] = displacement.y;
	outputs[outputIndex + 2] = displacement.z;
}
