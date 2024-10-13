namespace LandlessSkies.Core;

using Godot;
using SevenDev.Boundless.Utility;

[Tool]
[GlobalClass]
public sealed partial class CostumeHolder : Node3D, ISerializationListener {
	public Costume? Costume { get; private set; }

	[Export] public InterfaceResource<IItemData<Costume>> CostumeData = new();


	public CostumeHolder() {
		CostumeData.OnSet += SetCostume;
	}
	public CostumeHolder(IItemData<Costume>? costume) : this() {
		SetCostume(costume);
	}

	private void SetCostume(IItemData<Costume>? oldCostume, IItemData<Costume>? newCostume) {
		if (newCostume == oldCostume) return;
		Load(true);
	}
	public void SetCostume(IItemData<Costume>? newCostume) {
		SetCostume(CostumeData.Value, newCostume);
	}

	public void SetCostume(IPersistenceData<Costume> costumeData) {
		Unload();

		Costume? instance = costumeData.Load();
		CostumeData.Value = instance?.Data?.Value;

		Costume = instance?.ParentTo(this);
	}

	public void Load(bool forceReload = false) {
		if (Costume is not null && !forceReload) return;

		Unload();

		Costume = CostumeData?.Value?.Instantiate()?.ParentTo(this);
	}
	public void Unload() {
		Costume?.QueueFree();
		Costume = null;
	}

	public override void _Ready() {
		base._Ready();
		Load();
	}

	public void OnBeforeSerialize() {
		Unload();
	}
	public void OnAfterDeserialize() {
		Callable.From(() => Load()).CallDeferred();
	}
}