namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;

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
		if (property != PropertyName.Mesh) return base._Set(property, value);

		Callable method = Callable.From(OnMeshChanged);

		if (Mesh is not null) {
			WaterMeshManager.Remove(Mesh);
			if (Mesh.IsConnected(Mesh.SignalName.Changed, method)) {
				Mesh.Disconnect(Mesh.SignalName.Changed, method);
			}
		}

		Mesh = value.As<Mesh>();
		if (Mesh is not null && ! Mesh.IsConnected(Mesh.SignalName.Changed, method)) {
			WaterMeshManager.Add(Mesh);
			if (! Mesh.IsConnected(Mesh.SignalName.Changed, method)) {
				Mesh.Connect(Mesh.SignalName.Changed, method, (uint)ConnectFlags.Persist);
			}
		}

		return true;
	}

	public void OnBeforeSerialize() { }

	public void OnAfterDeserialize() {
		Mesh.Changed += OnMeshChanged;
	}
}