namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;


[Tool]
[GlobalClass]
public partial class TestCompositorEffect : CompositorEffect {
	private RenderingDevice? renderingDevice;
	private Rid nearestSampler;
	// private Rid linearSampler;


	private Rid shader;
	private Rid shaderPipeline;
	[Export] private RDShaderFile? ShaderFile;



	public TestCompositorEffect() : base() {
		EffectCallbackType = EffectCallbackTypeEnum.PostTransparent;
		RenderingServer.CallOnRenderThread(Callable.From(Construct));
	}

	public override void _Notification(int what) {
		base._Notification(what);
		if (what == NotificationPredelete) {
			Destruct();
		}
	}


	private void Construct() {
		renderingDevice = RenderingServer.GetRenderingDevice();
		if (renderingDevice is null) return;

		nearestSampler = renderingDevice.SamplerCreate(new() {
			MinFilter = RenderingDevice.SamplerFilter.Nearest,
			MagFilter = RenderingDevice.SamplerFilter.Nearest
		});

		// linearSampler = renderingDevice.SamplerCreate(new() {
		// 	MinFilter = RenderingDevice.SamplerFilter.Linear,
		// 	MagFilter = RenderingDevice.SamplerFilter.Linear
		// });

		if (ShaderFile is not null) {
			shader = renderingDevice.ShaderCreateFromSpirV(ShaderFile.GetSpirV());
			shaderPipeline = renderingDevice.ComputePipelineCreate(shader);
		}
	}
	private void Destruct() {
		if (renderingDevice is null) return;

		if (nearestSampler.IsValid) {
			renderingDevice.FreeRid(nearestSampler);
		}

		if (shaderPipeline.IsValid) {
			renderingDevice.FreeRid(shaderPipeline);
		}
		if (shader.IsValid) {
			renderingDevice.FreeRid(shader);
		}
	}

	public override void _RenderCallback(int effectCallbackType, RenderData renderData) {
		base._RenderCallback(effectCallbackType, renderData);
		if (renderingDevice is null || ShaderFile is null) return;

		if (effectCallbackType != (long)EffectCallbackTypeEnum.PostTransparent) return;

		RenderSceneBuffers renderSceneBuffers = renderData.GetRenderSceneBuffers();
		if (renderSceneBuffers is not RenderSceneBuffersRD sceneBuffers) return;

		Vector2I renderSize = sceneBuffers.GetInternalSize();
		if (renderSize.X == 0.0 && renderSize.Y == 0.0) return;

		uint xGroups = (uint)((renderSize.X - 1) / 8) + 1;
		uint yGroups = (uint)((renderSize.Y - 1) / 8) + 1;


		// bool linear = true;

		renderingDevice.DrawCommandBeginLabel("Test Label", new Color(1f, 1f, 1f));

		for (uint view = 0; view < sceneBuffers.GetViewCount(); view++) {
			long computeList = renderingDevice.ComputeListBegin();
			renderingDevice.ComputeListBindComputePipeline(computeList, shaderPipeline);
			renderingDevice.ComputeListBindColor(computeList, sceneBuffers, view, shader, 0);
			renderingDevice.ComputeListBindDepth(computeList, sceneBuffers, view, shader, /* linear ? linearSampler :  */nearestSampler, 1);
			// renderingDevice.ComputeListSetPushConstant(computeList, push_constant.to_byte_array(), push_constant.size() * 4);
			renderingDevice.ComputeListDispatch(computeList, xGroups, yGroups, 1);
			renderingDevice.ComputeListEnd();
		}

		renderingDevice.DrawCommandEndLabel();
	}
}