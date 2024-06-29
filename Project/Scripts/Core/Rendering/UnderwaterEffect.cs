namespace LandlessSkies.Core;

using System;
using Godot;
using Godot.Collections;
using SevenDev.Utility;

[Tool]
[GlobalClass]
public partial class UnderwaterEffect : BaseCompositorEffect {
	public static readonly StringName Context = "UnderwaterEffect";
	public static readonly StringName WaterMapName = "water_map";
	public static readonly StringName WaterDepthName = "water_depth";


	[Export] private RDShaderFile? RenderShaderFile {
		get => _renderShaderFile;
		set {
			_renderShaderFile = value;

			if (RenderingDevice is not null) {
				Destruct();
				Construct();
			}
		}
	}
	private RDShaderFile? _renderShaderFile;
	private Rid renderShader;

	[Export] private RDShaderFile? ComputeShaderFile {
		get => _computeShaderFile;
		set {
			_computeShaderFile = value;

			if (RenderingDevice is not null) {
				Destruct();
				Construct();
			}
		}
	}
	private RDShaderFile? _computeShaderFile;
	private Rid computeShader;

	private Rid displacementSampler;
	private Rid depthSampler;

	[Export] private Texture2Drd? WaterDisplacementTexture;


	private readonly RDAttachmentFormat waterMapAttachmentFormat = new() {
		Format = RenderingDevice.DataFormat.R32G32B32A32Sfloat,
		Samples = RenderingDevice.TextureSamples.Samples1,
		UsageFlags = (uint)(RenderingDevice.TextureUsageBits.ColorAttachmentBit | RenderingDevice.TextureUsageBits.StorageBit)
	};
	private readonly RDAttachmentFormat waterDepthAttachmentFormat = new() {
		Format = RenderingDevice.DataFormat.D32Sfloat,
		Samples = RenderingDevice.TextureSamples.Samples1,
		UsageFlags = (uint)RenderingDevice.TextureUsageBits.DepthStencilAttachmentBit
	};
	private long framebufferFormat;

	private readonly RDVertexAttribute vertexAttribute = new() {
		Format = RenderingDevice.DataFormat.R32G32B32Sfloat,
		Location = 0,
		Stride = sizeof(float) * 3,
	};
	private long vertexFormat;

	private Rid renderPipeline;
	private Rid computePipeline;



	public UnderwaterEffect() : base() {
		EffectCallbackType = EffectCallbackTypeEnum.PostTransparent;
	}



	public override void _RenderCallback(int effectCallbackType, RenderData renderData) {
		base._RenderCallback(effectCallbackType, renderData);

		if (WaterDisplacementTexture is null || ! WaterDisplacementTexture.TextureRdRid.IsValid) return;

		// if (effectCallbackType != (long)EffectCallbackTypeEnum.PostTransparent) return;
		float[] waterMeshVertices = WaterMeshManager.WaterVertices;
		uint[] waterMeshIndices = WaterMeshManager.WaterIndices;
		if (waterMeshVertices.Length == 0 || waterMeshVertices.Length % 3 != 0 || waterMeshIndices.Length == 0 || waterMeshIndices.Length % 3 != 0) return;

		if (RenderingDevice is null || _renderShaderFile is null || _computeShaderFile is null) return;


		RenderSceneBuffers renderSceneBuffers = renderData.GetRenderSceneBuffers();
		if (renderSceneBuffers is not RenderSceneBuffersRD sceneBuffers) return;
		RenderSceneData renderSceneData = renderData.GetRenderSceneData();
		if (renderSceneData is not RenderSceneDataRD sceneData) return;

		uint viewCount = sceneBuffers.GetViewCount();


		Vector2I renderSize = sceneBuffers.GetInternalSize();
		if (renderSize.X == 0.0 && renderSize.Y == 0.0) {
			throw new ArgumentException("Render size is incorrect");
		}

		uint xGroups = (uint)((renderSize.X - 1) / 8) + 1;
		uint yGroups = (uint)((renderSize.Y - 1) / 8) + 1;


		if (sceneBuffers.HasTexture(Context, WaterMapName)) {
			// Reset the Color and Depth textures if their sizes are wrong
			RDTextureFormat textureFormat = sceneBuffers.GetTextureFormat(Context, WaterMapName);
			if (textureFormat.Width != renderSize.X || textureFormat.Height != renderSize.Y
				|| textureFormat.Format != waterMapAttachmentFormat.Format // Should only happen when actively changing the Format in the Editor
			) {
				sceneBuffers.ClearContext(Context);
			}
		}

		if (! sceneBuffers.HasTexture(Context, WaterMapName)) {
			// Create and cache the Map and Depth to create the Water Buffer
			sceneBuffers.CreateTexture(Context, WaterMapName, waterMapAttachmentFormat.Format, waterMapAttachmentFormat.UsageFlags, waterMapAttachmentFormat.Samples, renderSize, viewCount, 1, true);
			sceneBuffers.CreateTexture(Context, WaterDepthName, waterDepthAttachmentFormat.Format, waterDepthAttachmentFormat.UsageFlags, waterDepthAttachmentFormat.Samples, renderSize, viewCount, 1, true);
		}


		(Rid vertexBuffer, Rid vertexArray) = RenderingDevice.VertexArrayCreate(waterMeshVertices, vertexFormat);
		(Rid indexBuffer, Rid indexArray) = RenderingDevice.IndexArrayCreate(waterMeshIndices);

		Color[] clearColors = [new Color(0, 0, 0, 0)];
		for (uint view = 0; view < viewCount; view++) {
			Rid waterMap = sceneBuffers.GetTextureSlice(Context, WaterMapName, view, 0, 1, 1);
			Rid waterDepth = sceneBuffers.GetTextureSlice(Context, WaterDepthName, view, 0, 1, 1);

			// Include the Map and Depth from earlier
			Rid waterBuffer = RenderingDevice.FramebufferCreate([waterMap, waterDepth], framebufferFormat);
			if (! waterBuffer.IsValid) {
				throw new ArgumentException("Water Mask Frame Buffer is Invalid");
			}


			Projection projection = sceneData.GetViewProjection(view);
			float nearClippingPlane = projection.GetZNear();
			float farClippingPlane = projection.GetZFar();
			Projection transform = new(sceneData.GetCamTransform().Inverse());

			// World-space -> Clip-space Matrix to be used in the rendering shader
			Projection WorldToClip = projection * transform;
			// Eye Offset for fancy VR multi-view
			Vector3 eyeOffset = sceneData.GetViewEyeOffset(view);

			Dictionary waterScaleSetting = ProjectSettings.GetSetting("shader_globals/water_scale").AsGodotDictionary();
			float waterScale = waterScaleSetting["value"].As<float>();

			Dictionary waterIntensitySetting = ProjectSettings.GetSetting("shader_globals/water_intensity").AsGodotDictionary();
			float waterIntensity = waterIntensitySetting["value"].As<float>();

			// Unfolding into a push constant
			float[] renderPushConstant = [
				WorldToClip.X.X, WorldToClip.X.Y, WorldToClip.X.Z, WorldToClip.X.W,
				WorldToClip.Y.X, WorldToClip.Y.Y, WorldToClip.Y.Z, WorldToClip.Y.W,
				WorldToClip.Z.X, WorldToClip.Z.Y, WorldToClip.Z.Z, WorldToClip.Z.W,
				WorldToClip.W.X, WorldToClip.W.Y, WorldToClip.W.Z, WorldToClip.W.W,

				eyeOffset.X, eyeOffset.Y, // Don't pad for these two because they get packed together
				nearClippingPlane, farClippingPlane,
				waterScale, waterIntensity, 0, 0,

			];
			byte[] renderPushConstantBytes = new byte[renderPushConstant.Length * sizeof(float)];
			Buffer.BlockCopy(renderPushConstant, 0, renderPushConstantBytes, 0, renderPushConstantBytes.Length);


			// Render the Geometry (see vertCoords and vertIndices) to an intermediate framebuffer To use later
			RenderingDevice.DrawCommandBeginLabel("Render Water Mask", new Color(1f, 1f, 1f));
			long drawList = RenderingDevice.DrawListBegin(waterBuffer, RenderingDevice.InitialAction.Clear, RenderingDevice.FinalAction.Store, RenderingDevice.InitialAction.Clear, RenderingDevice.FinalAction.Discard, clearColors);
			RenderingDevice.DrawListBindRenderPipeline(drawList, renderPipeline);
			RenderingDevice.DrawListBindVertexArray(drawList, vertexArray);
			RenderingDevice.DrawListBindIndexArray(drawList, indexArray);
			RenderingDevice.DrawListBindSampler(drawList, renderShader, WaterDisplacementTexture.TextureRdRid, displacementSampler, 0);
			RenderingDevice.DrawListSetPushConstant(drawList, renderPushConstantBytes, (uint)renderPushConstantBytes.Length);
			RenderingDevice.DrawListDraw(drawList, true, 2);
			RenderingDevice.DrawListEnd();
			RenderingDevice.DrawCommandEndLabel();

			Dictionary waterColorSetting = ProjectSettings.GetSetting("shader_globals/water_color").AsGodotDictionary();
			Color waterColor = waterColorSetting["value"].AsColor();

			// Unfolding into a push constant
			float[] computePushConstant = [
				renderSize.X, renderSize.Y,
				nearClippingPlane, farClippingPlane,

				waterColor.R, waterColor.G, waterColor.B, 0,
			];
			byte[] computePushConstantBytes = new byte[computePushConstant.Length * sizeof(float)];
			Buffer.BlockCopy(computePushConstant, 0, computePushConstantBytes, 0, computePushConstantBytes.Length);

			// Here we draw the Underwater effect, using the waterBuffer to know where there is water geometry
			RenderingDevice.DrawCommandBeginLabel("Render Underwater Effect", new Color(1f, 1f, 1f));
			long computeList = RenderingDevice.ComputeListBegin();
			RenderingDevice.ComputeListBindComputePipeline(computeList, computePipeline);
			RenderingDevice.ComputeListBindColor(computeList, computeShader, sceneBuffers, view, 0);
			RenderingDevice.ComputeListBindDepth(computeList, computeShader, sceneBuffers, view, depthSampler, 1);
			RenderingDevice.ComputeListBindImage(computeList, computeShader, waterMap, 2);
			RenderingDevice.ComputeListSetPushConstant(computeList, computePushConstantBytes, (uint)computePushConstantBytes.Length);
			RenderingDevice.ComputeListDispatch(computeList, xGroups, yGroups, 1);
			RenderingDevice.ComputeListEnd();
			RenderingDevice.DrawCommandEndLabel();

			RenderingDevice.FreeRid(waterBuffer);
		}

		RenderingDevice.FreeRid(vertexArray);
		RenderingDevice.FreeRid(vertexBuffer);
		RenderingDevice.FreeRid(indexArray);
		RenderingDevice.FreeRid(indexBuffer);
	}




	protected override void ConstructBehaviour(RenderingDevice renderingDevice) {
		// Framebuffer Format includes a depth attachment to Self-occlude
		framebufferFormat = renderingDevice.FramebufferFormatCreate([waterMapAttachmentFormat, waterDepthAttachmentFormat]);
		vertexFormat = renderingDevice.VertexFormatCreate([vertexAttribute]);

		displacementSampler = renderingDevice.SamplerCreate(new() {
			MinFilter = RenderingDevice.SamplerFilter.Linear,
			MagFilter = RenderingDevice.SamplerFilter.Linear,
			RepeatU = RenderingDevice.SamplerRepeatMode.Repeat,
			RepeatV = RenderingDevice.SamplerRepeatMode.Repeat,
			RepeatW = RenderingDevice.SamplerRepeatMode.Repeat,
		});

		depthSampler = renderingDevice.SamplerCreate(new() {
			MinFilter = RenderingDevice.SamplerFilter.Nearest,
			MagFilter = RenderingDevice.SamplerFilter.Nearest
		});

		ConstructRenderPipeline(renderingDevice);
		ConstructComputePipeline(renderingDevice);
	}

	private void ConstructRenderPipeline(RenderingDevice renderingDevice) {
		if (RenderShaderFile is null) return;

		renderShader = renderingDevice.ShaderCreateFromSpirV(RenderShaderFile.GetSpirV());
		if (! renderShader.IsValid) {
			throw new ArgumentException("Render Shader is Invalid");
		}


		RDPipelineColorBlendState blend = new() {
			Attachments = [new RDPipelineColorBlendStateAttachment()]
		};

		renderPipeline = renderingDevice.RenderPipelineCreate(
			renderShader,
			framebufferFormat,
			vertexFormat,
			RenderingDevice.RenderPrimitive.Triangles,
			new RDPipelineRasterizationState(),
			new RDPipelineMultisampleState(),
			new RDPipelineDepthStencilState() {
				// Enable Self-occlusion via Depth Test
				EnableDepthTest = true,
				EnableDepthWrite = true,
				DepthCompareOperator = RenderingDevice.CompareOperator.LessOrEqual
			},
			blend
		);
		if (! renderPipeline.IsValid) {
			throw new ArgumentException("Render Pipeline is Invalid");
		}
	}


	private void ConstructComputePipeline(RenderingDevice renderingDevice) {
		if (ComputeShaderFile is null) return;

		computeShader = renderingDevice.ShaderCreateFromSpirV(ComputeShaderFile.GetSpirV());
		if (! computeShader.IsValid) {
			throw new ArgumentException("Compute Shader is Invalid");
		}

		computePipeline = renderingDevice.ComputePipelineCreate(computeShader);
		if (! computePipeline.IsValid) {
			throw new ArgumentException("Compute Pipeline is Invalid");
		}
	}


	protected override void DestructBehaviour(RenderingDevice renderingDevice) {
		if (displacementSampler.IsValid) {
			renderingDevice.FreeRid(displacementSampler);
			displacementSampler = default;
		}

		if (depthSampler.IsValid) {
			renderingDevice.FreeRid(depthSampler);
			depthSampler = default;
		}

		if (renderShader.IsValid) {
			renderingDevice.FreeRid(renderShader);
			renderShader = default;
		}
		// Don't need to free the pipeline as freeing the shader does that for us.
		renderPipeline = default;

		if (computeShader.IsValid) {
			renderingDevice.FreeRid(computeShader);
			computeShader = default;
		}
		// Same as above
		computePipeline = default;
	}
}