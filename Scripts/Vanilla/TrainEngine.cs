namespace LandlessSkies.Vanilla;

using Godot;

public partial class TrainEngine : PathFollow3D {
	[Export] public double speed = 3f;

	public override void _Process(double delta) {
		Progress += (float)(speed * delta);
	}
}
