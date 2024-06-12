namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;

[Tool]
[GlobalClass]
public partial class BlitCompositorEffect : BaseCompositorEffect {
	private RenderingDevice? renderingDevice;
	private Rid nearestSampler;


	[Export] private RDShaderFile? ShaderFile {
		get => _shaderFile;
		set {
			_shaderFile = value;

			if (renderingDevice is not null) {
				Destruct();
				Construct();
			}
		}
	}
	private RDShaderFile? _shaderFile;
	private Rid shader;

	private Rid computePipeline;



	public BlitCompositorEffect() : base() {
		EffectCallbackType = EffectCallbackTypeEnum.PostTransparent;
	}


	protected override void ConstructBehaviour(RenderingDevice renderingDevice) {
		if (ShaderFile is null) return;
		shader = renderingDevice.ShaderCreateFromSpirV(ShaderFile.GetSpirV());

		nearestSampler = renderingDevice.SamplerCreate(new() {
			MinFilter = RenderingDevice.SamplerFilter.Nearest,
			MagFilter = RenderingDevice.SamplerFilter.Nearest
		});

		computePipeline = renderingDevice.ComputePipelineCreate(shader);
	}
	protected override void DestructBehaviour(RenderingDevice renderingDevice) {
		if (nearestSampler.IsValid) {
			renderingDevice.FreeRid(nearestSampler);
		}

		if (computePipeline.IsValid) {
			renderingDevice.FreeRid(computePipeline);
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


		renderingDevice.DrawCommandBeginLabel("Test Label", new Color(1f, 1f, 1f));

		for (uint view = 0; view < sceneBuffers.GetViewCount(); view++) {
			long computeList = renderingDevice.ComputeListBegin();
			renderingDevice.ComputeListBindComputePipeline(computeList, computePipeline);
			renderingDevice.ComputeListBindColor(computeList, shader, sceneBuffers, view, 0);
			renderingDevice.ComputeListBindDepth(computeList, shader, sceneBuffers, view, nearestSampler, 1);
			renderingDevice.ComputeListDispatch(computeList, xGroups, yGroups, 1);
			renderingDevice.ComputeListEnd();
		}

		renderingDevice.DrawCommandEndLabel();
	}
}