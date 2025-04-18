namespace LandlessSkies.Core;

using Godot;

public partial class TraitModifierOperation : Processor {
	public readonly ITraitModifier Modifier;
	public readonly TraitModifierCollection ModifierCollection;


	public TraitModifierOperation(TraitModifierCollection collection, ITraitModifier modifier) : base() {
		ModifierCollection = collection;
		Modifier = modifier;
	}
}