namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public abstract partial class Costume : Resource, IUIObject {
	[Export] public PackedScene? ModelScene { get; private set; }

	public abstract string DisplayName { get; }
	public abstract Texture2D? DisplayPortrait { get; }


	public virtual Model? Instantiate() {
		Model? model = ModelScene?.Instantiate<Model>();
		if (model is not null) {
			model.Costume = this;
		}

		return model;
	}
}