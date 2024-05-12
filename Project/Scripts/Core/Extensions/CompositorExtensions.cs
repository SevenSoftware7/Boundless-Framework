namespace LandlessSkies.Core;

using Godot;

public static class CompositorExtensions {

	public static RDUniform GetImageUniform(Rid image, int binding = 0) {
		RDUniform uniform = new() {
			UniformType = RenderingDevice.UniformType.Image,
			Binding = binding
		};

		uniform.AddId(image);

		return uniform;
	}

	public static RDUniform GetSamplerUniform(Rid image, Rid sampler, int binding = 0) {
		RDUniform uniform = new() {
			UniformType = RenderingDevice.UniformType.SamplerWithTexture,
			Binding = binding
		};

		uniform.AddId(sampler);
		uniform.AddId(image);

		return uniform;
	}

	public static void ComputeListBindColor(this RenderingDevice device, long computeList, RenderSceneBuffersRD sceneBuffers, uint view, Rid shaderRid, uint setIndex) {
		device.ComputeListBindUniformRid(computeList, GetImageUniform(sceneBuffers.GetColorLayer(view)), shaderRid, setIndex);
	}
	public static void ComputeListBindDepth(this RenderingDevice device, long computeList, RenderSceneBuffersRD sceneBuffers, uint view, Rid shaderRid, Rid sampler, uint setIndex) {
		device.ComputeListBindUniformRid(computeList, GetSamplerUniform(sceneBuffers.GetDepthLayer(view), sampler), shaderRid, setIndex);
	}

	public static void ComputeListBindUniformRid(this RenderingDevice device, long computeList, RDUniform uniform, Rid shaderRid, uint setIndex) {
		Rid set = UniformSetCacheRD.GetCache(shaderRid, setIndex, [uniform]);

		device.ComputeListBindUniformSet(computeList, set, setIndex);
	}
}