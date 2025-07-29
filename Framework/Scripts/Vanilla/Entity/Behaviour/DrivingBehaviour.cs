namespace EndlessTwilight;

using Godot;
using Seven.Boundless;

[Tool]
[GlobalClass]
public partial class DrivingBehaviour : SittingBehaviour {
	[Export] public VehicleBehaviour Vehicle;
	private Vector3 _driverUp = Vector3.Up;

	private EntityBehaviour? previousBehaviour;

	protected override Transform3D SittingPosition => Vehicle?.Entity.CostumeHolder?.Costume?.GlobalTransform ?? Vehicle?.Entity.GlobalTransform ?? Transform3D.Identity;


	protected DrivingBehaviour() : this(null!, null!) { }
	public DrivingBehaviour(Entity entity, VehicleBehaviour vehicle) : base(entity) {
		Vehicle = vehicle;
	}

	protected override void _Start(EntityBehaviour? previousBehaviour) {
		if (Vehicle is null) {
			Stop();
			return;
		}
		_driverUp = Entity.UpDirection;

		base._Start(previousBehaviour);
	}

	protected override void _Stop(EntityBehaviour? nextBehaviour) {
		base._Stop(nextBehaviour);
		Entity.UpDirection = _driverUp;
		Entity.Inertia += Vehicle.Entity.Movement + Vehicle.Entity.Inertia;
	}

	public override void _Process(double delta) {
		base._Process(delta);
		Entity.UpDirection = Vehicle.Entity.UpDirection;
	}

	protected override void OnDismount() {
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