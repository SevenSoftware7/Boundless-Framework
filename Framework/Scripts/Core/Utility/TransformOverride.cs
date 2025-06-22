namespace SevenDev.Boundless;

using Godot;

[Tool]
[GlobalClass]
public partial class TransformOverride : Node3D {
	[Export] public Transform3D OverrideTransform = Transform3D.Identity;
	[Export] public bool OverridePositionX = false;
	[Export] public bool OverridePositionY = false;
	[Export] public bool OverridePositionZ = false;
	[Export] public bool OverrideRotation = false;


	public override void _Process(double delta) {
		base._Process(delta);

		GlobalTransform = GlobalTransform with {
			Origin = GlobalPosition with {
				X = OverridePositionX ? OverrideTransform.Origin.X : GlobalPosition.X,
				Y = OverridePositionY ? OverrideTransform.Origin.Y : GlobalPosition.Y,
				Z = OverridePositionZ ? OverrideTransform.Origin.Z : GlobalPosition.Z,
			},
			Basis = OverrideRotation ? OverrideTransform.Basis : GlobalBasis
		};
	}
}
