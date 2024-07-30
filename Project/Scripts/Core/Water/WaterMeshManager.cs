namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class WaterMeshManager {
	public static float[] WaterVertices { get; private set; } = [];
	public static uint[] WaterIndices { get; private set; } = [];

	private static readonly Dictionary<WaterMesh, WaterMeshEntry> WaterMeshes = [];



	public static void Add(WaterMesh mesh) {
		if (mesh is null) return;

		AddInternal(mesh);

		UpdateBuffer();
	}
	public static void AddInternal(WaterMesh mesh) {
		if (mesh is null) return;

		WaterMeshes[mesh] = new WaterMeshEntry(mesh.Mesh, mesh.GlobalTransform);
	}

	public static void Remove(WaterMesh mesh) {
		if (mesh is null) return;

		RemoveInternal(mesh);

		UpdateBuffer();
	}
	private static void RemoveInternal(WaterMesh mesh) {
		if (mesh is null) return;

		WaterMeshes.Remove(mesh);
	}


	public static void UpdateMesh(WaterMesh mesh) {
		if (mesh is null) return;

		if (WaterMeshes.TryGetValue(mesh, out WaterMeshEntry? entry)) {
			if (entry?.SetMesh(mesh.Mesh, mesh.GlobalTransform) ?? false) {
				UpdateBuffer();
			}
		}
		else {
			AddInternal(mesh);
			UpdateBuffer();
		}
	}

	public static void UpdateTransform(WaterMesh mesh) {
		if (mesh is null) return;

		if (WaterMeshes.TryGetValue(mesh, out WaterMeshEntry? entry)) {
			if (entry?.SetTransform(mesh.GlobalTransform) ?? false) {
				UpdateBuffer(mesh);
			}
		}
		else {
			AddInternal(mesh);
			UpdateBuffer();
		}
	}

	private static void UpdateBuffer(WaterMesh mesh) {
		// Find the offset for the specified mesh
		int vertexOffset = 0;
		foreach (KeyValuePair<WaterMesh, WaterMeshEntry> item in WaterMeshes) {
			if (item.Key == mesh) break;
			vertexOffset += item.Value.Vertices.Length * 3; // 3 for X, Y, Z components
		}

		// Get the vertices of the specified mesh
		if (WaterMeshes.TryGetValue(mesh, out WaterMeshEntry? entry)) {
			Vector3[] vertices = entry.Vertices;

			// Create a span for the WaterVertices array
			Span<float> vertexSpan = WaterVertices;

			// Fill the relevant part of the vertex span
			foreach (Vector3 vertex in vertices) {
				vertexSpan[vertexOffset++] = vertex.X;
				vertexSpan[vertexOffset++] = vertex.Y;
				vertexSpan[vertexOffset++] = vertex.Z;
			}
		}
	}

	private static void UpdateBuffer() {
		int totalVertexCount = WaterMeshes.Sum(item => item.Value.Vertices.Length * 3);
		int totalIndexCount = WaterMeshes.Sum(item => item.Value.Indices.Length);

		// Allocate arrays
		WaterVertices = new float[totalVertexCount];
		WaterIndices = new uint[totalIndexCount];

		// Create spans for filling data
		Span<float> vertexSpan = WaterVertices;
		Span<uint> indexSpan = WaterIndices;

		int vertexOffset = 0;
		int indexOffset = 0;
		uint currentIndexOffset = 0;

		foreach (KeyValuePair<WaterMesh, WaterMeshEntry> item in WaterMeshes) {
			Vector3[] vertices = item.Value.Vertices;
			uint[] indices = item.Value.Indices;

			// Fill vertex span
			foreach (Vector3 vertex in vertices) {
				vertexSpan[vertexOffset++] = vertex.X;
				vertexSpan[vertexOffset++] = vertex.Y;
				vertexSpan[vertexOffset++] = vertex.Z;
			}

			// Fill index span
			for (int i = 0; i < indices.Length; i++) {
				indexSpan[indexOffset++] = indices[i] + currentIndexOffset;
			}

			// Update the index offset
			currentIndexOffset += (uint)vertices.Length;
		}
	}



	private class WaterMeshEntry {
		public Mesh Mesh { get; private set; } = null!;
		public Transform3D Transform { get; private set; }
		public Vector3[] Vertices { get; private set; } = [];
		public uint[] Indices { get; private set; } = [];


		public WaterMeshEntry(Mesh mesh, Transform3D transform) {
			SetMesh(mesh, transform);
		}


		public bool SetMesh(Mesh mesh, Transform3D? transform = null) {
			if (mesh is null) return false;
			if (transform == Transform || mesh == Mesh) return false;

			Mesh = mesh;
			Transform = transform ?? Transform;

			for (int i = 0; i < mesh.GetSurfaceCount(); ++i) {
				Godot.Collections.Array surfaceArrays = mesh.SurfaceGetArrays(i);

				Span<Vector3> vertices = surfaceArrays[(int)Mesh.ArrayType.Vertex].AsVector3Array();

				Vertices = new Vector3[vertices.Length];
				for (int j = 0; j < vertices.Length; j++) {
					Vertices[j] = Transform * vertices[j];
				}

				Indices = [.. surfaceArrays[(int)Mesh.ArrayType.Index].As<Godot.Collections.Array>().Select(i => i.AsUInt32())];
			}

			return true;
		}

		public bool SetTransform(Transform3D newTransform) {
			if (Transform == newTransform) return false;

			Transform3D oldInverseTransform = Transform == default ? Transform3D.Identity : Transform.Inverse();

			Span<Vector3> vertices = Vertices;

			for (int i = 0; i < vertices.Length; i++) {
				Vertices[i] = newTransform * (oldInverseTransform * vertices[i]);
			}

			Transform = newTransform;
			return true;
		}
	}
}