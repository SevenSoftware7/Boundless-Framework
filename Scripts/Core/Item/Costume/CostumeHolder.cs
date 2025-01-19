namespace LandlessSkies.Core;

using System.Collections.Generic;
using Godot;
using SevenDev.Boundless.Utility;
using SevenDev.Boundless.Persistence;

[Tool]
[GlobalClass]
public sealed partial class CostumeHolder : Node3D, ICustomizable {
	public Costume? Costume { get; private set; }

	private IItemKeyProvider? _costumeKeyProvider;
	[Export] public CostumeResourceItemKey? CostumeKeyProvider {
		get => _costumeKeyProvider as CostumeResourceItemKey;
		set {
			_costumeKeyProvider = value;
			if (IsNodeReady()) {
				Callable.From(Reload).CallDeferred();
			}
		}
	}

	public string DisplayName => Costume?.DisplayName ?? string.Empty;
	public Texture2D? DisplayPortrait => Costume?.DisplayPortrait;

	public Dictionary<string, ICustomization> GetCustomizations() {
		Dictionary<string, ICustomization> customizations = [];
		// TODO: Add Costume picker here

		if (Costume?.GetCustomizations() is Dictionary<string, ICustomization> costumeCustomizations) {
			foreach (var customization in costumeCustomizations) {
				customizations[customization.Key] = customization.Value;
			}
		}

		return customizations;
	}


	public CostumeHolder() { }
	public CostumeHolder(IItemData<Costume>? costume) : this() {
		SetCostume(costume);
	}

	public void SetCostume(IItemKeyProvider? newCostumeKey) {
		_costumeKeyProvider = newCostumeKey;

		Load(true);
	}
	public void SetCostume(IItemData<Costume>? newCostume) => SetCostume(newCostume?.KeyProvider);

	public void SetCostume(IPersistenceData<Costume> costumeData) {
		Unload();

		Costume = costumeData.Load(ItemRegistry.GlobalRegistry)?.ParentTo(this);
		_costumeKeyProvider = Costume?.ResourceKey;
	}


	public void Reload() {
		Unload();

		if (_costumeKeyProvider?.ItemKey is null) return;

		IItemData<Costume>? costumeData = ItemRegistry.GlobalRegistry.GetData<Costume>(_costumeKeyProvider.ItemKey);
		Costume = costumeData?.Instantiate()?.ParentTo(this);
	}

	public void Load() => Load(false);
	private void Load(bool forceReload = false) {
		if (Costume is not null && !forceReload) return;

		Reload();
	}

	public void Unload() {
		Costume?.QueueFree();
		Costume = null;
	}


	public override void _Ready() {
		base._Ready();
		Load();
	}
}