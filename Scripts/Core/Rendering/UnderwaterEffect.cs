namespace LandlessSkies.Core;

using System;
using Godot;
using SevenDev.Boundless.Utility;

[Tool]
[GlobalClass]
public partial class UnderwaterEffect : BaseCompositorEffect {
	public static readonly StringName Context = "UnderwaterEffect";
	public static readonly StringName WaterMapName = "water_map";
	public static readonly StringName WaterDepthName = "water_depth";


	[Export]
	private RDShaderFile? RenderShaderFile {
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

	[Export]
	private RDShaderFile? ComputeShaderFile {
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
		UsageFlags = (uint)RenderingDevice.TextureUsageBits.DepthStencilAttachmentBit,
	};
	private long framebufferFormat;

	private readonly RDVertexAttribute vertexAttribute = new() {
		Format = RenderingDevice.DataFormat.R32G32B32A32Sfloat,
		Location = 0,
		Stride = sizeof(float) * 4,
	};
	private readonly uint vertexLength = 4;
	private long vertexFormat;

	private Rid renderPipeline;
	private Rid computePipeline;



	public UnderwaterEffect() : base() {
		EffectCallbackType = EffectCallbackTypeEnum.PreTransparent;
	}



	public override void _RenderCallback(int effectCallbackType, RenderData renderData) {
		base._RenderCallback(effectCallbackType, renderData);

		if (RenderingDevice is null || _renderShaderFile is null || _computeShaderFile is null) return;
		if (WaterDisplacementTexture is null || !WaterDisplacementTexture.TextureRdRid.IsValid) return;


		RenderSceneBuffers renderSceneBuffers = renderData.GetRenderSceneBuffers();
		if (renderSceneBuffers is not RenderSceneBuffersRD sceneBuffers) return;
		RenderSceneData renderSceneData = renderData.GetRenderSceneData();
		if (renderSceneData is not RenderSceneDataRD sceneData) return;

		uint viewCount = sceneBuffers.GetViewCount();

		(Vector2I renderSize, uint xGroups, uint yGroups) = sceneBuffers.GetRenderSize(8);


		// ----- Get Water Mesh Data -----
		float[] waterMeshVertices = WaterMeshManager.WaterVertices;
		uint[] waterMeshIndices = WaterMeshManager.WaterIndices;
		if (waterMeshVertices.Length == 0 || waterMeshIndices.Length == 0) return;

		(Rid vertexBuffer, Rid vertexArray) = RenderingDevice.VertexArrayCreate(waterMeshVertices, vertexFormat, vertexLength);
		(Rid indexBuffer, Rid indexArray) = RenderingDevice.IndexArrayCreate(waterMeshIndices, 3);

		// ----- Water Mesh Info -----
		WaterMesh[] waterMeshes = WaterMeshManager.WaterMeshes;
		(Rid infoBuffer, Rid parametersBuffer) = GetStorageBuffers(waterMeshes);


		// ----- Create Water Map -----
		CreateWaterMap(sceneBuffers, viewCount, renderSize);


		Color[] clearColors = [new Color(0, 0, 0, 0)];
		float clearDepth = 0f;

		for (uint view = 0; view < viewCount; view++) {
			Rid waterMap = sceneBuffers.GetTextureSlice(Context, WaterMapName, view, 0, 1, 1);
			Rid waterDepth = sceneBuffers.GetTextureSlice(Context, WaterDepthName, view, 0, 1, 1);

			Rid waterBuffer = RenderingDevice.FramebufferCreate([waterMap, waterDepth], framebufferFormat);
			if (!waterBuffer.IsValid) {
				throw new ArgumentException("Water Mask Frame Buffer is Invalid");
			}

			Projection projection = sceneData.GetViewProjection(view);
			float nearPlane = projection.GetZNear();
			float farPlane = projection.GetZFar();

			// ----- Render the Water Mask -----
			byte[] renderPushConstantBytes = GetDrawPushConstant(sceneData, projection);

			RenderingDevice.DrawCommandBeginLabel("Render Water Mask", new Color(1f, 1f, 1f));
			long drawList = RenderingDevice.DrawListBegin(waterBuffer, RenderingDevice.InitialAction.Clear, RenderingDevice.FinalAction.Store, RenderingDevice.InitialAction.Clear, RenderingDevice.FinalAction.Discard, clearColors, clearDepth);
			RenderingDevice.DrawListBindRenderPipeline(drawList, renderPipeline);

			RenderingDevice.DrawListBindVertexArray(drawList, vertexArray);
			RenderingDevice.DrawListBindIndexArray(drawList, indexArray);

			RenderingDevice.DrawListBindSampler(drawList, renderShader, WaterDisplacementTexture.TextureRdRid, displacementSampler, 0);
			RenderingDevice.DrawListBindStorageBuffer(drawList, renderShader, infoBuffer, 1);

			RenderingDevice.DrawListSetPushConstant(drawList, renderPushConstantBytes, (uint)renderPushConstantBytes.Length);

			RenderingDevice.DrawListDraw(drawList, true, 2);
			RenderingDevice.DrawListEnd();
			RenderingDevice.DrawCommandEndLabel();


			// ----- Render the Effect -----
			byte[] computePushConstantBytes = GetComputePushConstant(projection, nearPlane, farPlane, renderSize);

			RenderingDevice.DrawCommandBeginLabel("Render Underwater Effect", new Color(1f, 1f, 1f));
			long computeList = RenderingDevice.ComputeListBegin();
			RenderingDevice.ComputeListBindComputePipeline(computeList, computePipeline);

			RenderingDevice.ComputeListBindColor(computeList, computeShader, sceneBuffers, view, 0);
			RenderingDevice.ComputeListBindDepth(computeList, computeShader, sceneBuffers, view, depthSampler, 1);
			RenderingDevice.ComputeListBindImage(computeList, computeShader, waterMap, 2);
			RenderingDevice.ComputeListBindStorageBuffer(computeList, computeShader, parametersBuffer, 3);

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

		RenderingDevice.FreeRid(infoBuffer);
		RenderingDevice.FreeRid(parametersBuffer);



		void CreateWaterMap(RenderSceneBuffersRD sceneBuffers, uint viewCount, Vector2I renderSize) {
			if (sceneBuffers.HasTexture(Context, WaterMapName)) {
				// Reset the Color and Depth textures if their sizes are wrong

				RDTextureFormat textureFormat = sceneBuffers.GetTextureFormat(Context, WaterMapName);
				if (textureFormat.Width != renderSize.X || textureFormat.Height != renderSize.Y
#if TOOLS
					// Should only happen when actively changing the Format in the Editor
					|| textureFormat.Format != waterMapAttachmentFormat.Format
#endif
				) {
					sceneBuffers.ClearContext(Context);
				}
			}

			if (!sceneBuffers.HasTexture(Context, WaterMapName)) {
				// Create and cache the Map and Depth to create the Water Buffer

				sceneBuffers.CreateTexture(Context, WaterMapName, waterMapAttachmentFormat.Format, waterMapAttachmentFormat.UsageFlags, waterMapAttachmentFormat.Samples, renderSize, viewCount, 1, true);
				sceneBuffers.CreateTexture(Context, WaterDepthName, waterDepthAttachmentFormat.Format, waterDepthAttachmentFormat.UsageFlags, waterDepthAttachmentFormat.Samples, renderSize, viewCount, 1, true);
			}
		}

		(Rid infoBuffer, Rid parametersBuffer) GetStorageBuffers(WaterMesh[] waterMeshes) {
			const int waterInfoStride = 20; // Pad 18 floats to 20
			float[] waterInfoBuffer = new float[waterMeshes.Length * waterInfoStride];


			const int waterParametersStride = 12; // Pad 11 floats to 12
			float[] waterParametersBuffer = new float[waterMeshes.Length * waterParametersStride];


			for (int i = 0; i < waterMeshes.Length; i++) {
				WaterMesh mesh = waterMeshes[i];
				int waterInfoIndex = i * waterInfoStride;
				int waterParameterIndex = i * waterParametersStride;

				// Water Info for rendering the Water Mask

				Projection transform = new(mesh.GlobalTransform);
				float intensity = mesh.WaterIntensity;
				float scale = mesh.WaterScale;

				waterInfoBuffer[waterInfoIndex] = transform.X.X; waterInfoBuffer[waterInfoIndex + 1] = transform.X.Y; waterInfoBuffer[waterInfoIndex + 2] = transform.X.Z; waterInfoBuffer[waterInfoIndex + 3] = transform.X.W;
				waterInfoBuffer[waterInfoIndex + 4] = transform.Y.X; waterInfoBuffer[waterInfoIndex + 5] = transform.Y.Y; waterInfoBuffer[waterInfoIndex + 6] = transform.Y.Z; waterInfoBuffer[waterInfoIndex + 7] = transform.Y.W;
				waterInfoBuffer[waterInfoIndex + 8] = transform.Z.X; waterInfoBuffer[waterInfoIndex + 9] = transform.Z.Y; waterInfoBuffer[waterInfoIndex + 10] = transform.Z.Z; waterInfoBuffer[waterInfoIndex + 11] = transform.Z.W;
				waterInfoBuffer[waterInfoIndex + 12] = transform.W.X; waterInfoBuffer[waterInfoIndex + 13] = transform.W.Y; waterInfoBuffer[waterInfoIndex + 14] = transform.W.Z; waterInfoBuffer[waterInfoIndex + 15] = transform.W.W;

				waterInfoBuffer[waterInfoIndex + 16] = intensity;
				waterInfoBuffer[waterInfoIndex + 17] = scale;

				// Water Parameters for the Post-Processing effect

				Color shallowColor = mesh.ShallowColor.SrgbToLinear();
				Color deepColor = mesh.DeepColor.SrgbToLinear();

				waterParametersBuffer[waterParameterIndex] = shallowColor.R; waterParametersBuffer[waterParameterIndex + 1] = shallowColor.G; waterParametersBuffer[waterParameterIndex + 2] = shallowColor.B;
				waterParametersBuffer[waterParameterIndex + 3] = mesh.FogDistance;
				waterParametersBuffer[waterParameterIndex + 4] = deepColor.R; waterParametersBuffer[waterParameterIndex + 5] = deepColor.G; waterParametersBuffer[waterParameterIndex + 6] = deepColor.B;
				waterParametersBuffer[waterParameterIndex + 7] = mesh.FogFade;
				waterParametersBuffer[waterParameterIndex + 8] = mesh.TransparencyDistance;
				waterParametersBuffer[waterParameterIndex + 9] = mesh.TransparencyFade;
			}

			byte[] infoBytes = CompositorExtensions.CreateByteBuffer(waterInfoBuffer);
			infoBuffer = RenderingDevice.StorageBufferCreate((uint)infoBytes.Length, infoBytes);

			byte[] parametersBytes = CompositorExtensions.CreateByteBuffer(waterParametersBuffer);
			parametersBuffer = RenderingDevice.StorageBufferCreate((uint)parametersBytes.Length, parametersBytes);

			return (infoBuffer, parametersBuffer);
		}


		byte[] GetDrawPushConstant(RenderSceneDataRD sceneData, Projection projection) {
			Projection transform = new(sceneData.GetCamTransform().Inverse());

			Projection WorldToClip = projection * transform; // World-space -> Clip-space Matrix to be used in the rendering shader

			float[] renderPushConstant = [
				WorldToClip.X.X, WorldToClip.X.Y, WorldToClip.X.Z, WorldToClip.X.W,
				WorldToClip.Y.X, WorldToClip.Y.Y, WorldToClip.Y.Z, WorldToClip.Y.W,
				WorldToClip.Z.X, WorldToClip.Z.Y, WorldToClip.Z.Z, WorldToClip.Z.W,
				WorldToClip.W.X, WorldToClip.W.Y, WorldToClip.W.Z, WorldToClip.W.W,
			];
			byte[] renderPushConstantBytes = CompositorExtensions.CreateByteBuffer(renderPushConstant);
			return renderPushConstantBytes;
		}

		byte[] GetComputePushConstant(Projection projection, float nearPlane, float farPlane, Vector2I renderSize) {
			projection = projection.Inverse();
			renderSize -= Vector2I.One; // RenderSize is off by one, maybe a backend specific range, maybe a godot bug

			float[] computePushConstantFloats = [
				projection.X.X, projection.X.Y, projection.X.Z, projection.X.W,
				projection.Y.X, projection.Y.Y, projection.Y.Z, projection.Y.W,
				projection.Z.X, projection.Z.Y, projection.Z.Z, projection.Z.W,
				projection.W.X, projection.W.Y, projection.W.Z, projection.W.W,
				nearPlane, farPlane,
			];
			int computeFloatsByteCount = computePushConstantFloats.Length * sizeof(float);

			int[] computePushConstantInts = [
				renderSize.X, renderSize.Y,
			];
			int computeIntsByteCount = computePushConstantInts.Length * sizeof(int);

			byte[] computePushConstantBytes = new byte[computeIntsByteCount + computeFloatsByteCount];
			Buffer.BlockCopy(computePushConstantFloats, 0, computePushConstantBytes, 0, computeFloatsByteCount);
			Buffer.BlockCopy(computePushConstantInts, 0, computePushConstantBytes, computeFloatsByteCount, computeIntsByteCount);
			return computePushConstantBytes;
		}
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



		void ConstructRenderPipeline(RenderingDevice renderingDevice) {
			if (RenderShaderFile is null) return;

			renderShader = renderingDevice.ShaderCreateFromSpirV(RenderShaderFile.GetSpirV());
			if (!renderShader.IsValid) {
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
					DepthCompareOperator = RenderingDevice.CompareOperator.GreaterOrEqual
				},
				blend
			);
			if (!renderPipeline.IsValid) {
				throw new ArgumentException("Render Pipeline is Invalid");
			}
		}

		void ConstructComputePipeline(RenderingDevice renderingDevice) {
			if (ComputeShaderFile is null) return;

			computeShader = renderingDevice.ShaderCreateFromSpirV(ComputeShaderFile.GetSpirV());
			if (!computeShader.IsValid) {
				throw new ArgumentException("Compute Shader is Invalid");
			}

			computePipeline = renderingDevice.ComputePipelineCreate(computeShader);
			if (!computePipeline.IsValid) {
				throw new ArgumentException("Compute Pipeline is Invalid");
			}
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