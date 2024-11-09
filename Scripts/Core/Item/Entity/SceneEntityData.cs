namespace LandlessSkies.Core;

using Godot;
using SevenDev.Boundless.Persistence;

[Tool]
[GlobalClass]
public partial class SceneEntityData : SceneItemData<Entity> {
	public override IDataKeyProvider<Entity> KeyProvider => _keyProvider;
	[Export] private EntityResourceDataKey _keyProvider = new();
}
