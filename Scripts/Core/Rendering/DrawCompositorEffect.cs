namespace LandlessSkies.Core;

using Godot;
using Godot.Collections;
using SevenDev.Boundless.Utility;

[Tool]
[GlobalClass]
public partial class DrawCompositorEffect : BaseCompositorEffect {
	[Export]
	private RDShaderFile? DrawShaderFile {
		get;
		set {
			field = value;

			if (RenderingDevice is not null) {
				Destruct();
				Callable.From(Construct);
			}
		}
	}


	private Rid drawShader;

	private Rid frameBuffer;
	private Rid frameBufferTexture;

	private Rid indexBuffer;
	private Rid indexArray;

	private Rid vertexBuffer;
	private Rid vertexArray;

	private Rid renderPipeline;



	public DrawCompositorEffect() : base() {
		EffectCallbackType = EffectCallbackTypeEnum.PostTransparent;
	}


	protected override void ConstructBehaviour(RenderingDevice renderingDevice) {
		if (DrawShaderFile is null) return;
		drawShader = renderingDevice.ShaderCreateFromSpirV(DrawShaderFile.GetSpirV());

		if (!drawShader.IsValid) {
			GD.Print("Shader is Invalid");
			return;
		}


		RDTextureFormat tex_format = new() {
			TextureType = RenderingDevice.TextureType.Type2D,
			Height = 256,
			Width = 256,
			Format = RenderingDevice.DataFormat.R8G8B8A8Uint,
			UsageBits = RenderingDevice.TextureUsageBits.ColorAttachmentBit | RenderingDevice.TextureUsageBits.CanCopyFromBit
		};
		RDTextureView tex_view = new();

		frameBufferTexture = renderingDevice.TextureCreate(tex_format, tex_view);
		if (!frameBufferTexture.IsValid) {
			GD.Print("Frame Buffer Texture is Invalid");
			return;
		}


		Array<RDAttachmentFormat> attachments = [
			new RDAttachmentFormat() {
				Format = tex_format.Format,
				Samples = RenderingDevice.TextureSamples.Samples1,
				UsageFlags = (uint)tex_format.UsageBits
			}
		];
		long frameBufferFormat = renderingDevice.FramebufferFormatCreate(attachments);
		frameBuffer = renderingDevice.FramebufferCreate([frameBufferTexture], frameBufferFormat);
		if (!frameBuffer.IsValid) {
			GD.Print("Frame Buffer is Invalid");
			return;
		}




		Array<RDVertexAttribute> vertexAttributes = [
			new RDVertexAttribute() {
				Format = RenderingDevice.DataFormat.R32G32B32Sfloat,
				Location = 0,
				Stride = 4*3,
			}
		];

		long vertexFormat = renderingDevice.VertexFormatCreate(vertexAttributes);

		float[] points = [
			0,      0,      0,
			-0.25f, 0f,     0,
			-0.25f, 0.25f,  0,
			0,      0.25f,  0,
			0.25f,  0.25f,  0,
			0.25f,  0,      0,
		];
		vertexBuffer = renderingDevice.VertexBufferCreate(points);
		if (!vertexBuffer.IsValid) {
			GD.Print("Vertex Buffer is Invalid");
			return;
		}
		vertexArray = renderingDevice.VertexArrayCreate((uint)(points.Length / 3), vertexFormat, [vertexBuffer]);
		if (!vertexArray.IsValid) {
			GD.Print("Vertex Array is Invalid");
			return;
		}



		ushort[] indices = [
			0, 1, 2,
			0, 2, 3,
			0, 3, 4,
			0, 4, 5,
		];
		indexBuffer = renderingDevice.IndexBufferCreate(indices);
		if (!indexBuffer.IsValid) {
			GD.Print("Index Buffer is Invalid");
			return;
		}
		indexArray = renderingDevice.IndexArrayCreate(indexBuffer, 0, (uint)indices.Length);
		if (!indexArray.IsValid) {
			GD.Print("Index Array is Invalid");
			return;
		}


		RDPipelineColorBlendState blend = new();
		blend.Attachments.Add(new RDPipelineColorBlendStateAttachment());



		renderPipeline = renderingDevice.RenderPipelineCreate(
			drawShader,
			renderingDevice.FramebufferGetFormat(frameBuffer),
			vertexFormat,
			RenderingDevice.RenderPrimitive.Triangles,
			new RDPipelineRasterizationState(),
			new RDPipelineMultisampleState(),
			new RDPipelineDepthStencilState(),
			blend
		);
		if (!renderPipeline.IsValid) {
			GD.Print("Render Pipeline is Invalid");
			return;
		}
	}
	protected override void DestructBehaviour(RenderingDevice renderingDevice) {
		if (frameBuffer.IsValid) {
			renderingDevice.FreeRid(frameBuffer);
		}
		if (frameBufferTexture.IsValid) {
			renderingDevice.FreeRid(frameBufferTexture);
		}

		if (indexBuffer.IsValid) {
			renderingDevice.FreeRid(indexBuffer);
		}
		if (indexArray.IsValid) {
			renderingDevice.FreeRid(indexArray);
		}

		if (vertexBuffer.IsValid) {
			renderingDevice.FreeRid(vertexBuffer);
		}
		if (vertexArray.IsValid) {
			renderingDevice.FreeRid(vertexArray);
		}


		if (renderPipeline.IsValid) {
			renderingDevice.FreeRid(renderPipeline);
		}
		if (drawShader.IsValid) {
			renderingDevice.FreeRid(drawShader);
		}
	}

	public override void _RenderCallback(int effectCallbackType, RenderData renderData) {
		base._RenderCallback(effectCallbackType, renderData);
		if (RenderingDevice is null || DrawShaderFile is null) return;
		if (!drawShader.IsValid) {
			GD.Print("Shader not valid");
			return;
		}
		if (!renderPipeline.IsValid) {
			GD.Print("Render Pipeline not valid");
			return;
		}


		RenderingDevice.DrawCommandBeginLabel("Test Label", new Color(1f, 1f, 1f));

		Color[] clear_colors = [new Color(0, 0, 0, 0)];

		long drawList = RenderingDevice.DrawListBegin(frameBuffer, RenderingDevice.DrawFlags.ClearAll, clear_colors);
		RenderingDevice.DrawListBindRenderPipeline(drawList, renderPipeline);
		RenderingDevice.DrawListBindVertexArray(drawList, vertexArray);
		RenderingDevice.DrawListBindIndexArray(drawList, indexArray);
		RenderingDevice.DrawListDraw(drawList, true, 2);
		RenderingDevice.DrawListEnd(RenderingDevice.BarrierMask.AllBarriers);

		RenderingDevice.DrawCommandEndLabel();
	}
}