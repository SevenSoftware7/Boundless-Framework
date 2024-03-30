namespace LandlessSkies.Core;

using System.Collections.Generic;
using Godot;

[Tool]
[GlobalClass]
public abstract partial class Weapon : Loadable3D, IWeapon {
	public virtual int StyleCount { get; } = 1;
	public abstract int Style { get; set; }



	public abstract WeaponData WeaponData { get; protected set; }
	public abstract IWeapon.Type WeaponType { get; }
	public abstract IWeapon.Size WeaponSize { get; }
	public abstract Handedness Handedness { get; set; }

	public abstract IUIObject UIObject { get; }
	public virtual ICustomizable[] Children => [];
	public virtual ICustomizationParameter[] Customizations => [];


	public abstract IEnumerable<AttackInfo> GetAttacks(Entity target);
	public virtual void Inject(Entity? entity) { }


	// public virtual void HandleStyleInput(Player.InputInfo inputInfo) { }
	public virtual void HandleInput(CameraController3D cameraController, InputDevice inputDevice) { }

	public abstract ISaveData<Weapon> Save();
}