#[compute]
#version 450

layout(local_size_x = 8, local_size_y = 8, local_size_z = 1) in;

layout(rgba32f, set = 0, binding = 0) uniform image2D color_image; // Color to output the computed displacement to

layout(push_constant, std430) uniform Params {
	restrict readonly ivec2 size; // x: texture width, y: texture height
	restrict readonly float time;
};


// TODO: Replace the Simplex noise with FFTs

// Permutation table
vec3 mod289(vec3 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
vec4 mod289(vec4 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
vec4 permute(vec4 x) { return mod289(((x*34.0)+1.0)*x); }

// Gradient table
vec4 taylorInvSqrt(vec4 r) { return 1.79284291400159 - 0.85373472095314 * r; }
vec3 fade(vec3 t) { return t * t * t * (t * (t * 6.0 - 15.0) + 10.0); }

// Simplex noise function
float noise(vec3 v)
{
	const vec2  C = vec2(1.0 / 6.0, 1.0 / 3.0) ;
	const vec4  D = vec4(0.0, 0.5, 1.0, 2.0);

	// First corner
	vec3 i  = floor(v + dot(v, C.yyy) );
	vec3 x0 =   v - i + dot(i, C.xxx) ;

	// Other corners
	vec3 g = step(x0.yzx, x0.xyz);
	vec3 l = 1.0 - g;
	vec3 i1 = min( g.xyz, l.zxy );
	vec3 i2 = max( g.xyz, l.zxy );

	// Permutations
	vec3 x1 = x0 - i1 + C.xxx;
	vec3 x2 = x0 - i2 + C.yyy;
	vec3 x3 = x0 - D.yyy;

	// Gradients
	i = mod289(i);
	vec4 p = permute( permute( permute(
				i.z + vec4(0.0, i1.z, i2.z, 1.0 ))
			+ i.y + vec4(0.0, i1.y, i2.y, 1.0 ))
			+ i.x + vec4(0.0, i1.x, i2.x, 1.0 ));

	// Blend gradients
	vec4 j = p - 49.0 * floor(p * (1.0 / 49.0)) ;  // Modulo 7
	vec4 x_ = floor(j * (1.0 / 7.0));
	vec4 y_ = floor(j - 7.0 * x_ );    // Modulo 7

	vec4 x = x_ * (1.0 / 7.0);
	vec4 y = y_ * (1.0 / 7.0);
	vec4 h = 1.0 - abs(x) - abs(y);

	vec4 b0 = vec4( x.xy, y.xy );
	vec4 b1 = vec4( x.zw, y.zw );

	vec4 s0 = floor(b0) * 2.0 + 1.0;
	vec4 s1 = floor(b1) * 2.0 + 1.0;
	vec4 sh = -step(h, vec4(0.0));

	vec4 a0 = b0.xzyw + s0.xzyw * sh.xxyy ;
	vec4 a1 = b1.xzyw + s1.xzyw * sh.zzww ;

	vec3 p0 = vec3(a0.xy,h.x);
	vec3 p1 = vec3(a0.zw,h.y);
	vec3 p2 = vec3(a1.xy,h.z);
	vec3 p3 = vec3(a1.zw,h.w);

	// Normalize gradients
	vec4 norm = taylorInvSqrt(vec4(dot(p0,p0), dot(p1,p1), dot(p2,p2), dot(p3,p3)));
	p0 *= norm.x;
	p1 *= norm.y;
	p2 *= norm.z;
	p3 *= norm.w;

	// Mix final noise value
	vec4 m = max(0.6 - vec4(dot(x0,x0), dot(x1,x1), dot(x2,x2), dot(x3,x3)), 0.0);
	m = m * m;
	return 42.0 * dot( m*m, vec4( dot(p0,x0), dot(p1,x1), dot(p2,x2), dot(p3,x3) ) );
}

// Noise function that returns a 3D vector based on UV coordinates
vec3 noise3D(vec2 uv, float time)
{
	float n1 = noise(vec3(uv, time));
	float n2 = noise(vec3(uv, time + 1.0));
	float n3 = noise(vec3(uv, time + 2.0));
	return vec3(n1, n2, n3);
}


vec3 generate_displacement(vec2 uv)
{
	return noise3D(uv, time * 0.2);
}

void main()
{
	ivec2 uv = ivec2(gl_GlobalInvocationID.xy);
	vec2 scaled_uv = vec2(uv) / size;

	vec3 displacement = generate_displacement(scaled_uv);
	displacement = clamp(displacement * 0.5 + 0.5, 0, 1); // keep values in range [-1, 1] and store as [0, 1]

	imageStore(color_image, uv, vec4(displacement, 0));
}