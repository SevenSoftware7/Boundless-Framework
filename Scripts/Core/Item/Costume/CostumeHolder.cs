namespace LandlessSkies.Core;

using System.Collections.Generic;
using Godot;
using SevenDev.Boundless.Utility;
using SevenDev.Boundless.Persistence;

[Tool]
[GlobalClass]
public sealed partial class CostumeHolder : Node3D, ICustomizable {
	public Costume? Costume { get; private set; }

	private IDataKeyProvider<Costume>? _costumeKeyProvider;
	[Export] public CostumeResourceDataKey? CostumeKeyProvider {
		get => _costumeKeyProvider as CostumeResourceDataKey;
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

	public void SetCostume(IDataKeyProvider<Costume>? newCostumeKey) {
		_costumeKeyProvider = newCostumeKey;

		Load(true);
	}
	public void SetCostume(IItemData<Costume>? newCostume) => SetCostume(newCostume?.KeyProvider);

	public void SetCostume(IPersistenceData<Costume> costumeData) {
		Unload();

		Costume = costumeData.Load()?.ParentTo(this);
		_costumeKeyProvider = Costume?.ResourceKey;
	}


	public void Load() => Load(false);
	private void Load(bool forceReload = false) {
		if (Costume is not null && !forceReload) return;

		Unload();

		Costume = _costumeKeyProvider?.GetData()?.Instantiate()?.ParentTo(this);
	}
	public void Unload() {
		Costume?.QueueFree();
		Costume = null;
	}
	public void Reload() => Load(true);


	public override void _Ready() {
		base._Ready();
		Load();
	}
}