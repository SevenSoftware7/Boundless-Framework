namespace LandlessSkies.Core;

using System.Collections.Generic;
using Godot;
using SevenDev.Boundless.Utility;
using SevenDev.Boundless.Persistence;
using SevenDev.Boundless.Injection;

[Tool]
[GlobalClass]
public sealed partial class CostumeHolder : Node3D, ICustomizable {
	public IInjectionNode InjectionNode { get; }

	public Costume? Costume { get; private set; }

	private IItemDataProvider? _registry;
	[Injectable]
	private IItemDataProvider? Registry {
		get => _registry;
		set {
			_registry = value;
			Reload();
		}
	}

	private IItemKeyProvider? _costumeKeyProvider;
	[Export] public CostumeResourceItemKey? CostumeKeyProvider {
		get => _costumeKeyProvider as CostumeResourceItemKey;
		set {
			_costumeKeyProvider = value;
			Reload();
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


	public CostumeHolder() {
		InjectionNode = new GodotNodeInjectionNode(this);
	}
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
		if (Registry is null) return;

		Costume = costumeData.Load(Registry)?.ParentTo(this);
		_costumeKeyProvider = Costume?.ResourceKey;
	}


	public void Reload() {
		Unload();

		if (_registry is null) return;
		if (_costumeKeyProvider?.ItemKey is null) return;

		IItemData<Costume>? costumeData = _registry.GetData<Costume>(_costumeKeyProvider.ItemKey);
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

	public void RequestInjection() {
		this.RequestInjection<IItemDataProvider>();
	}

	public override void _Ready() {
		base._Ready();

		RequestInjection();
	}

}