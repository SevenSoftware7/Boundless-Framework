namespace Seven.Boundless;

using System;
using System.Collections.Generic;
using Godot;
using Seven.Boundless.Utility;

[Tool]
[GlobalClass]
public partial class WaterDisplacementEffect : BaseCompositorEffect {
	public static readonly StringName Context = "WaterDisplacementEffect";
	public static readonly StringName DisplacementMapName = "water_displacement";
	[Export]
	private Texture2Drd? Texture {
		get;
		set {
			DestructDisplacementMap();
			field = value;
			ConstructDisplacementMap();
		}
	}

	[Export]
	private RDShaderFile? ComputeShaderFile {
		get;
		set {
			DestructComputePipeline();
			field = value;
			ConstructComputePipeline();
		}
	}
	private Rid computeShader;

	private Rid computePipeline;


	[Export]
	private RDShaderFile? FetchShaderFile {
		get;
		set {
			DestructFetchPipeline();
			field = value;
			ConstructFetchPipeline();
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

		ConstructSampler();
	}


	public override void _RenderCallback(int effectCallbackType, RenderData renderData) {
		base._RenderCallback(effectCallbackType, renderData);

		if (RenderingDevice is null || ComputeShaderFile is null || Texture is null) return;


		Vector2I renderSize = new(128, 128);
		(uint xGroups, uint yGroups) = CompositorExtensions.GetGroups(renderSize, 8);
		ComputeDisplacement(renderSize, xGroups, yGroups, 0.2f, waterDisplacementMap);

		FetchDisplacementData();



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

		void FetchDisplacementData() {
			if (!FetchWaterDisplacement || FetchShaderFile is null) return;

			// ----- Get WaterDisplacementSubscriber Info -----
			Span<IWaterDisplacementSubscriber> subs = [.. Subscribers];
			uint subscriberCount = (uint)subs.Length;
			if (subscriberCount == 0) return;


			// ----- Create Input Buffer -----
			const int fetchInputStride = 4;
			float[] fetchInputs = new float[subscriberCount * fetchInputStride];
			for (int i = 0; i < subscriberCount; i++) {
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
			const int fetchOutputStride = 3;
			const int paddedUnitSize = 4;
			uint floatCount = subscriberCount * fetchOutputStride;
			uint padOverflow = floatCount % paddedUnitSize;
			uint paddedFloatCount = floatCount + (padOverflow == 0 ? 0 : paddedUnitSize - padOverflow);

			Rid outputbuffer = RenderingDevice.StorageBufferCreate(paddedFloatCount * sizeof(float));


			RenderingDevice.DrawCommandBeginLabel("Fetch Water Displacement", new Color(1f, 1f, 1f));
			long fetchList = RenderingDevice.ComputeListBegin();
			RenderingDevice.ComputeListBindComputePipeline(fetchList, fetchPipeline);

			RenderingDevice.ComputeListBindSampler(fetchList, fetchShader, waterDisplacementMap, displacementSampler, 0);
			RenderingDevice.ComputeListBindStorageBuffer(fetchList, fetchShader, inputBuffer, 1);
			RenderingDevice.ComputeListBindStorageBuffer(fetchList, fetchShader, outputbuffer, 2);

			RenderingDevice.ComputeListDispatch(fetchList, subscriberCount, 1, 1);
			RenderingDevice.ComputeListEnd();
			RenderingDevice.DrawCommandEndLabel();


			// ----- Read Output buffer -----
			float[] fetchOutputs = new float[floatCount];
			byte[] fetchOutputBytes = RenderingDevice.BufferGetData(outputbuffer, sizeBytes: floatCount * sizeof(float));
			Buffer.BlockCopy(fetchOutputBytes, 0, fetchOutputs, 0, fetchOutputBytes.Length);

			// ----- Notify WaterDisplacementSubscribers -----
			for (int i = 0; i < subscriberCount; i++) {
				IWaterDisplacementSubscriber? reader = subs[i];
				if (reader is null) continue;

				int displacementIndex = i * fetchOutputStride;

				Vector3 displacement = new(fetchOutputs[displacementIndex], fetchOutputs[displacementIndex + 1], fetchOutputs[displacementIndex + 2]);
				reader.UpdateWaterDisplacement(displacement);
			}

			RenderingDevice.FreeRid(inputBuffer);
			RenderingDevice.FreeRid(outputbuffer);
		}

	}




	protected override void ConstructBehaviour() {
		ConstructSampler();
		ConstructComputePipeline();
		ConstructFetchPipeline();
		ConstructDisplacementMap();
	}
	protected override void DestructBehaviour() {
		DestructSampler();
		DestructComputePipeline();
		DestructFetchPipeline();
		DestructDisplacementMap();
	}


	private void ConstructSampler() {
		displacementSampler = RenderingDevice.SamplerCreate(new() {
			MinFilter = RenderingDevice.SamplerFilter.Linear,
			MagFilter = RenderingDevice.SamplerFilter.Linear,
			RepeatU = RenderingDevice.SamplerRepeatMode.Repeat,
			RepeatV = RenderingDevice.SamplerRepeatMode.Repeat,
			RepeatW = RenderingDevice.SamplerRepeatMode.Repeat,
		});
	}
	private void DestructSampler() {
		if (displacementSampler.IsValid) {
			RenderingDevice.FreeRid(displacementSampler);
			displacementSampler = default;
		}
	}

	private void ConstructComputePipeline() {
		if (ComputeShaderFile is null) return;

		computeShader = RenderingDevice.ShaderCreateFromSpirV(ComputeShaderFile.GetSpirV());
		if (!computeShader.IsValid) {
			throw new ArgumentException("Compute Shader is Invalid");
		}

		computePipeline = RenderingDevice.ComputePipelineCreate(computeShader);
		if (!computePipeline.IsValid) {
			throw new ArgumentException("Compute Pipeline is Invalid");
		}
	}
	private void DestructComputePipeline() {
		if (computeShader.IsValid) {
			RenderingDevice.FreeRid(computeShader);
			computeShader = default;
		}
		// Don't need to free the pipeline as freeing the shader does that for us.
		else if (computePipeline.IsValid) {
			RenderingDevice.FreeRid(computePipeline);
		}
		computePipeline = default;
	}

	private void ConstructFetchPipeline() {
		if (FetchShaderFile is not null) {
			fetchShader = RenderingDevice.ShaderCreateFromSpirV(FetchShaderFile.GetSpirV());
			if (!fetchShader.IsValid) {
				throw new ArgumentException("Fetch Shader is Invalid");
			}

			fetchPipeline = RenderingDevice.ComputePipelineCreate(fetchShader);
			if (!fetchPipeline.IsValid) {
				throw new ArgumentException("Fetch Pipeline is Invalid");
			}
		}
	}
	private void DestructFetchPipeline() {
		if (fetchShader.IsValid) {
			RenderingDevice.FreeRid(fetchShader);
			fetchShader = default;
		}
		// Don't need to free the pipeline as freeing the shader does that for us.
		else if (fetchPipeline.IsValid) {
			RenderingDevice.FreeRid(fetchPipeline);
		}
		fetchPipeline = default;
	}

	private void ConstructDisplacementMap() {
		if (Texture is null) return;
		waterDisplacementMap = RenderingDevice.TextureCreate(waterDisplacementMapAttachmentFormat, new RDTextureView(), null);
		Texture.TextureRdRid = waterDisplacementMap;
	}
	private void DestructDisplacementMap() {
		if (Texture is not null) {
			Texture.TextureRdRid = default;
		}
		if (waterDisplacementMap.IsValid) {
			RenderingDevice.FreeRid(waterDisplacementMap);
			waterDisplacementMap = default;
		}
	}
}