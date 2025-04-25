namespace LandlessSkies.Core;

using System.Collections.Generic;
using Godot;

[GlobalClass]
public partial class TraitModifierTrigger : EntityTrigger {
	[Export] private Godot.Collections.Array<TraitModifier> _traitModifiers = [];

	private readonly List<Entity> targets = [];


	protected override void _EntityEntered(Entity entity) {
		if (targets.Contains(entity)) return;

		targets.Add(entity);
	}

	public override void _PhysicsProcess(double delta) {
		base._Process(delta);
		foreach (Entity target in targets) {
			ApplyModifiers(target);
		}
		targets.Clear();
	}

	private void ApplyModifiers(Entity target) {
		TraitModifierCollection targetTraitModifiers = target.TraitModifiers;
		foreach (TraitModifier item in _traitModifiers) {
			TraitModifier modifier = (TraitModifier) item.Duplicate();

			targetTraitModifiers.Add(modifier);
			targetTraitModifiers.RemoveProgressively(target, modifier, 1000, 1000);
		}
	}
}