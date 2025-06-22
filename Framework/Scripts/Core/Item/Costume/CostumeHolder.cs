namespace SevenDev.Boundless;

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


	[Injectable]
	private IItemDataProvider? Registry {
		get;
		set {
			field = value;
			Reload();
		}
	}

	[Export] public ResourceItemKey CostumeKeyProvider {
		get;
		set {
			field = value;
			Reload();
		}
	} = new();

	public ItemKey? ItemKey {
		get => CostumeKeyProvider.ItemKey;
		set {
			CostumeKeyProvider.ItemKey = value;
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

	public void SetCostume(IItemData<Costume>? newCostume) {
		ItemKey = newCostume?.ItemKey;

		Load(true);
	}

	public void SetCostume(IPersistenceData<Costume> costumeData) {
		Unload();
		if (Registry is null) return;

		Costume = costumeData.Load(Registry)?.ParentTo(this);
		if (Costume?.Data?.KeyProvider is ResourceItemKey keyProvider) {
			CostumeKeyProvider = keyProvider;
		}
	}


	public void Reload() {
		if (!IsNodeReady()) return;

		Unload();

		if (Registry is null) return;
		if (ItemKey is not ItemKey itemKey) return;

		IItemData<Costume>? costumeData = Registry.GetData<Costume>(itemKey);
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