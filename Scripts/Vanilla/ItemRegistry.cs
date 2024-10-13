using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using LandlessSkies.Core;

namespace LandlessSkies.Vanilla;

[Tool]
[GlobalClass]
public partial class ItemRegistry : Node {
	[Export] public Array<Resource> ItemData {
		get => [.. _itemData.OfType<Resource>(), null];
		set {
			IEnumerable<IItemData> nonResourceData = value.OfType<IItemData>().Where(data => data is not Resource);
			_itemData = value.OfType<IItemData>().Concat(nonResourceData).ToList();
			Callable.From(RegisterData).CallDeferred();
		}
	}
	private List<IItemData> _itemData = [];

	private void RegisterData() {
		foreach (IItemData data in _itemData) {
			data.Register();
		}
	}
}