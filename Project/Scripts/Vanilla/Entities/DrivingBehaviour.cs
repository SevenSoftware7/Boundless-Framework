namespace LandlessSkies.Vanilla;

using Godot;
using LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class DrivingBehaviour : SittingBehaviour {
	[Export] public VehicleBehaviour? Vehicle;

	private EntityBehaviour? previousBehaviour;

	protected override Transform3D SittingPosition => Vehicle?.Entity?.Model?.GlobalTransform ?? Vehicle?.Entity?.GlobalTransform ?? Transform3D.Identity;


	protected DrivingBehaviour() : this(null!, null!) { }
	public DrivingBehaviour(Entity entity, VehicleBehaviour vehicle) : base(entity) { // Can't make Primary Constructor
		Vehicle = vehicle;
	}


	public override void Stop() {
		base.Stop();
		if (Entity is null || Vehicle?.Entity is null) return;

		Entity.Inertia += Vehicle.Entity.Velocity;
	}


	public override void HandlePlayer(Player player) {
		base.HandlePlayer(player);

		Vehicle?.HandlePlayer(player);
	}
	public override void DisavowPlayer() {
		base.DisavowPlayer();

		Vehicle?.DisavowPlayer();
	}
}