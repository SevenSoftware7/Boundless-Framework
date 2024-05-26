namespace LandlessSkies.Vanilla;

using Godot;
using LandlessSkies.Core;

[GlobalClass]
public partial class SeatInteractable : Interactable {
	[Export] private VehicleBehaviour? Vehicle = null;
	public static readonly StringName Mount = "Mount";

	public override string InteractLabel => Mount;

	public override void Interact(Entity entity, Player? player = null, int shapeIndex = 0) {
		if (Vehicle is null) return;
		entity.SetBehaviour(new DrivingBehaviour(entity, Vehicle));
	}

	public override bool IsInteractable(Entity entity) => Vehicle is not null;
}