namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using KGySoft.CoreLibraries;


public static class WaterMeshManager {
	public static WaterMesh[] WaterMeshes { get; private set; } = [];
	public static float[] WaterVertices { get; private set; } = [];
	public static uint[] WaterIndices { get; private set; } = [];

	private static readonly Dictionary<WaterMesh, WaterMeshEntry> WaterMeshInfo = [];



	public static void Add(WaterMesh mesh) {
		if (mesh is null) return;

		if (WaterMeshInfo.TryGetValue(mesh, out WaterMeshEntry? entry) && entry is not null) {
			if (entry.SetMesh(mesh.Mesh)) {
				UpdateBuffer();
			}
		}
		else {
			AddInternal(mesh);
			UpdateBuffer();
		}

		UpdateBuffer();
	}
	public static void AddInternal(WaterMesh mesh) {
		if (mesh is null) return;

		WaterMeshInfo[mesh] = new WaterMeshEntry(mesh.Mesh);
	}

	public static void Remove(WaterMesh mesh) {
		if (mesh is null) return;

		RemoveInternal(mesh);

		UpdateBuffer();
	}
	private static void RemoveInternal(WaterMesh mesh) {
		if (mesh is null) return;

		WaterMeshInfo.Remove(mesh);
	}


	private static void UpdateBuffer() {
		int totalVertexCount = WaterMeshInfo.Sum(item => item.Value.Vertices.Length * 4);
		int totalIndexCount = WaterMeshInfo.Sum(item => item.Value.Indices.Length);

		// Allocate arrays
		WaterVertices = new float[totalVertexCount];
		WaterIndices = new uint[totalIndexCount];
		WaterMeshes = [.. WaterMeshInfo.Keys];

		// Create spans for filling data
		Span<float> vertexSpan = WaterVertices;
		Span<uint> indexSpan = WaterIndices;

		int vertexOffset = 0;
		int indexOffset = 0;
		uint meshIndex = 0;
		uint currentIndexOffset = 0;

		foreach (KeyValuePair<WaterMesh, WaterMeshEntry> item in WaterMeshInfo) {
			Vector3[] vertices = item.Value.Vertices;
			uint[] indices = item.Value.Indices;

			// Fill vertex span
			foreach (Vector3 vertex in vertices) {
				vertexSpan[vertexOffset++] = vertex.X;
				vertexSpan[vertexOffset++] = vertex.Y;
				vertexSpan[vertexOffset++] = vertex.Z;
				vertexSpan[vertexOffset++] = meshIndex;
			}

			// Fill index span
			for (int i = 0; i < indices.Length; i++) {
				indexSpan[indexOffset++] = indices[i] + currentIndexOffset;
			}

			// Update the index offset
			meshIndex++;
			currentIndexOffset += (uint)vertices.Length;
		}
	}



	private class WaterMeshEntry {
		public Mesh Mesh { get; private set; } = null!;
		public Vector3[] Vertices { get; private set; } = [];
		public uint[] Indices { get; private set; } = [];


		public WaterMeshEntry(Mesh mesh) {
			SetMesh(mesh);
		}


		public bool SetMesh(Mesh mesh) {
			if (mesh is null) return false;


			for (int i = 0; i < mesh.GetSurfaceCount(); ++i) {
				Godot.Collections.Array surfaceArrays = mesh.SurfaceGetArrays(i);

				Span<Vector3> vertices = surfaceArrays[(int)Mesh.ArrayType.Vertex].AsVector3Array();

				Vertices = new Vector3[vertices.Length];
				for (int j = 0; j < vertices.Length; j++) {
					Vertices[j] = vertices[j];
				}

				Indices = [.. surfaceArrays[(int)Mesh.ArrayType.Index].As<Godot.Collections.Array>().Select(i => i.AsUInt32())];
			}

			return true;
		}
	}
}