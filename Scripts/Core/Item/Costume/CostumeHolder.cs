namespace LandlessSkies.Core;

using Godot;
using Godot.Collections;
using SevenDev.Boundless.Utility;

[Tool]
[GlobalClass]
public sealed partial class CostumeHolder : Node3D/* , ISerializationListener */ {
	[Export] public Costume? Costume { get; private set; }

	[Export] public DataKey? CostumeKey {
		get => _costumeKey;
		set => SetCostume(value);
	}
	private DataKey? _costumeKey;


	public CostumeHolder() { }
	public CostumeHolder(IItemData<Costume>? costume) : this() {
		SetCostume(costume);
	}

	public void SetCostume(DataKey? newCostumeKey) {
		DataKey? oldKey = Costume?.Key;
		if (newCostumeKey is not null && newCostumeKey == oldKey) return;

		_costumeKey = newCostumeKey;

		Load(true);
	}
	public void SetCostume(IItemData<Costume>? newCostume) => SetCostume(newCostume?.Key);

	public void SetCostume(IPersistenceData<Costume> costumeData) {
		Unload();

		Costume? instance = costumeData.Load();
		_costumeKey = instance?.Key;
		Costume = instance?.SetOwnerAndParent(this);
	}


	public void Load() => Load(false);
	private void Load(bool forceReload = false) {
		if (Costume is not null && !forceReload) return;

		Unload();

		Costume = IItemData<Costume>.GetData(CostumeKey)?.Instantiate()?.SetOwnerAndParent(this);
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