namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using Godot;
using SevenDev.Boundless.Utility;

[Tool]
[GlobalClass]
public partial class WaterDisplacementEffect : BaseCompositorEffect {
	public static readonly StringName Context = "WaterDisplacementEffect";
	public static readonly StringName DisplacementMapName = "water_displacement";
	[Export]
	private Texture2Drd? Texture {
		get;
		set {
			if (RenderingDevice is not null) Destruct();

			field = value;

			if (RenderingDevice is not null) Construct();
		}
	}

	[Export]
	private RDShaderFile? ComputeShaderFile {
		get;
		set {
			if (RenderingDevice is not null) Destruct();

			field = value;

			if (RenderingDevice is not null) Construct();
		}
	}
	private Rid computeShader;

	private Rid computePipeline;


	[Export]
	private RDShaderFile? FetchShaderFile {
		get;
		set {
			if (RenderingDevice is not null) Destruct();

			field = value;

			if (RenderingDevice is not null) Construct();
		}
	}
	private Rid fetchShader;

	private Rid fetchPipeline;

	private Rid displacementSampler;



	private readonly RDTextureFormat waterDisplacementMapAttachmentFormat = new() {
		Width = 128,
		Height = 128,
		TextureType = RenderingDevice.TextureType.Type2D,
		Format = RenderingDevice.DataFormat.R8G8B8A8Unorm,
		Samples = RenderingDevice.TextureSamples.Samples1,
		UsageBits = RenderingDevice.TextureUsageBits.StorageBit | RenderingDevice.TextureUsageBits.SamplingBit | RenderingDevice.TextureUsageBits.CanCopyFromBit
	};
	private Rid waterDisplacementMap;

	[Export] public bool FetchWaterDisplacement;
	public static readonly HashSet<IWaterDisplacementSubscriber> Subscribers = [];



	public WaterDisplacementEffect() : base() {
		EffectCallbackType = EffectCallbackTypeEnum.PreTransparent;
	}


	public override void _RenderCallback(int effectCallbackType, RenderData renderData) {
		base._RenderCallback(effectCallbackType, renderData);

		if (RenderingDevice is null || ComputeShaderFile is null || Texture is null) return;


		Vector2I renderSize = new(128, 128);
		(uint xGroups, uint yGroups) = CompositorExtensions.GetGroups(renderSize, 8);
		ComputeDisplacement(renderSize, xGroups, yGroups, 0.2f, waterDisplacementMap);

		FetchDisplacementData(xGroups, yGroups);



		void ComputeDisplacement(Vector2I renderSize, uint xGroups, uint yGroups, float frequency, Rid map) {
			int[] computePushConstantInts = [
				renderSize.X, renderSize.Y,
			];
			int computeIntsByteCount = computePushConstantInts.Length * sizeof(int);

			float[] computePushConstantFloats = [
				Time.GetTicksMsec() / 1000f, frequency
			];
			int computeFloatsByteCount = computePushConstantFloats.Length * sizeof(float);

			byte[] computePushConstantBytes = new byte[computeIntsByteCount + computeFloatsByteCount];
			Buffer.BlockCopy(computePushConstantInts, 0, computePushConstantBytes, 0, computeIntsByteCount);
			Buffer.BlockCopy(computePushConstantFloats, 0, computePushConstantBytes, computeIntsByteCount, computeFloatsByteCount);


			RenderingDevice.DrawCommandBeginLabel("Render Water Displacement", new Color(1f, 1f, 1f));
			long computeList = RenderingDevice.ComputeListBegin();
			RenderingDevice.ComputeListBindComputePipeline(computeList, computePipeline);

			RenderingDevice.ComputeListBindImage(computeList, computeShader, map, 0);

			RenderingDevice.ComputeListSetPushConstant(computeList, computePushConstantBytes, (uint)computePushConstantBytes.Length);

			RenderingDevice.ComputeListDispatch(computeList, xGroups, yGroups, 1);
			RenderingDevice.ComputeListEnd();
			RenderingDevice.DrawCommandEndLabel();
		}

		void FetchDisplacementData(uint xGroups, uint yGroups) {
			if (!FetchWaterDisplacement || FetchShaderFile is null) return;

			// ----- Get WaterDisplacementSubscriber Info -----
			Span<IWaterDisplacementSubscriber> subs = [.. Subscribers];
			if (subs.Length == 0) return;


			// ----- Create Input Buffer -----
			const int fetchInputStride = 4;
			float[] fetchInputs = new float[subs.Length * fetchInputStride];
			for (int i = 0; i < subs.Length; i++) {
				IWaterDisplacementSubscriber reader = subs[i];
				int index = i * fetchInputStride;
				(Vector3 location, WaterMesh mesh)? readerInfo = reader.GetInfo();

				if (readerInfo is null) {
					subs[i] = null!;
					continue;
				}

				fetchInputs[index] = readerInfo.Value.location.X;
				fetchInputs[index + 1] = readerInfo.Value.location.Z;
				fetchInputs[index + 2] = readerInfo.Value.mesh.WaterIntensity;
				fetchInputs[index + 3] = readerInfo.Value.mesh.WaterScale;
			}
			byte[] fetchInputBytes = CompositorExtensions.CreateByteBuffer(fetchInputs);
			Rid inputBuffer = RenderingDevice.StorageBufferCreate((uint)fetchInputBytes.Length, fetchInputBytes);


			// ----- Create Output Buffer -----
			const int fetchOutputStride = 4; // Pad 3 floats to 4
			float[] fetchOutputs = new float[subs.Length * fetchOutputStride];
			byte[] fetchOutputsBytes = new byte[fetchOutputs.Length * sizeof(float)];
			Rid outputbuffer = RenderingDevice.StorageBufferCreate((uint)fetchOutputsBytes.Length, fetchOutputsBytes);


			RenderingDevice.DrawCommandBeginLabel("Fetch Water Displacement", new Color(1f, 1f, 1f));
			long fetchList = RenderingDevice.ComputeListBegin();
			RenderingDevice.ComputeListBindComputePipeline(fetchList, fetchPipeline);

			RenderingDevice.ComputeListBindSampler(fetchList, fetchShader, waterDisplacementMap, displacementSampler, 0);
			RenderingDevice.ComputeListBindStorageBuffer(fetchList, fetchShader, inputBuffer, 1);
			RenderingDevice.ComputeListBindStorageBuffer(fetchList, fetchShader, outputbuffer, 2);

			RenderingDevice.ComputeListDispatch(fetchList, xGroups, yGroups, 1);
			RenderingDevice.ComputeListEnd();
			RenderingDevice.DrawCommandEndLabel();


			// ----- Read Output buffer -----
			byte[] outputData = RenderingDevice.BufferGetData(outputbuffer);
			Buffer.BlockCopy(outputData, 0, fetchOutputs, 0, outputData.Length);

			// ----- Notify WaterDisplacementSubscribers -----
			for (int i = 0; i < subs.Length; i++) {
				IWaterDisplacementSubscriber? reader = subs[i];
				int index = i * fetchOutputStride;
				if (reader is null) continue;

				Vector3 displacement = new(fetchOutputs[index], fetchOutputs[index + 1], fetchOutputs[index + 2]);
				reader.UpdateWaterDisplacement(displacement);
			}

			RenderingDevice.FreeRid(inputBuffer);
			RenderingDevice.FreeRid(outputbuffer);
		}

	}




	protected override void ConstructBehaviour(RenderingDevice renderingDevice) {
		if (ComputeShaderFile is null || Texture is null) return;

		displacementSampler = renderingDevice.SamplerCreate(new() {
			MinFilter = RenderingDevice.SamplerFilter.Linear,
			MagFilter = RenderingDevice.SamplerFilter.Linear,
			RepeatU = RenderingDevice.SamplerRepeatMode.Repeat,
			RepeatV = RenderingDevice.SamplerRepeatMode.Repeat,
			RepeatW = RenderingDevice.SamplerRepeatMode.Repeat,
		});

		computeShader = renderingDevice.ShaderCreateFromSpirV(ComputeShaderFile.GetSpirV());
		if (!computeShader.IsValid) {
			throw new ArgumentException("Compute Shader is Invalid");
		}

		computePipeline = renderingDevice.ComputePipelineCreate(computeShader);
		if (!computePipeline.IsValid) {
			throw new ArgumentException("Compute Pipeline is Invalid");
		}

		if (FetchShaderFile is not null) {
			fetchShader = renderingDevice.ShaderCreateFromSpirV(FetchShaderFile.GetSpirV());
			if (!fetchShader.IsValid) {
				throw new ArgumentException("Fetch Shader is Invalid");
			}

			fetchPipeline = renderingDevice.ComputePipelineCreate(fetchShader);
			if (!fetchPipeline.IsValid) {
				throw new ArgumentException("Fetch Pipeline is Invalid");
			}
		}


		waterDisplacementMap = renderingDevice.TextureCreate(waterDisplacementMapAttachmentFormat, new RDTextureView(), null);
		Texture.TextureRdRid = waterDisplacementMap;
	}


	protected override void DestructBehaviour(RenderingDevice renderingDevice) {
		if (displacementSampler.IsValid) {
			renderingDevice.FreeRid(displacementSampler);
			displacementSampler = default;
		}


		if (computeShader.IsValid) {
			renderingDevice.FreeRid(computeShader);
			computeShader = default;
		}
		// Don't need to free the pipeline as freeing the shader does that for us.
		computePipeline = default;


		if (fetchShader.IsValid) {
			renderingDevice.FreeRid(fetchShader);
			fetchShader = default;
		}
		// Don't need to free the pipeline as freeing the shader does that for us.
		fetchPipeline = default;


		if (Texture is not null) {
			Texture.TextureRdRid = default;
		}
		if (waterDisplacementMap.IsValid) {
			renderingDevice.FreeRid(waterDisplacementMap);
			waterDisplacementMap = default;
		}
	}
}