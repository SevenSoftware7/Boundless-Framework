namespace LandlessSkies.Vanilla;

using Godot;

public partial class TrainEngine : PathFollow3D {
	[Export] public float speed = 3f;

	public override void _Process(double delta) {
		if (Engine.IsEditorHint()) return;

		Progress += speed * (float)delta;
	}
}
