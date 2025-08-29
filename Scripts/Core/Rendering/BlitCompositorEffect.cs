namespace Seven.Boundless;

using Godot;
using Seven.Boundless.Utility;

[Tool]
[GlobalClass]
public partial class BlitCompositorEffect : BaseCompositorEffect {
	private Rid depthSampler;


	[Export]
	private RDShaderFile? ShaderFile {
		get;
		set {
			DestructComputePipeline();
			field = value;
			ConstructComputePipeline();
		}
	}
	private Rid shader;

	private Rid computePipeline;



	public BlitCompositorEffect() : base() {
		EffectCallbackType = EffectCallbackTypeEnum.PostTransparent;
	}


	protected virtual void ComputeListBind(in long computeList, in Rid shader, in RenderSceneBuffersRD sceneBuffers, in uint view) { }

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
			ComputeListBind(computeList, shader, sceneBuffers, view);
			RenderingDevice.ComputeListDispatch(computeList, xGroups, yGroups, 1);
			RenderingDevice.ComputeListEnd();
		}

		RenderingDevice.DrawCommandEndLabel();
	}

	protected virtual void BlitConstructBehaviour() { }
	protected virtual void BlitDestructBehaviour() { }

	protected sealed override void ConstructBehaviour() {
		if (ShaderFile is null) return;

		depthSampler = RenderingDevice.SamplerCreate(new() {
			MinFilter = RenderingDevice.SamplerFilter.Nearest,
			MagFilter = RenderingDevice.SamplerFilter.Nearest
		});

		ConstructComputePipeline();

		BlitConstructBehaviour();
	}
	protected sealed override void DestructBehaviour() {
		if (depthSampler.IsValid) {
			RenderingDevice.FreeRid(depthSampler);
		}

		DestructComputePipeline();

		BlitDestructBehaviour();
	}


	private void ConstructComputePipeline() {
		if (ShaderFile is null) return;

		shader = RenderingDevice.ShaderCreateFromSpirV(ShaderFile.GetSpirV());
		computePipeline = RenderingDevice.ComputePipelineCreate(shader);
	}

	private void DestructComputePipeline() {
		if (computePipeline.IsValid) {
			RenderingDevice.FreeRid(computePipeline);
		}
		if (shader.IsValid) {
			RenderingDevice.FreeRid(shader);
		}
	}
}