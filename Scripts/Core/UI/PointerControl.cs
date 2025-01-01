namespace LandlessSkies.Core;

using Godot;

[GlobalClass]
public partial class PointerControl : CenterContainer {
	public Transform3D? Target;
	[Export] public Camera3D? ProjectorCamera;

	public override void _Process(double delta) {
		base._Process(delta);

		Visible = Target is not null;
		if (!Target.HasValue || ProjectorCamera is null) return;

		Vector3 position = Target.Value.Origin;
		if (ProjectorCamera.IsPositionBehind(position)) {
			Visible = false;
			return;
		}

		Position = ProjectorCamera.UnprojectPosition(Target.Value.Origin);
	}
}
