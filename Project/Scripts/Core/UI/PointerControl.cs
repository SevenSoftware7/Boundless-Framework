using Godot;

namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class PointerControl : CenterContainer {
	[Export] public Node3D? Target;
	[Export] public Camera3D? ProjectorCamera;

	public override void _Process(double delta) {
		base._Process(delta);

		Visible = Target is not null;
		if (Target is null || ProjectorCamera is null)
			return;

		Position = ProjectorCamera.UnprojectPosition(Target.GlobalPosition);
	}
}
