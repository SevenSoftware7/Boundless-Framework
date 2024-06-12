namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public sealed partial class WaterInstance : MeshInstance3D, ISerializationListener {
	private Transform3D? lastTransform = null;

	private void OnMeshChanged() {
		WaterMeshManager.Replace(Mesh, Mesh, GlobalTransform);
	}


	public override void _Process(double delta) {
		base._Process(delta);

		if (lastTransform is null || GlobalTransform != lastTransform) {
			WaterMeshManager.Replace(Mesh, Mesh, GlobalTransform);
			lastTransform = GlobalTransform;
		}
	}

	public override void _EnterTree() {
		base._EnterTree();
		WaterMeshManager.Add(Mesh, GlobalTransform);
	}
	public override void _ExitTree() {
		base._ExitTree();
		WaterMeshManager.Remove(Mesh);
	}

	public override bool _Set(StringName property, Variant value) {
		if (property == PropertyName.Mesh) {
			if (Mesh is not null) {
				WaterMeshManager.Remove(Mesh);
				if (Mesh.IsConnected(Mesh.SignalName.Changed, Callable.From(OnMeshChanged))) {
					Mesh.Changed -= OnMeshChanged;
				}
			}

			Mesh? newMesh = value.As<Mesh>();
			if (newMesh is not null) {
				WaterMeshManager.Add(newMesh);
				newMesh.Changed += OnMeshChanged;
			}
		}

		return base._Set(property, value);
	}

	public void OnBeforeSerialize() { }

	public void OnAfterDeserialize() {
		Mesh.Changed += OnMeshChanged;
	}
}