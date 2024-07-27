using System.Linq;
using Godot;
using KGySoft.CoreLibraries;
using SevenDev.Utility;

namespace LandlessSkies.Core;

public sealed partial class WaterArea : Area3D {
	public WaterArea() {
		CollisionLayer = Collisions.Water;
		CollisionMask = Collisions.Entity | Collisions.Interactable;
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

	// public override void _Process(double delta) {
	// 	base._Process(delta);
	// 	GetOverlappingBodies().OfType<Entity>().ForEach(e =>
	// 		/* Callable.From(() =>  */e.PropagateAction<IWaterCollisionNotifier>(c => {
	// 			c.Enter(this);
	// 		})/* ).CallDeferred() */
	// 	);
	// }
}