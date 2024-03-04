using Godot;
using Godot.Collections;
using System;


namespace LandlessSkies.Core;

[Tool]
public abstract partial class CharacterModel : Model {
	public override CharacterCostume Costume => (_costume as CharacterCostume)!;


	protected CharacterModel() : base() {}
	public CharacterModel(CharacterCostume costume) : base(costume) {}
}