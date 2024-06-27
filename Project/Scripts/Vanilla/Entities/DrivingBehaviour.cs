namespace LandlessSkies.Vanilla;

using Godot;
using LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class DrivingBehaviour : SittingBehaviour {
	[Export] public VehicleBehaviour? Vehicle;

	private EntityBehaviour? previousBehaviour;

	protected override Transform3D SittingPosition => Vehicle?.Entity?.CostumeHolder?.Model?.GlobalTransform ?? Vehicle?.Entity?.GlobalTransform ?? Transform3D.Identity;


	protected DrivingBehaviour() : this(null!, null!) { }
	public DrivingBehaviour(Entity entity, VehicleBehaviour vehicle) : base(entity) {
		Vehicle = vehicle;
	}


	protected override void _Stop() {
		base._Stop();
		if (Entity is null || Vehicle?.Entity is null) return;

		Entity.Inertia += Vehicle.Entity.Velocity;
	}


	public override void HandlePlayer(Player player) {
		Vehicle?.HandlePlayer(player); // Do it in this order because player handling is done child-first

		base.HandlePlayer(player);
	}
	public override void DisavowPlayer() {
		Vehicle?.DisavowPlayer(); // Same here

		base.DisavowPlayer();
	}
}