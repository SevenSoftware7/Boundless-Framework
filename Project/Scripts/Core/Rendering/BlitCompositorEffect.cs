namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;

[Tool]
[GlobalClass]
public partial class BlitCompositorEffect : BaseCompositorEffect {
	private Rid depthSampler;


	[Export]
	private RDShaderFile? ShaderFile {
		get => _shaderFile;
		set {
			_shaderFile = value;

			if (RenderingDevice is not null) {
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


	protected virtual void BlitConstructBehaviour(in RenderingDevice renderingDevice) { }
	protected virtual void BlitDestructBehaviour(in RenderingDevice renderingDevice) { }
	protected virtual void ComputeListBind(in RenderingDevice renderingDevice, in long computeList, in Rid shader, in RenderSceneBuffersRD sceneBuffers, in uint view) { }


	protected sealed override void ConstructBehaviour(RenderingDevice renderingDevice) {
		if (ShaderFile is null) return;
		shader = renderingDevice.ShaderCreateFromSpirV(ShaderFile.GetSpirV());

		depthSampler = renderingDevice.SamplerCreate(new() {
			MinFilter = RenderingDevice.SamplerFilter.Nearest,
			MagFilter = RenderingDevice.SamplerFilter.Nearest
		});

		computePipeline = renderingDevice.ComputePipelineCreate(shader);

		BlitConstructBehaviour(renderingDevice);
	}
	protected sealed override void DestructBehaviour(RenderingDevice renderingDevice) {
		if (depthSampler.IsValid) {
			renderingDevice.FreeRid(depthSampler);
		}

		if (computePipeline.IsValid) {
			renderingDevice.FreeRid(computePipeline);
		}
		if (shader.IsValid) {
			renderingDevice.FreeRid(shader);
		}

		BlitDestructBehaviour(renderingDevice);
	}

	public override void _RenderCallback(int effectCallbackType, RenderData renderData) {
		base._RenderCallback(effectCallbackType, renderData);
		if (RenderingDevice is null || ShaderFile is null) return;

		if (effectCallbackType != (long)EffectCallbackTypeEnum.PostTransparent) return;

		RenderSceneBuffers renderSceneBuffers = renderData.GetRenderSceneBuffers();
		if (renderSceneBuffers is not RenderSceneBuffersRD sceneBuffers) return;

		Vector2I renderSize = sceneBuffers.GetInternalSize();
		if (renderSize.X == 0.0 && renderSize.Y == 0.0) return;

		uint xGroups = (uint)((renderSize.X - 1) / 8) + 1;
		uint yGroups = (uint)((renderSize.Y - 1) / 8) + 1;


		RenderingDevice.DrawCommandBeginLabel("Test Label", new Color(1f, 1f, 1f));

		for (uint view = 0; view < sceneBuffers.GetViewCount(); view++) {
			long computeList = RenderingDevice.ComputeListBegin();
			RenderingDevice.ComputeListBindComputePipeline(computeList, computePipeline);
			RenderingDevice.ComputeListBindColor(computeList, shader, sceneBuffers, view, 0, 0);
			RenderingDevice.ComputeListBindDepth(computeList, shader, sceneBuffers, view, depthSampler, 0, 1);
			ComputeListBind(RenderingDevice, computeList, shader, sceneBuffers, view);
			RenderingDevice.ComputeListDispatch(computeList, xGroups, yGroups, 1);
			RenderingDevice.ComputeListEnd();
		}

		RenderingDevice.DrawCommandEndLabel();
	}
}