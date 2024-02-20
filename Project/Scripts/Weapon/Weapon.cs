using System;
using System.Collections.Generic;
using Godot;

namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public abstract partial class Weapon : Loadable3D, IWeapon, IInjectable<Entity?> {
	public abstract WeaponData Data { get; protected set; }

	[ExportGroup("Costume")]
	public abstract WeaponCostume? Costume { get; set; }
	public abstract int Style { get; set; }



	public IWeapon.Type WeaponType => Data?.Type ?? 0;
	public abstract IWeapon.Handedness WeaponHandedness { get; set; }

	public virtual IUIObject UIObject => Data;
	public virtual ICustomizable[] Children => [];
	public virtual ICustomizationParameter[] Customizations => [];


	[Signal] public delegate void CostumeChangedEventHandler(WeaponCostume? newCostume, WeaponCostume? oldCostume);



	protected Weapon() : base() {}
	public Weapon(WeaponData data, WeaponCostume? costume) : base() {
		ArgumentNullException.ThrowIfNull(data);

		Data = data;
		SetCostume(costume ?? data.BaseCostume);
		Name = $"{nameof(Weapon)} - {Data.DisplayName}";
	}



	public abstract void SetCostume(WeaponCostume? costume);


	public abstract IEnumerable<AttackAction.IInfo> GetAttacks(Entity target);
	public virtual void Inject(Entity? owner) {}


	public virtual void HandleInput(Player.InputInfo inputInfo) {}
}