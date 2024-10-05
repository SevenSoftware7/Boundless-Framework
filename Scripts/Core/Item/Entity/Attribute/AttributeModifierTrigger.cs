namespace LandlessSkies.Core;

using Godot;

[GlobalClass]
public partial class AttributeModifierTrigger : EntityTrigger {
	[Export] private Godot.Collections.Array<AttributeModifier> _attributeModifiers = [];


	protected override void _EntityEntered(Entity entity) {
		entity.AttributeModifiers.AddRange(_attributeModifiers);
	}
}