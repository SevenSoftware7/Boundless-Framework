namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public abstract partial class SceneCostumeData : SceneItemData<Costume> {
	public override IDataKeyProvider<Costume> KeyProvider => _keyProvider;
	[Export] private CostumeResourceDataKey _keyProvider = new();
}