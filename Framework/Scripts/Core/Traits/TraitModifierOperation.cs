namespace Seven.Boundless;

using Godot;

public partial class TraitModifierOperation : Node {
	public readonly ITraitModifier Modifier;
	public readonly TraitModifierCollection ModifierCollection;


	public TraitModifierOperation(TraitModifierCollection collection, ITraitModifier modifier) : base() {
		ModifierCollection = collection;
		Modifier = modifier;
	}
}