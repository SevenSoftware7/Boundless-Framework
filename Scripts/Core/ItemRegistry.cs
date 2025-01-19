namespace LandlessSkies.Core;

using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using SevenDev.Boundless.Persistence;

[Tool]
[GlobalClass]
public partial class ItemRegistry : Node {
	public static readonly CompositeItemDataRegistry GlobalRegistry = new();
	private readonly ItemDataRegistry Registry = new(GD.Print);

	[Export] public Array<Resource> ItemData {
		get => [.._itemData.OfType<Resource>(), null];
		set {
			IEnumerable<IItemData> itemData = value.OfType<IItemData>();
			IEnumerable<IItemData> nonResourceData = itemData.Where(data => data is not Resource);
			_itemData = [..itemData, ..nonResourceData];

			if (IsNodeReady()) {
				Callable.From(RegisterData).CallDeferred();
			}
		}
	}
	private List<IItemData> _itemData = [];


	private void RegisterData() {
		Registry.Clear();
		foreach (IItemData data in _itemData) {
			Registry.RegisterData(data);
		}

		GlobalRegistry.AddRegistry(Registry);
	}

	public override void _Ready() {
		base._Ready();
		RegisterData();


		// if (Engine.IsEditorHint()) return;


		// Mod? mod = ModLoader.LoadInternalMod("TestMod");
		// mod?.Start();


		// mod?.Stop();
	}
}