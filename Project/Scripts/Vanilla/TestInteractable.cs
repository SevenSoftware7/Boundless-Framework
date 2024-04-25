namespace LandlessSkies.Vanilla;

using Godot;
using LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class TestInteractable : Interactable {
	public override string InteractLabel { get; protected set; } = "Test";

	public override void Interact(Entity entity) {
		GD.Print($"Entity {entity.Name} interacted with {Name}");
	}

	public override bool IsInteractable(Entity entity) {
		return true;
	}
}