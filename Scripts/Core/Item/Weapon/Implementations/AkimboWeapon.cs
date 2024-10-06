namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using SevenDev.Boundless.Utility;
using SevenDev.Boundless.Injection;


[Tool]
[GlobalClass]
public sealed partial class AkimboWeapon : WeaponCollection, IInjectionInterceptor<Handedness> {
	public IWeapon? MainWeapon { get; private set; }
	public IWeapon? SideWeapon { get; private set; }

	public override uint Style {
		get => MainWeapon?.Style ?? 0;
		set {
			if (MainWeapon is not null && value <= MainWeapon.MaxStyle) {
				MainWeapon.Style = value;
			}
			else if (SideWeapon is not null && value == MaxStyle + 1) {
				SideWeapon.Style++;
			}
		}
	}
	public override uint MaxStyle => (MainWeapon?.MaxStyle ?? 0) + (uint)(SideWeapon is null ? 0 : 1);

	protected override IWeapon? Weapon => MainWeapon;


	private AkimboWeapon() : base() { }
	public AkimboWeapon(IWeapon? mainWeapon, IWeapon? sideWeapon) : this() {
		MainWeapon = mainWeapon;
		if (MainWeapon is Node mainWeaponNode) mainWeaponNode.ParentTo(this);

		SideWeapon = sideWeapon;
		if (SideWeapon is Node sideWeaponNode) sideWeaponNode.ParentTo(this);
	}
	public AkimboWeapon(ISaveData<IWeapon>? mainWeaponSave, ISaveData<IWeapon>? sideWeaponSave) : this(mainWeaponSave?.Load(), sideWeaponSave?.Load()) { }



	public override List<ICustomization> GetCustomizations() => base.GetCustomizations();
	public override List<ICustomizable> GetSubCustomizables() {
		List<ICustomizable> list = base.GetSubCustomizables();
		if (MainWeapon is not null) list.Add(MainWeapon);
		if (SideWeapon is not null) list.Add(SideWeapon);
		return list;
	}

	public override IEnumerable<Attack.Wrapper> GetAttacks(Entity target) {
		IWeapon? currentWeapon = MainWeapon;
		return new IWeapon?[] { MainWeapon, SideWeapon }
			.OfType<IWeapon>()
			.SelectMany(w => w.GetAttacks(target))
			.Concat(base.GetAttacks(target));
	}


	public Handedness Intercept(Node child, Handedness value) {
		if (child == SideWeapon) return value.Reverse();
		return value;
	}

	public override ISaveData<IWeapon> Save() {
		return new AkimboWeaponSaveData(this);
	}


	protected override void UpdateWeapons() {
		IWeapon[] weapons = GetChildren().OfType<IWeapon>().ToArray();
		MainWeapon = weapons.Length > 0 ? weapons[0] : null;
		if (MainWeapon is Node nodeMainWeapon) {
			nodeMainWeapon.SafeRename("Main");
		}

		SideWeapon = weapons.Length > 1 ? weapons[1] : null;
		if (SideWeapon is Node nodeSideWeapon) {
			nodeSideWeapon.SafeRename("Side");
		}
	}


	[Serializable]
	public class AkimboWeaponSaveData(AkimboWeapon akimbo) : ISaveData<AkimboWeapon> {
		private readonly ISaveData<IWeapon>? MainWeaponSave = akimbo.MainWeapon?.Save();
		private readonly ISaveData<IWeapon>? SideWeaponSave = akimbo.SideWeapon?.Save();

		public AkimboWeapon Load() {
			return new AkimboWeapon(MainWeaponSave, SideWeaponSave);
		}
	}
}