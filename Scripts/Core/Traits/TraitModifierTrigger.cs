namespace LandlessSkies.Core;

using System.Collections.Generic;
using System.Threading.Tasks;
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
		// // foreach (TraitModifier traitModifier in _traitModifiers) {
		// // 	Task _ = target.TraitModifiers.AddProgressively(traitModifier, 1000);
		// // }
		// // await Task.Delay(3000);
		// target.TraitModifiers.AddRange(_traitModifiers);
		// foreach (TraitModifier traitModifier in _traitModifiers) {
		// 	Task<bool> _ = target.TraitModifiers.RemoveProgressively(traitModifier, 1000);
		// }
		TraitModifierCollection targetTraitModifiers = target.TraitModifiers;
		foreach (TraitModifier item in _traitModifiers) {
			// if (targetTraitModifiers.Contains(item)) continue;
			AddChild(new TraitModifierApplier(1000, targetTraitModifiers, (TraitModifier)item.Duplicate()));
		}
	}
}