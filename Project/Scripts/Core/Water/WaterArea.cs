using Godot;
using SevenDev.Utility;

namespace LandlessSkies.Core;

public sealed partial class WaterArea : Area3D {
	public WaterArea() {
		CollisionLayer = Collisions.Water;
		CollisionMask = Collisions.Entity | Collisions.Interactable;
	}


	public void GetWaterSurfaceAt(Vector3 location) {

	}


	private void OnBodyEntered(Node3D body) {
		if (body is Entity entity)
			entity.PropagateAction<IWaterCollisionNotifier>(c => {
				c.Enter(this);
			});
	}

	private void OnBodyExited(Node3D body) {
		if (body is Entity entity)
			entity.PropagateAction<IWaterCollisionNotifier>(c => {
				c.Exit(this);
			});
	}


	public override void _Ready() {
		base._Ready();
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;
	}
}