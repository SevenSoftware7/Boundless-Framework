namespace LandlessSkies.Vanilla;

using Godot;
using LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class DrivingBehaviour : SittingBehaviour {
	[Export] public VehicleBehaviour Vehicle;

	private EntityBehaviour? previousBehaviour;

	protected override Transform3D SittingPosition => Vehicle?.Entity?.CostumeHolder?.Model?.GlobalTransform ?? Vehicle?.Entity?.GlobalTransform ?? Transform3D.Identity;


	protected DrivingBehaviour() : this(null!, null!) { }
	public DrivingBehaviour(Entity entity, VehicleBehaviour vehicle) : base(entity) {
		Vehicle = vehicle;
	}


	protected override void _Stop() {
		base._Stop();
		if (Entity is null || Vehicle?.Entity is null) return;

		Entity.Inertia += Vehicle.Entity.Movement + Vehicle.Entity.Inertia;
	}

	public override void Dismount() {
		base.Dismount();
		Vehicle.Driver = null;
	}

	public override void HandlePlayer(Player player) {
		Vehicle.HandlePlayer(player); // Do this child-first because player handling is done in that order

		base.HandlePlayer(player);
	}
	public override void DisavowPlayer() {
		Vehicle.DisavowPlayer(); // Same here

		base.DisavowPlayer();
	}
}