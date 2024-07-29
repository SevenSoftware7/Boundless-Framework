namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public sealed partial class WaterMesh : MeshInstance3D, ISerializationListener {
	private Transform3D? lastTransform = null;


	public WaterMesh() : base() {
		Layers = VisualLayers.Water;
	}



	private void OnMeshChanged() {
		Mesh? mesh = Mesh;
		WaterMeshManager.UpdateTransform(mesh, GlobalTransform);
	}


	public override void _Process(double delta) {
		base._Process(delta);
		if (lastTransform is not null && GlobalTransform == lastTransform) return;

		Mesh? mesh = Mesh;

		lastTransform = GlobalTransform;
		WaterMeshManager.UpdateTransform(mesh, lastTransform.Value);
	}

	public override void _EnterTree() {
		base._EnterTree();

		Mesh? mesh = Mesh;

		lastTransform = GlobalTransform;
		WaterMeshManager.Add(mesh, lastTransform.Value);

		if (mesh is not null) {
			mesh.Changed += OnMeshChanged;
		}
	}
	public override void _ExitTree() {
		base._ExitTree();

		Mesh? mesh = Mesh;

		lastTransform = null;
		WaterMeshManager.Remove(mesh);

		if (mesh is not null) {
			mesh.Changed -= OnMeshChanged;
		}

	}

	public override bool _Set(StringName property, Variant value) {
		if (property != MeshInstance3D.PropertyName.Mesh) return base._Set(property, value);

		Mesh? mesh = Mesh;

		if (mesh is not null) {
			WaterMeshManager.Remove(mesh);
			mesh.Changed -= OnMeshChanged;
		}

		mesh = Mesh = value.As<Mesh>();

		if (mesh is not null) {
			WaterMeshManager.Add(mesh, GlobalTransform);
			mesh.Changed += OnMeshChanged;
		}

		return true;
	}

	public void OnBeforeSerialize() {
		if (Mesh is Mesh mesh) mesh.Changed -= OnMeshChanged;
	}

	public void OnAfterDeserialize() {
		if (Mesh is Mesh mesh) mesh.Changed += OnMeshChanged;
	}
}