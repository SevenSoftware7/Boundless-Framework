namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;

[Tool]
[GlobalClass]
public sealed partial class WaterInstance : MeshInstance3D, ISerializationListener {
	private Mesh? lastMesh = null;
	private Transform3D? lastTransform = null;

	private void OnMeshChanged() {
		WaterMeshManager.Replace(Mesh, Mesh, GlobalTransform);
	}


	public override void _Process(double delta) {
		base._Process(delta);

		if (lastTransform is null || lastMesh is null || GlobalTransform != lastTransform || Mesh != lastMesh) {
			if (lastMesh is not null) {
				lastMesh.Changed -= OnMeshChanged;
			}

			WaterMeshManager.Replace(lastMesh, Mesh, GlobalTransform);

			lastTransform = GlobalTransform;
			lastMesh = Mesh;

			if (lastMesh is not null) {
				lastMesh.Changed += OnMeshChanged;
			}
		}
	}

	public override void _EnterTree() {
		base._EnterTree();
		WaterMeshManager.Add(Mesh, GlobalTransform);

		lastMesh = Mesh;
		lastTransform = GlobalTransform;

		if (lastMesh is not null) {
			lastMesh.Changed += OnMeshChanged;
		}
	}
	public override void _ExitTree() {
		base._ExitTree();
		WaterMeshManager.Remove(Mesh);

		if (lastMesh is not null) {
			lastMesh.Changed -= OnMeshChanged;
		}

		lastMesh = null;
		lastTransform = null;
	}

	// public override bool _Set(StringName property, Variant value) {
	// 	if (property != PropertyName.Mesh) return base._Set(property, value);

	// 	Callable method = Callable.From(OnMeshChanged);

	// 	if (Mesh is not null) {
	// 		WaterMeshManager.Remove(Mesh);
	// 		if (Mesh.IsConnected(Mesh.SignalName.Changed, method)) {
	// 			Mesh.Disconnect(Mesh.SignalName.Changed, method);
	// 		}
	// 	}

	// 	Mesh = value.As<Mesh>();
	// 	if (Mesh is not null && ! Mesh.IsConnected(Mesh.SignalName.Changed, method)) {
	// 		WaterMeshManager.Add(Mesh);
	// 		if (! Mesh.IsConnected(Mesh.SignalName.Changed, method)) {
	// 			Mesh.Connect(Mesh.SignalName.Changed, method/* , (uint)ConnectFlags.Persist */);
	// 		}
	// 	}

	// 	return true;
	// }

	public void OnBeforeSerialize() {
		if (Mesh is not null) Mesh.Changed -= OnMeshChanged;
	}

	public void OnAfterDeserialize() {
		if (Mesh is not null) Mesh.Changed += OnMeshChanged;
	}
}