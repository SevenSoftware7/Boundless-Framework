namespace LandlessSkies.Core;

using Godot;

[Tool]
[GlobalClass]
public partial class SceneEntityData : SceneItemData<Entity> {
	public override IDataKeyProvider<Entity> KeyProvider => _keyProvider;
	[Export] private EntityResourceDataKey _keyProvider = new();
}
