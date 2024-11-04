namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public partial class SceneCompanionData : SceneItemData<Companion> {
	public override IDataKeyProvider<Companion> KeyProvider => _keyProvider;
	[Export] private CompanionResourceDataKey _keyProvider = new();
}