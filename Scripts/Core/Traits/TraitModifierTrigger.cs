namespace LandlessSkies.Core;

using System.Threading.Tasks;
using Godot;

[GlobalClass]
public partial class TraitModifierTrigger : EntityTrigger {
	[Export] private Godot.Collections.Array<TraitModifier> _traitModifiers = [];


	protected override async void _EntityEntered(Entity entity) {
		foreach (TraitModifier traitModifier in _traitModifiers) {
			Task _ = entity.TraitModifiers.AddProgressively(traitModifier, 1000);
		}
		await Task.Delay(3000);
		foreach (TraitModifier traitModifier in _traitModifiers) {
			Task<bool> _ = entity.TraitModifiers.RemoveProgressively(traitModifier, 1000);
		}
	}
}