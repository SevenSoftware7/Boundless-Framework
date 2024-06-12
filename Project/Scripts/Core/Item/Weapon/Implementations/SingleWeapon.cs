namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using SevenDev.Utility;

[Tool]
[GlobalClass]
public abstract partial class SingleWeapon : Weapon, /* ICostumable<WeaponCostume>,  */IUIObject {
	[Export] private string _displayName = string.Empty;
	public override string DisplayName => _displayName;
	public override Texture2D? DisplayPortrait => CostumeHolder?.Costume?.DisplayPortrait;


	[ExportGroup("Costume")]
	// [Export] public WeaponCostume? Costume {
	// 	get => _costume;
	// 	set => SetCostume(value);
	// }
	// private WeaponCostume? _costume;
	[Export] public CostumeHolder? CostumeHolder;



	public override int Style {
		get => _style;
		set => _style = value % (StyleCount + 1);
	}
	private int _style;


	[Signal] public delegate void CostumeChangedEventHandler(WeaponCostume? newCostume, WeaponCostume? oldCostume);


	protected SingleWeapon() : base() { }
	public SingleWeapon(WeaponCostume? costume = null) {
		CostumeHolder = new CostumeHolder(costume).ParentTo(this);
	}


	public override List<ICustomizable> GetSubCustomizables() {
		List<ICustomizable> list = base.GetSubCustomizables();
		if (CostumeHolder?.Model is not null) list.Add(CostumeHolder.Model);
		return list;
	}

	public override ISaveData<Weapon> Save() => new SingleWeaponSaveData<SingleWeapon>(this);

	[Serializable]
	public class SingleWeaponSaveData<T>(T weapon) : SceneSaveData<Weapon>(weapon) where T : SingleWeapon {
		public string? CostumePath = weapon.CostumeHolder?.Costume?.ResourcePath;

		public override SingleWeapon? Load() {
			if (base.Load() is not SingleWeapon weapon) return null;

			if (CostumePath is not null) {
				WeaponCostume? costume = ResourceLoader.Load<WeaponCostume>(CostumePath);
				weapon.CostumeHolder = new CostumeHolder(costume).ParentTo(weapon);
			}

			return weapon;
		}
		// protected override WeaponCostume? GetCostume(T data) => data.Costume;
		// protected override void SetCostume(T data, WeaponCostume? costume) => data.Costume = costume;


		// public override SingleWeapon? Load() {
		// 	if (base.Load() is not SingleWeapon weapon) return null;

		// 	return base.Load();
		// }
	}
}