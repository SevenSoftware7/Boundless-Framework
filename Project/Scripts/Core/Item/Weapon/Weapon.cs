namespace LandlessSkies.Core;

using System.Collections.Generic;
using Godot;

[Tool]
[GlobalClass]
public abstract partial class Weapon : Loadable3D, IWeapon, ISkeletonAdaptable {
	public virtual int StyleCount { get; } = 1;
	public abstract int Style { get; set; }


	public abstract IWeapon.Type WeaponType { get; }
	public abstract IWeapon.Usage WeaponUsage { get; }
	public abstract IWeapon.Size WeaponSize { get; }
	public abstract Handedness Handedness { get; protected set; }
	public abstract Skeleton3D? Skeleton { get; protected set; }

	public abstract IUIObject UIObject { get; }
	public virtual ICustomizable[] Children => [];
	public virtual ICustomizationParameter[] Customizations => [];


	public abstract IEnumerable<AttackInfo> GetAttacks(Entity target);
	public virtual void Inject(Entity? entity) { }


	// public virtual void HandleStyleInput(Player.InputInfo inputInfo) { }

	public virtual void HandleInput(Entity entity, CameraController3D cameraController, InputDevice inputDevice, HudManager hud) { }

	public abstract ISaveData<Weapon> Save();

	public virtual void SetParentSkeleton(Skeleton3D? skeleton) => Skeleton = skeleton;

	public virtual void SetHandedness(Handedness handedness) => Handedness = handedness;

}