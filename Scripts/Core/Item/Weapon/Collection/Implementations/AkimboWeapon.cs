namespace Seven.Boundless;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Seven.Boundless.Utility;
using Seven.Boundless.Injection;
using Seven.Boundless.Injection.Godot;
using Seven.Boundless.Persistence;


[Tool]
[GlobalClass]
public sealed partial class AkimboWeapon : CompositeWeapon, IInjectionInterceptor<Handedness>, IInjectionBlocker<StyleState>, IInjectionInterceptor<StyleState>, IPersistent<AkimboWeapon> {
	public IInjectionNode InjectionNode { get; }

	public IWeapon? MainWeapon {
		get;
		private set {
			field = value;
			if (value is Node mainWeaponNode) mainWeaponNode.SafeReparentTo(this);
		}
	}

	public IWeapon? SideWeapon {
		get;
		private set {
			field = value;
			if (value is Node sideWeaponNode) sideWeaponNode.SafeReparentTo(this);
		}
	}


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

	Handedness IInjectionInterceptor<Handedness>.Intercept(IInjectionNode child, Handedness value) {
		if (child.UnderlyingObject == SideWeapon) return value.Reverse();
		return value;
	}

	public override IWeapon? CurrentWeapon => MainWeapon;


	private AkimboWeapon() : base() {
		InjectionNode = new GodotNodeInjectionNode(this);
	}
	public AkimboWeapon(IWeapon? mainWeapon, IWeapon? sideWeapon) : this() {
		MainWeapon = mainWeapon;
		SideWeapon = sideWeapon;
	}
	public AkimboWeapon(IPersistenceData<IWeapon>? mainWeaponSave, IPersistenceData<IWeapon>? sideWeaponSave, IItemDataProvider registry) : this(mainWeaponSave?.Load(registry), sideWeaponSave?.Load(registry)) { }


	public override IEnumerable<IWeapon> GetWeapons() {
		if (MainWeapon is not null) yield return MainWeapon;
		if (SideWeapon is not null) yield return SideWeapon;
	}

	protected override void _RefreshWeapons() {
		IWeapon[] weapons = [.. GetChildren().OfType<IWeapon>()];
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

		protected override AkimboWeapon Instantiate(IItemDataProvider registry) => new(MainWeaponSave, SideWeaponSave, registry);
	}
}