namespace Seven.Boundless;

using System.Collections.Generic;
using Godot;
using Seven.Boundless.Utility;
using Seven.Boundless.Persistence;
using Seven.Boundless.Injection;
using Seven.Boundless.Injection.Godot;


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

	[Export] private string ItemKeyString {
		get => ItemKey?.String ?? string.Empty;
		set => ItemKey = string.IsNullOrWhiteSpace(value) ? null : new ItemKey(value);
	}
	public ItemKey? ItemKey {
		get;
		set {
			field = value;
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
		if (Costume?.Data?.ItemKey.HasValue ?? false) {
			ItemKey = Costume.Data.ItemKey.Value;
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