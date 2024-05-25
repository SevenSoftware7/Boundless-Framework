namespace LandlessSkies.Vanilla;

using Godot;
using LandlessSkies.Core;

[GlobalClass]
public partial class SeatInteractable : Interactable {
	[Export] private VehicleBehaviour? vehicle;
	public static readonly StringName Mount = "Mount";

	public override string InteractLabel => Mount;

	public override void Interact(Entity entity, Player? player = null, int shapeIndex = 0) {
		entity.SetBehaviour(new DrivingBehaviour(entity, vehicle!));
	}

	public override bool IsInteractable(Entity entity) => vehicle is not null;
}