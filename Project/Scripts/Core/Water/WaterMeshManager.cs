namespace LandlessSkies.Core;

using System.Collections.Generic;
using System.Linq;
using Godot;

public static class WaterMeshManager {
	public static float[] WaterVertices { get; private set; } = [];
	public static uint[] WaterIndices { get; private set; } = [];

	private static readonly List<Vector3> waterVertices = [];
	private static readonly List<uint> waterIndices = [];

	private static readonly Dictionary<Mesh, (int vertStartIndex, int indexStartIndex, int vertCount, int indexCount)> waterMeshes = [];

	public static void Add(Mesh mesh, Transform3D transform = default) {
		if (mesh is null) return;

		AddInternal(mesh, transform);

		UpdateBuffer();
	}
	public static void AddInternal(Mesh mesh, Transform3D transform = default) {
		if (mesh is null) return;

		for (int i = 0; i < mesh.GetSurfaceCount(); ++i) {
			Godot.Collections.Array surfaceArrays = mesh.SurfaceGetArrays(i);

			Vector3[] vertices = surfaceArrays[(int)Mesh.ArrayType.Vertex].AsVector3Array();
			uint[] indices = [.. surfaceArrays[(int)Mesh.ArrayType.Index].As<Godot.Collections.Array>().Select(i => i.AsUInt32())];


			int currentVertCount = waterVertices.Count;
			int currentIndicesCount = waterIndices.Count;
			int vertCount = vertices.Length;
			int indexCount = indices.Length;


			waterVertices.AddRange(vertices.Select(v => transform * v));
			waterIndices.AddRange(indices.Select(i => i + (uint)currentIndicesCount));

			waterMeshes[mesh] = (currentVertCount, currentIndicesCount, vertCount, indexCount);
		}
	}

	public static void Remove(Mesh mesh) {
		if (mesh is null) return;

		RemoveInternal(mesh);
		UpdateBuffer();
	}
	private static void RemoveInternal(Mesh mesh) {
		if (! waterMeshes.TryGetValue(mesh, out var res)) return;

		waterVertices.RemoveRange(res.vertStartIndex, res.vertCount);
		waterIndices.RemoveRange(res.indexStartIndex, res.indexCount);
		waterMeshes.Remove(mesh);
	}

	public static void Replace(Mesh oldMesh, Mesh newMesh, Transform3D transform = default) {
		if (oldMesh is null && newMesh is null) return;

		if (oldMesh is not null) {
			RemoveInternal(oldMesh);
		}
		if (newMesh is not null) {
			AddInternal(newMesh, transform);
		}

		UpdateBuffer();
	}

	private static void UpdateBuffer() {
		WaterVertices = [.. waterVertices.SelectMany<Vector3, float>(v => [v.X, v.Y, v.Z])];
		WaterIndices = [.. waterIndices];
	}
}