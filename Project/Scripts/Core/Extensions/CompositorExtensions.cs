namespace SevenDev.Utility;

using System;
using Godot;
using Godot.Collections;

public static class CompositorExtensions {
	public static Rid IndexBufferCreate(this RenderingDevice renderingDevice, ushort[] indices) {
		byte[] byteIndices = new byte[indices.Length * sizeof(ushort)];
		Buffer.BlockCopy(indices, 0, byteIndices, 0, byteIndices.Length);

		return renderingDevice.IndexBufferCreate((uint)indices.Length, RenderingDevice.IndexBufferFormat.Uint16, byteIndices);
	}
	public static Rid IndexBufferCreate(this RenderingDevice renderingDevice, uint[] indices) {
		byte[] byteIndices = new byte[indices.Length * sizeof(uint)];
		Buffer.BlockCopy(indices, 0, byteIndices, 0, byteIndices.Length);

		return renderingDevice.IndexBufferCreate((uint)indices.Length, RenderingDevice.IndexBufferFormat.Uint32, byteIndices);
	}

	public static (Rid indexBuffer, Rid indexArray)? IndexArrayCreate(this RenderingDevice renderingDevice, ushort[] indices, uint indexOffset = 0) {
		Rid indexBuffer = renderingDevice.IndexBufferCreate(indices);
		if (!indexBuffer.IsValid) {
			throw new ArgumentException("Index Buffer is Invalid");
		}
		Rid indexArray = renderingDevice.IndexArrayCreate(indexBuffer, indexOffset, (uint)indices.Length);
		if (!indexArray.IsValid) {
			throw new ArgumentException("Index Array is Invalid");
		}

		return (indexBuffer, indexArray);
	}

	public static (Rid indexBuffer, Rid indexArray) IndexArrayCreate(this RenderingDevice renderingDevice, uint[] indices, uint indexOffset = 0) {
		Rid indexBuffer = renderingDevice.IndexBufferCreate(indices);
		if (!indexBuffer.IsValid) {
			throw new ArgumentException("Index Buffer is Invalid");
		}
		Rid indexArray = renderingDevice.IndexArrayCreate(indexBuffer, indexOffset, (uint)indices.Length);
		if (!indexArray.IsValid) {
			throw new ArgumentException("Index Array is Invalid");
		}

		return (indexBuffer, indexArray);
	}

	public static Rid VertexBufferCreate(this RenderingDevice renderingDevice, float[] vertices) {
		if (vertices.Length % 3 != 0) throw new ArgumentException("Invalid number of values in the points buffer, there should be three float values per point.", nameof(vertices));
		byte[] byteVertices = new byte[vertices.Length * sizeof(uint)];
		Buffer.BlockCopy(vertices, 0, byteVertices, 0, byteVertices.Length);

		return renderingDevice.VertexBufferCreate((uint)byteVertices.Length, byteVertices);
	}

	public static (Rid vertexBuffer, Rid vertexArray) VertexArrayCreate(this RenderingDevice renderingDevice, float[] points, long vertexFormat) {
		Rid vertexBuffer = renderingDevice.VertexBufferCreate(points);
		if (!vertexBuffer.IsValid) {
			throw new ArgumentException("Vertex Buffer is Invalid");
		}
		Rid vertexArray = renderingDevice.VertexArrayCreate((uint)(points.Length / 3), vertexFormat, [vertexBuffer]);
		if (!vertexArray.IsValid) {
			throw new ArgumentException("Vertex Array is Invalid");
		}

		return (vertexBuffer, vertexArray);
	}

	public static (Rid framebufferTexture, Rid framebuffer) FramebufferCreate(this RenderingDevice renderingDevice, RDTextureFormat textureFormat, RDTextureView textureView, RenderingDevice.TextureSamples textureSamples = RenderingDevice.TextureSamples.Samples1) {
		Rid frameBufferTexture = renderingDevice.TextureCreate(textureFormat, textureView);
		if (!frameBufferTexture.IsValid) {
			throw new ArgumentException("Frame Buffer Texture is Invalid");
		}

		Array<RDAttachmentFormat> attachments = [
			new RDAttachmentFormat() {
				Format = textureFormat.Format,
				Samples = textureSamples,
				UsageFlags = (uint)textureFormat.UsageBits
			}
		];
		long frameBufferFormat = renderingDevice.FramebufferFormatCreate(attachments);
		Rid frameBuffer = renderingDevice.FramebufferCreate([frameBufferTexture], frameBufferFormat);
		if (!frameBuffer.IsValid) {
			throw new ArgumentException("Frame Buffer is Invalid");
		}

		return (frameBufferTexture, frameBuffer);
	}

	public static RDUniform AddIds(this RDUniform uniform, Span<Rid> ids) {
		for (int i = 0; i < ids.Length; i++) {
			uniform.AddId(ids[i]);
		}

		return uniform;
	}

	public static void ComputeListBind(this RenderingDevice device, long computeList, Rid shaderRid, Rid[] ids, RenderingDevice.UniformType uniformType, uint setIndex, int binding = 0) {
		RDUniform uniform = new RDUniform() {
			UniformType = uniformType,
			Binding = binding,
		}.AddIds(ids);

		device.ComputeListBindUniform(computeList, uniform, shaderRid, setIndex);
	}

	public static void ComputeListBindImage(this RenderingDevice device, long computeList, Rid shaderRid, Rid image, uint setIndex, int binding = 0) =>
		device.ComputeListBind(computeList, shaderRid, [image], RenderingDevice.UniformType.Image, setIndex, binding);

	public static void ComputeListBindSampler(this RenderingDevice device, long computeList, Rid shaderRid, Rid image, Rid sampler, uint setIndex, int binding = 0) =>
		device.ComputeListBind(computeList, shaderRid, [sampler, image], RenderingDevice.UniformType.SamplerWithTexture, setIndex, binding);

	public static void ComputeListBindColor(this RenderingDevice device, long computeList, Rid shaderRid, RenderSceneBuffersRD sceneBuffers, uint view, uint setIndex, int binding = 0) =>
		device.ComputeListBindImage(computeList, shaderRid, sceneBuffers.GetColorLayer(view), setIndex, binding);

	public static void ComputeListBindDepth(this RenderingDevice device, long computeList, Rid shaderRid, RenderSceneBuffersRD sceneBuffers, uint view, Rid sampler, uint setIndex, int binding = 0) =>
		device.ComputeListBindSampler(computeList, shaderRid, sceneBuffers.GetDepthLayer(view), sampler, setIndex, binding);

	public static void ComputeListBindStorageBuffer(this RenderingDevice device, long computeList, Rid shaderRid, Rid buffer, uint setIndex, int binding = 0) =>
		device.ComputeListBind(computeList, shaderRid, [buffer], RenderingDevice.UniformType.StorageBuffer, setIndex, binding);


	public static void ComputeListBindUniform(this RenderingDevice device, long computeList, RDUniform uniform, Rid shaderRid, uint setIndex) {
		Rid set = UniformSetCacheRD.GetCache(shaderRid, setIndex, [uniform]);

		device.ComputeListBindUniformSet(computeList, set, setIndex);
	}


	public static void DrawListBind(this RenderingDevice device, long drawList, Rid shaderRid, Rid[] ids, RenderingDevice.UniformType uniformType, uint setIndex, int binding = 0) {
		RDUniform uniform = new RDUniform() {
			UniformType = uniformType,
			Binding = binding,
		}.AddIds(ids);

		device.DrawListBindUniform(drawList, uniform, shaderRid, setIndex);
	}

	public static void DrawListBindImage(this RenderingDevice device, long drawList, Rid shaderRid, Rid image, uint setIndex, int binding = 0) =>
		device.DrawListBind(drawList, shaderRid, [image], RenderingDevice.UniformType.Image, setIndex, binding);

	public static void DrawListBindSampler(this RenderingDevice device, long drawList, Rid shaderRid, Rid image, Rid sampler, uint setIndex, int binding = 0) =>
		device.DrawListBind(drawList, shaderRid, [sampler, image], RenderingDevice.UniformType.SamplerWithTexture, setIndex, binding);

	public static void DrawListBindColor(this RenderingDevice device, long drawList, Rid shaderRid, RenderSceneBuffersRD sceneBuffers, uint view, uint setIndex, int binding = 0) =>
		device.DrawListBindImage(drawList, shaderRid, sceneBuffers.GetColorLayer(view), setIndex, binding);

	public static void DrawListBindDepth(this RenderingDevice device, long drawList, Rid shaderRid, RenderSceneBuffersRD sceneBuffers, uint view, Rid sampler, uint setIndex, int binding = 0) =>
		device.DrawListBindSampler(drawList, shaderRid, sceneBuffers.GetDepthLayer(view), sampler, setIndex, binding);


	public static void DrawListBindUniform(this RenderingDevice device, long drawList, RDUniform uniform, Rid shaderRid, uint setIndex) {
		Rid set = UniformSetCacheRD.GetCache(shaderRid, setIndex, [uniform]);

		device.DrawListBindUniformSet(drawList, set, setIndex);
	}
}