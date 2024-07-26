namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using SevenDev.Utility;

[Tool]
[GlobalClass]
public partial class WaterDisplacementEffect : BaseCompositorEffect {
	public static readonly StringName Context = "WaterDisplacementEffect";
	public static readonly StringName DisplacementMapName = "water_displacement";
	[Export]
	private Texture2Drd? Texture {
		get => _texture;
		set {
			if (RenderingDevice is not null) Destruct();

			_texture = value;

			if (RenderingDevice is not null) Construct();
		}
	}
	private Texture2Drd? _texture;

	[Export]
	private RDShaderFile? ComputeShaderFile {
		get => _computeShaderFile;
		set {
			if (RenderingDevice is not null) Destruct();

			_computeShaderFile = value;

			if (RenderingDevice is not null) Construct();
		}
	}
	private RDShaderFile? _computeShaderFile;
	private Rid computeShader;

	private Rid computePipeline;


	[Export]
	private RDShaderFile? FetchShaderFile {
		get => _fetchShaderFile;
		set {
			if (RenderingDevice is not null) Destruct();

			_fetchShaderFile = value;

			if (RenderingDevice is not null) Construct();
		}
	}
	private RDShaderFile? _fetchShaderFile;
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

		if (RenderingDevice is null || _computeShaderFile is null || Texture is null) return;


		Vector2I renderSize = new(128, 128);
		if (renderSize.X == 0.0 && renderSize.Y == 0.0) {
			throw new ArgumentException("Render size is incorrect");
		}

		uint xGroups = (uint)((renderSize.X - 1) / 8) + 1;
		uint yGroups = (uint)((renderSize.Y - 1) / 8) + 1;


		// Unfolding into a push constant
		int[] computePushConstantInts = [
			renderSize.X, renderSize.Y,
		];
		int computeIntsByteCount = computePushConstantInts.Length * sizeof(int);

		float[] computePushConstantFloats = [
			Time.GetTicksMsec() / 1000f, 0
		];
		int computeFloatsByteCount = computePushConstantFloats.Length * sizeof(float);

		byte[] computePushConstantBytes = new byte[computeIntsByteCount + computeFloatsByteCount];
		Buffer.BlockCopy(computePushConstantInts, 0, computePushConstantBytes, 0, computeIntsByteCount);
		Buffer.BlockCopy(computePushConstantFloats, 0, computePushConstantBytes, computeIntsByteCount, computeFloatsByteCount);

		// Here we draw the Underwater effect, using the waterBuffer to know where there is water geometry
		RenderingDevice.DrawCommandBeginLabel("Render Water Displacement", new Color(1f, 1f, 1f));
		long computeList = RenderingDevice.ComputeListBegin();
		RenderingDevice.ComputeListBindComputePipeline(computeList, computePipeline);

		RenderingDevice.ComputeListBindImage(computeList, computeShader, waterDisplacementMap, 0);

		RenderingDevice.ComputeListSetPushConstant(computeList, computePushConstantBytes, (uint)computePushConstantBytes.Length);

		RenderingDevice.ComputeListDispatch(computeList, xGroups, yGroups, 1);
		RenderingDevice.ComputeListEnd();
		RenderingDevice.DrawCommandEndLabel();



		if (!FetchWaterDisplacement || _fetchShaderFile is null ) return;
		if (Subscribers.Count == 0) return;


		float waterScale = ProjectSettings.GetSetting("shader_globals/water_scale").AsGodotDictionary()["value"].As<float>();

		float waterIntensity = ProjectSettings.GetSetting("shader_globals/water_intensity").AsGodotDictionary()["value"].As<float>();


		float[] fetchPushConstantFloats = [
			waterScale, waterIntensity, 0, 0
		];
		int FetchFloatsByteCount = fetchPushConstantFloats.Length * sizeof(int);

		byte[] fetchPushConstantBytes = new byte[FetchFloatsByteCount];
		Buffer.BlockCopy(fetchPushConstantFloats, 0, fetchPushConstantBytes, 0, FetchFloatsByteCount);

		IWaterDisplacementSubscriber[] subs = [.. Subscribers];

		float[] locations = new float[subs.Length * 4];
		for (int i = 0; i < subs.Length; i+=4) {
			IWaterDisplacementSubscriber reader = subs[i];
			Vector3 readerLocation = reader.GetLocation();
			locations[i] = readerLocation.X;
			locations[i + 1] = readerLocation.X;
			locations[i + 2] = readerLocation.Z;
		}

		byte[]? locationBytes = new byte[locations.Length * sizeof(float)];
		Buffer.BlockCopy(locations, 0, locationBytes, 0, locationBytes.Length);

		Rid buffer = RenderingDevice.StorageBufferCreate((uint)locationBytes.Length, locationBytes);


		RenderingDevice.DrawCommandBeginLabel("Fetch Water Displacement", new Color(1f, 1f, 1f));
		long fetchList = RenderingDevice.ComputeListBegin();
		RenderingDevice.ComputeListBindComputePipeline(fetchList, fetchPipeline);

		RenderingDevice.ComputeListBindStorageBuffer(fetchList, fetchShader, buffer, 0);
		RenderingDevice.ComputeListBindSampler(fetchList, fetchShader, waterDisplacementMap, displacementSampler, 1);

		RenderingDevice.ComputeListSetPushConstant(fetchList, fetchPushConstantBytes, (uint)fetchPushConstantBytes.Length);

		RenderingDevice.ComputeListDispatch(fetchList, xGroups, yGroups, 1);
		RenderingDevice.ComputeListEnd();
		RenderingDevice.DrawCommandEndLabel();

		byte[] data = RenderingDevice.BufferGetData(buffer);
		Buffer.BlockCopy(data, 0, locations, 0, data.Length);


		for (int i = 0; i < subs.Length; i+=4) {
			IWaterDisplacementSubscriber reader = subs[i];
			reader.UpdateWaterDisplacement(new(locations[i], locations[i+1], locations[i+2]));
		}

		RenderingDevice.FreeRid(buffer);
	}




	protected override void ConstructBehaviour(RenderingDevice renderingDevice) {
		if (_computeShaderFile is null || _texture is null) return;

		displacementSampler = renderingDevice.SamplerCreate(new() {
			MinFilter = RenderingDevice.SamplerFilter.Linear,
			MagFilter = RenderingDevice.SamplerFilter.Linear,
			RepeatU = RenderingDevice.SamplerRepeatMode.Repeat,
			RepeatV = RenderingDevice.SamplerRepeatMode.Repeat,
			RepeatW = RenderingDevice.SamplerRepeatMode.Repeat,
		});

		computeShader = renderingDevice.ShaderCreateFromSpirV(_computeShaderFile.GetSpirV());
		if (!computeShader.IsValid) {
			throw new ArgumentException("Compute Shader is Invalid");
		}

		computePipeline = renderingDevice.ComputePipelineCreate(computeShader);
		if (!computePipeline.IsValid) {
			throw new ArgumentException("Compute Pipeline is Invalid");
		}

		if (_fetchShaderFile is not null) {
			fetchShader = renderingDevice.ShaderCreateFromSpirV(_fetchShaderFile.GetSpirV());
			if (!fetchShader.IsValid) {
				throw new ArgumentException("Fetch Shader is Invalid");
			}

			fetchPipeline = renderingDevice.ComputePipelineCreate(fetchShader);
			if (!fetchPipeline.IsValid) {
				throw new ArgumentException("Fetch Pipeline is Invalid");
			}
		}


		waterDisplacementMap = renderingDevice.TextureCreate(waterDisplacementMapAttachmentFormat, new RDTextureView(), null);
		_texture.TextureRdRid = waterDisplacementMap;
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