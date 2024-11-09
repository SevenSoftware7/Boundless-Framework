namespace LandlessSkies.Core;

using Godot;

public abstract partial class DetectorArea3D<T> : Area3D {
	private void OnBodyEntered(Node3D body) {
		if (body is T target) {
			OnTargetEntered(target);
		}
	}

	protected abstract void OnTargetEntered(T target);


	public override void _Ready() {
		base._Ready();
		BodyEntered += OnBodyEntered;
	}
}