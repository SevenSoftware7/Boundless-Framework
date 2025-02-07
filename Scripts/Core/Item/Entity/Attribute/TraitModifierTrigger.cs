namespace LandlessSkies.Core;

using Godot;

[GlobalClass]
public partial class TraitModifierTrigger : EntityTrigger {
	[Export] private Godot.Collections.Array<TraitModifier> _traitModifiers = [];


	protected override void _EntityEntered(Entity entity) {
		entity.TraitModifiers.AddRange(_traitModifiers);
	}
}