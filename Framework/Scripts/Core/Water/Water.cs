namespace SevenDev.Boundless;

using Godot;
using SevenDev.Boundless.Utility;

[GlobalClass]
public sealed partial class Water : Area3D {
	[Export] public WaterMesh? Mesh { get; private set; }


	public Water() {
		CollisionLayer = CollisionLayers.Water;
		CollisionMask = CollisionLayers.Entity | CollisionLayers.Prop;
		Monitorable = false;
	}



	private void _OnBodyEntered(Node3D body) {
		body.PropagateAction<IWaterCollisionListener>(c => c.OnEnterWater(this));
	}

	private void _OnBodyExited(Node3D body) {
		body.PropagateAction<IWaterCollisionListener>(c => c.OnExitWater(this));
	}


	public override void _Ready() {
		base._Ready();

		BodyEntered += _OnBodyEntered;
		BodyExited += _OnBodyExited;
	}

}