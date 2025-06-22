namespace EndlessTwilight;

using Godot;
using SevenDev.Boundless;

[GlobalClass]
public partial class VehicleSeatInteractable : Interactable {
	public static readonly StringName Mount = "Mount";

	[Export] private VehicleBehaviour? Vehicle = null;

	public override string InteractLabel => Mount;
	public override float? MinLookIncidence => 0f;

	public override void Interact(Entity entity, Player? player = null, int shapeIndex = 0) {
		if (Vehicle is null || Vehicle.Driver is not null) return;
		entity.SetBehaviour(Vehicle.Driver = new DrivingBehaviour(entity, Vehicle));
	}

	public override bool IsInteractable(Entity entity) => Vehicle is not null;
}