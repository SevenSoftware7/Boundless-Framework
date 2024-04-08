using Godot;

namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class TestCompositorEffect : CompositorEffect {
	private Rid nearestSampler;
	// private Rid linearSampler;


	[Export] private RDShaderFile? ShaderFile {
		get => _shaderFile;
		set {
			_shaderFile = value;
			if (CompositorExtensions.RenderingDevice is not RenderingDevice renderingDevice)
				return;


			if (_shaderFile is null) {
				FreeShader(renderingDevice);
				return;
			}

			shader = renderingDevice.ShaderCreateFromSpirV(_shaderFile.GetSpirV());
			shaderPipeline = renderingDevice.ComputePipelineCreate(shader);
		}
	}


	private RDShaderFile? _shaderFile;

	private Rid shader;
	private Rid shaderPipeline;



	public TestCompositorEffect() : base() {
		EffectCallbackType = EffectCallbackTypeEnum.PostTransparent;
		RenderingServer.CallOnRenderThread(Callable.From(Construct));
	}


	private void Construct() {
		if (CompositorExtensions.RenderingDevice is not RenderingDevice renderingDevice)
			return;

		nearestSampler = renderingDevice.SamplerCreate(new() {
			MinFilter = RenderingDevice.SamplerFilter.Nearest,
			MagFilter = RenderingDevice.SamplerFilter.Nearest
		});
		// linearSampler = renderingDevice.SamplerCreate(new() {
		// 	MinFilter = RenderingDevice.SamplerFilter.Linear,
		// 	MagFilter = RenderingDevice.SamplerFilter.Linear
		// });
	}
	private void Destruct() {
		if (CompositorExtensions.RenderingDevice is not RenderingDevice renderingDevice)
			return;

		FreeShader(renderingDevice);

		renderingDevice.FreeRid(nearestSampler);
		// renderingDevice.FreeRid(linearSampler);
	}

	private void FreeShader(RenderingDevice renderingDevice) {
		if (shaderPipeline.IsValid) {
			renderingDevice.FreeRid(shaderPipeline);
			shaderPipeline = default;
		}
		if (shader.IsValid) {
			renderingDevice.FreeRid(shader);
			shader = default;
		}
	}

	public override void _RenderCallback(int effectCallbackType, RenderData renderData) {
		base._RenderCallback(effectCallbackType, renderData);
		if (CompositorExtensions.RenderingDevice is not RenderingDevice renderingDevice || _shaderFile is null)
			return;

		if (effectCallbackType != (long)EffectCallbackTypeEnum.PostTransparent)
			return;

		RenderSceneBuffers renderSceneBuffers = renderData.GetRenderSceneBuffers();
		if (renderSceneBuffers is not RenderSceneBuffersRD sceneBuffers)
			return;

		Vector2I renderSize = sceneBuffers.GetInternalSize();
		if (renderSize.X == 0.0 && renderSize.Y == 0.0)
			return;

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

	public override void _Notification(int what) {
		base._Notification(what);
		if (what == NotificationPredelete) {
			Destruct();
		}
	}
}