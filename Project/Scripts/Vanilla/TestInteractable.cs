namespace LandlessSkies.Vanilla;

using Godot;
using LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class TestInteractable : Interactable {
	public override string InteractLabel => "Interact";
	public override float? MinLookIncidence => 0f;

	public override void Interact(Entity entity, int shapeIndex = 0) {
		GD.Print($"Entity {entity.Name} interacted with {Name}, shape {GetShape3D(shapeIndex)?.Name} (index {shapeIndex})");
	}

	public override bool IsInteractable(Entity entity) {
		return true;
	}
}