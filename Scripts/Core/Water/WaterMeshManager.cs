namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;


public static class WaterMeshManager {
	public static WaterMesh[] WaterMeshes { get; private set; } = [];
	public static float[] WaterVertices { get; private set; } = [];
	public static uint[] WaterIndices { get; private set; } = [];

	public static readonly Dictionary<WaterMesh, WaterMeshEntry> WaterMeshInfo = [];



	public static void Add(WaterMesh mesh, bool enabled = true) {
		if (mesh is null) return;

		if (WaterMeshInfo.TryGetValue(mesh, out WaterMeshEntry? entry) && entry is not null) {
			if (entry.SetMesh(mesh.Mesh)) {
				UpdateBuffer();
			}
		}
		else {
			AddInternal(mesh, enabled);
			UpdateBuffer();
		}
	}
	public static void AddInternal(WaterMesh mesh, bool enabled = true) {
		if (mesh is null) return;

		WaterMeshInfo[mesh] = new WaterMeshEntry(mesh.Mesh, enabled);
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

	public static void SetEnabled(WaterMesh mesh, bool enabled) {
		if (WaterMeshInfo.TryGetValue(mesh, out WaterMeshEntry? entry) && entry is not null) {
			entry.IsEnabled = enabled;
			UpdateBuffer();
		}
	}


	private static void UpdateBuffer() {
		IEnumerable<KeyValuePair<WaterMesh, WaterMeshEntry>> infos = WaterMeshInfo.Where(i => i.Value.IsEnabled);
		IEnumerable<WaterMesh> waterMeshes = infos.Select(i => i.Key);
		IEnumerable<WaterMeshEntry> entries = infos.Select(i => i.Value);

		int totalVertexCount = entries.Sum(e => e.Vertices.Length) * 4;
		int totalIndexCount = entries.Sum(e => e.Indices.Length);

		// Allocate arrays
		WaterVertices = new float[totalVertexCount];
		WaterIndices = new uint[totalIndexCount];
		WaterMeshes = [.. waterMeshes];

		// Create spans for filling data
		Span<float> vertexSpan = WaterVertices;
		Span<uint> indexSpan = WaterIndices;

		int vertexIndex = 0;
		int indexIndex = 0; // lol
		uint vertexIndexOffset = 0;

		uint meshIndex = 0;
		foreach (KeyValuePair<WaterMesh, WaterMeshEntry> item in infos) {
			Span<Vector3> vertices = item.Value.Vertices;
			Span<uint> indices = item.Value.Indices;

			// Fill vertex span
			foreach (Vector3 vertex in vertices) {
				vertexSpan[vertexIndex++] = vertex.X;
				vertexSpan[vertexIndex++] = vertex.Y;
				vertexSpan[vertexIndex++] = vertex.Z;
				vertexSpan[vertexIndex++] = meshIndex;
			}

			// Fill index span
			for (int i = 0; i < indices.Length; i++) {
				indexSpan[indexIndex++] = indices[i] + vertexIndexOffset;
			}

			// Update the index offset
			meshIndex++;
			vertexIndexOffset += (uint)vertices.Length;
		}
	}



	public class WaterMeshEntry {
		public Mesh Mesh { get; private set; } = null!;
		public Vector3[] Vertices { get; private set; } = [];
		public uint[] Indices { get; private set; } = [];
		public bool IsEnabled = true;


		public WaterMeshEntry(Mesh mesh, bool enabled = true) {
			SetMesh(mesh);
			IsEnabled = enabled;
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