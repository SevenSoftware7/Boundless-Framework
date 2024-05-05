#if TOOLS

namespace LandlessSkies.Core;

using System;
using System.Linq;
using Godot;
using Godot.Collections;

public partial class MultiWeapon {
	// [Export]
	// private Array<WeaponData> WeaponDatas {
	// 	get => [.. _weapons.Select(weapon => weapon?.WeaponData)];
	// 	set {
	// 		if (this.IsInitializationSetterCall()) {
	// 			return;
	// 		}

	// 		Array<WeaponData> datas = WeaponDatas;

	// 		// If nothing changed
	// 		if (datas.SequenceEqual(value))
	// 			return;

	// 		// If the items were only moved
	// 		if (datas.Count == value.Count && datas.Intersect(value).Count() == value.Count) {
	// 			Weapons = [.. value.Select(d => Weapons.FirstOrDefault(w => w?.WeaponData == d))];

	// 			UpdateCurrent();
	// 			return;
	// 		}

	// 		int minLength = Math.Min(value.Count, datas.Count);
	// 		for (int i = 0; i < Math.Max(value.Count, datas.Count); i++) {
	// 			switch (i) {
	// 				// If an item was modified
	// 				case int index when index < minLength && datas[index] != value[index]:
	// 					SetWeapon(index, value[index]);
	// 					break;

	// 				// If an item was added
	// 				case int index when index >= minLength && index < value.Count:
	// 					AddWeapon(value[index]);
	// 					break;

	// 				// If an item was removed
	// 				case int index when index >= minLength && index >= value.Count:
	// 					RemoveWeapon(index);
	// 					break;
	// 			}
	// 		}

	// 		UpdateCurrent();
	// 		NotifyPropertyListChanged();
	// 	}
	// }

	// public override void _ValidateProperty(Dictionary property) {
	// 	base._ValidateProperty(property);


	// 	StringName name = property["name"].AsStringName();


	// 	if (name == PropertyName.WeaponDatas) {
	// 		property["usage"] = (int)(property["usage"].As<PropertyUsageFlags>() & ~PropertyUsageFlags.Storage);
	// 	}
	// }
}

#endif