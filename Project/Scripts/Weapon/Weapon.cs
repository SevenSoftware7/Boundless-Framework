namespace LandlessSkies.Core;

using System.Collections.Generic;
using Godot;

[Tool]
[GlobalClass]
public abstract partial class Weapon : Loadable3D, IWeapon, IInjectable<Entity?> {
	public abstract int Style { get; set; }



	public abstract IWeapon.Type WeaponType { get; }
	public abstract IWeapon.Size WeaponSize { get; }
	public abstract Handedness Handedness { get; set; }

	public abstract IUIObject UIObject { get; }
	public virtual ICustomizable[] Children => [];
	public virtual ICustomizationParameter[] Customizations => [];


	public abstract IEnumerable<AttackAction.IInfo> GetAttacks(Entity target);
	public virtual void Inject(Entity? owner) { }


	public virtual void HandleInput(Player.InputInfo inputInfo) { }

	public abstract ISaveData<Weapon> Save();
}