namespace LandlessSkies.Core;

using System;
using Godot;
using SevenDev.Utility;

[Tool]
[GlobalClass]
public partial class WaterDisplacementEffect : BaseCompositorEffect {
	public static readonly StringName Context = "WaterDisplacementEffect";
	public static readonly StringName DisplacementMapName = "water_displacement";
	[Export] private Texture2Drd? Texture {
		get => _texture;
		set {
			if (RenderingDevice is not null) Destruct();

			_texture = value;

			if (RenderingDevice is not null) Construct();
		}
	}
	private Texture2Drd? _texture;


	[Export] private RDShaderFile? ComputeShaderFile {
		get => _computeShaderFile;
		set {
			if (RenderingDevice is not null) Destruct();

			_computeShaderFile = value;

			if (RenderingDevice is not null) Construct();
		}
	}
	private RDShaderFile? _computeShaderFile;
	private Rid computeShader;

	private readonly RDTextureFormat waterDisplacementMapAttachmentFormat = new() {
		Width = 128,
		Height = 128,
		TextureType = RenderingDevice.TextureType.Type2D,
		Format = RenderingDevice.DataFormat.R8G8B8A8Unorm,
		Samples = RenderingDevice.TextureSamples.Samples1,
		UsageBits = RenderingDevice.TextureUsageBits.StorageBit | RenderingDevice.TextureUsageBits.SamplingBit | RenderingDevice.TextureUsageBits.CanCopyFromBit
	};
	private Rid waterDisplacementMap;

	private Rid computePipeline;



	public WaterDisplacementEffect() : base() {
		EffectCallbackType = EffectCallbackTypeEnum.PostOpaque;
	}



	public override void _RenderCallback(int effectCallbackType, RenderData renderData) {
		base._RenderCallback(effectCallbackType, renderData);

		if (RenderingDevice is null || _computeShaderFile is null || Texture is null) return;


		Vector2I renderSize = new(128, 128);
		if (renderSize.X == 0.0 && renderSize.Y == 0.0) {
			throw new ArgumentException("Render size is incorrect");
		}

		uint xGroups = (uint)((renderSize.X - 1) / 8) + 1;
		uint yGroups = (uint)((renderSize.Y - 1) / 8) + 1;


		// Unfolding into a push constant
		int[] computePushConstantInts = [
			renderSize.X, renderSize.Y, 0, 0,
			// nearClippingPlane, farClippingPlane,

			// waterColor.R, waterColor.G, waterColor.B, 0,
		];
		int intsByteCount = computePushConstantInts.Length * sizeof(int);

		// float[] computePushConstantFloats = [

		// ];
		// int floatsByteCount = computePushConstantFloats.Length * sizeof(float);

		byte[] computePushConstantBytes = new byte[intsByteCount/*  + floatsByteCount */];
		Buffer.BlockCopy(computePushConstantInts, 0, computePushConstantBytes, 0, intsByteCount);
		// Buffer.BlockCopy(computePushConstantFloats, 0, computePushConstantBytes, intsByteCount, floatsByteCount);

		// Here we draw the Underwater effect, using the waterBuffer to know where there is water geometry
		RenderingDevice.DrawCommandBeginLabel("Render Water Displacement", new Color(1f, 1f, 1f));
		long computeList = RenderingDevice.ComputeListBegin();
		RenderingDevice.ComputeListBindComputePipeline(computeList, computePipeline);
		RenderingDevice.ComputeListBindImage(computeList, computeShader, waterDisplacementMap, 0);
		RenderingDevice.ComputeListSetPushConstant(computeList, computePushConstantBytes, (uint)computePushConstantBytes.Length);
		RenderingDevice.ComputeListDispatch(computeList, xGroups, yGroups, 1);
		RenderingDevice.ComputeListEnd();
		RenderingDevice.DrawCommandEndLabel();
	}




	protected override void ConstructBehaviour(RenderingDevice renderingDevice) {
		if (Texture is null || ComputeShaderFile is null) return;

		computeShader = renderingDevice.ShaderCreateFromSpirV(ComputeShaderFile.GetSpirV());
		if (! computeShader.IsValid) {
			throw new ArgumentException("Compute Shader is Invalid");
		}

		computePipeline = renderingDevice.ComputePipelineCreate(computeShader);
		if (! computePipeline.IsValid) {
			throw new ArgumentException("Compute Pipeline is Invalid");
		}


		waterDisplacementMap = renderingDevice.TextureCreate(waterDisplacementMapAttachmentFormat, new RDTextureView(), null);
		Texture.TextureRdRid = waterDisplacementMap;
	}


	protected override void DestructBehaviour(RenderingDevice renderingDevice) {
		if (computeShader.IsValid) {
			renderingDevice.FreeRid(computeShader);
			computeShader = default;
		}
		// Don't need to free the pipeline as freeing the shader does that for us.
		computePipeline = default;

		if (Texture is not null) {
			Texture.TextureRdRid = default;
		}
		if (waterDisplacementMap.IsValid) {
			renderingDevice.FreeRid(waterDisplacementMap);
			waterDisplacementMap = default;
		}
	}
}