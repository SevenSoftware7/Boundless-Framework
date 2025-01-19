namespace LandlessSkies.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using SevenDev.Boundless.Utility;
using SevenDev.Boundless.Injection;
using SevenDev.Boundless.Persistence;


[Tool]
[GlobalClass]
public sealed partial class AkimboWeapon : CompositeWeapon, IInjectionInterceptor<Handedness>, IInjectionBlocker<StyleState>, IInjectionInterceptor<StyleState>, IPersistent<AkimboWeapon> {
	public IInjectionNode InjectionNode { get; }

	public IWeapon? MainWeapon {
		get => _mainWeapon;
		private set {
			_mainWeapon = value;
			if (value is Node mainWeaponNode) mainWeaponNode.SafeReparentTo(this);
		}
	}
	private IWeapon? _mainWeapon;

	public IWeapon? SideWeapon {
		get => _sideWeapon;
		private set {
			_sideWeapon = value;
			if (value is Node sideWeaponNode) sideWeaponNode.SafeReparentTo(this);
		}
	}
	private IWeapon? _sideWeapon;


	public override StyleState Style => MainWeapon?.Style ?? 0;
	bool IInjectionBlocker<StyleState>.ShouldBlock(IInjectionNode child, StyleState value) {
		object childObject = child.UnderlyingObject;
		if (childObject == MainWeapon && value > MainWeapon!.MaxStyle) return true;
		if (childObject == SideWeapon && value < MaxStyle) return true;
		return false;
	}
	StyleState IInjectionInterceptor<StyleState>.Intercept(IInjectionNode child, StyleState value) {
		object childObject = child.UnderlyingObject;
		if (childObject == MainWeapon) return value;
		if (childObject == SideWeapon) return SideWeapon?.Style + 1 ?? 0;
		return value;
	}
	public override StyleState MaxStyle => (MainWeapon?.MaxStyle ?? 0) + (SideWeapon is null ? 0 : 1);

	public override IWeapon? CurrentWeapon => MainWeapon;


	private AkimboWeapon() : base() {
		InjectionNode = new GodotNodeInjectionNode(this);
	}
	public AkimboWeapon(IWeapon? mainWeapon, IWeapon? sideWeapon) : this() {
		MainWeapon = mainWeapon;
		SideWeapon = sideWeapon;
	}
	public AkimboWeapon(IPersistenceData<IWeapon>? mainWeaponSave, IPersistenceData<IWeapon>? sideWeaponSave, IItemDataRegistry registry) : this(mainWeaponSave?.Load(registry), sideWeaponSave?.Load(registry)) { }


	public override Dictionary<string, ICustomization> GetCustomizations() => base.GetCustomizations();
	public override IEnumerable<IUIObject> GetSubObjects() {
		IEnumerable<IUIObject> subObjects = base.GetSubObjects();
		if (MainWeapon is not null) subObjects = subObjects.Append(MainWeapon);
		if (SideWeapon is not null) subObjects = subObjects.Append(SideWeapon);
		return subObjects;
	}

	public override IEnumerable<Action.Wrapper> GetAttacks(Entity target) {
		IWeapon? currentWeapon = MainWeapon;
		return new IWeapon?[] { MainWeapon, SideWeapon }
			.OfType<IWeapon>()
			.SelectMany(w => w.GetAttacks(target))
			.Concat(base.GetAttacks(target));
	}


	Handedness IInjectionInterceptor<Handedness>.Intercept(IInjectionNode child, Handedness value) {
		if (child.UnderlyingObject == SideWeapon) return value.Reverse();
		return value;
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


	public override IPersistenceData<AkimboWeapon> Save() => new AkimboWeaponSaveData(this);


	[Serializable]
	public class AkimboWeaponSaveData(AkimboWeapon akimbo) : PersistenceData<AkimboWeapon>(akimbo) {
		private readonly IPersistenceData<IWeapon>? MainWeaponSave = (akimbo.MainWeapon as IPersistent<IWeapon>)?.Save();
		private readonly IPersistenceData<IWeapon>? SideWeaponSave = (akimbo.SideWeapon as IPersistent<IWeapon>)?.Save();

		protected override AkimboWeapon Instantiate(IItemDataRegistry registry) => new(MainWeaponSave, SideWeaponSave, registry);
	}
}