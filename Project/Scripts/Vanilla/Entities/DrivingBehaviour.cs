namespace LandlessSkies.Vanilla;

using Godot;
using LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class DrivingBehaviour : SittingBehaviour {
	[Export] public VehicleBehaviour Vehicle;
	private EntityBehaviour? previousBehaviour;
	private PromptControl? leavePrompt;

	protected override Transform3D SittingPosition => Vehicle.Entity.Model?.GlobalTransform ?? Vehicle.Entity.GlobalTransform;


	protected DrivingBehaviour() : this(null!, null!) { }
	public DrivingBehaviour(Entity entity, VehicleBehaviour vehicle) : base(entity) {
		Vehicle = vehicle;
	}

	public override void Stop() {
		base.Stop();
		Entity.Inertia += Vehicle.Entity.Velocity;
	}


	public override void SetupPlayer(Player player) {
		base.SetupPlayer(player);

		Vehicle?.SetupPlayer(player);
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