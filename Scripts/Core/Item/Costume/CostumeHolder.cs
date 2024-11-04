namespace LandlessSkies.Core;

using Godot;
using Godot.Collections;
using SevenDev.Boundless.Utility;

[Tool]
[GlobalClass]
public sealed partial class CostumeHolder : Node3D/* , ISerializationListener */ {
	[Export] public Costume? Costume { get; private set; }

	[Export] public CostumeResourceDataKey? CostumeKeyProvider {
		get => _costumeKeyProvider as CostumeResourceDataKey;
		set => SetCostume(value);
	}
	private IDataKeyProvider<Costume>? _costumeKeyProvider;


	public CostumeHolder() { }
	public CostumeHolder(IItemData<Costume>? costume) : this() {
		SetCostume(costume);
	}

	public void SetCostume(IDataKeyProvider<Costume>? newCostumeKey) {
		CostumeResourceDataKey? oldKey = Costume?.ResourceKey;
		if (newCostumeKey is not null && newCostumeKey.Key == oldKey?.Key) return;

		_costumeKeyProvider = newCostumeKey;

		Load(true);
	}
	public void SetCostume(IItemData<Costume>? newCostume) => SetCostume(newCostume?.KeyProvider);

	public void SetCostume(IPersistenceData<Costume> costumeData) {
		Unload();

		Costume? instance = costumeData.Load();
		_costumeKeyProvider = instance?.ResourceKey;
		Costume = instance?.SetOwnerAndParent(this);
	}


	public void Load() => Load(false);
	private void Load(bool forceReload = false) {
		if (Costume is not null && !forceReload) return;

		Unload();

		Costume = _costumeKeyProvider?.GetData()?.Instantiate()?.SetOwnerAndParent(this);
	}
	public void Unload() {
		Costume?.QueueFree();
		Costume = null;
	}


	public override void _Ready() {
		base._Ready();
		Load();
	}

	public override void _ValidateProperty(Dictionary property) {
		base._ValidateProperty(property);

		StringName name = property["name"].AsStringName();


		if (name == PropertyName.Costume) {
			property["usage"] = (int)(property["usage"].As<PropertyUsageFlags>() & ~PropertyUsageFlags.Editor);
		}
	}
}