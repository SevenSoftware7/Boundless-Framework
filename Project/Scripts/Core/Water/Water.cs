namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;

[Tool]
[GlobalClass]
public sealed partial class Water : Area3D {
	[Export] public WaterMesh? Mesh { get; private set; }


	public Water() {
		CollisionLayer = CollisionLayers.Water;
		CollisionMask = CollisionLayers.Entity | CollisionLayers.Prop;
		Monitorable = false;
	}



	private void OnBodyEntered(Node3D body) {
		body.PropagateAction<IWaterCollisionNotifier>(c => {
			c.OnEnterWater(this);
		});
	}

	private void OnBodyExited(Node3D body) {
		body.PropagateAction<IWaterCollisionNotifier>(c => {
			c.OnExitWater(this);
		});
	}


	public override void _Ready() {
		base._Ready();
		if (Engine.IsEditorHint()) return;

		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;
	}
}