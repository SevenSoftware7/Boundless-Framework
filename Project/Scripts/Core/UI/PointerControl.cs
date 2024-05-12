using Godot;

namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class PointerControl : CenterContainer {
	public Transform3D? Target;
	[Export] public Camera3D? ProjectorCamera;

	public override void _Process(double delta) {
		base._Process(delta);

		Visible = Target is not null;
		if (! Target.HasValue || ProjectorCamera is null)
			return;

		Position = ProjectorCamera.UnprojectPosition(Target.Value.Origin);
	}
}
