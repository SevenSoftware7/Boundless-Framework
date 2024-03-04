using Godot;

namespace LandlessSkies.Core;

[Tool]
public abstract partial class CharacterModel : Model {
	public new CharacterCostume Costume {
		get => (base.Costume as CharacterCostume)!;
		set => base.Costume = value;
	}


	protected CharacterModel() : base() {}
	public CharacterModel(CharacterCostume costume) : base(costume) {}
}