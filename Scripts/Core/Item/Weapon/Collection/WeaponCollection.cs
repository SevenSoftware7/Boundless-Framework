namespace LandlessSkies.Core;

using System.Collections.Generic;
using Godot;
using SevenDev.Boundless.Persistence;

[Tool]
[GlobalClass]
public abstract partial class WeaponCollection : Node, IWeapon, ISerializationListener {
	public virtual string DisplayName => CurrentWeapon?.DisplayName ?? "";
	public virtual Texture2D? DisplayPortrait => CurrentWeapon?.DisplayPortrait;

	protected bool _lockBackingList = false;

	public virtual IWeapon.WeaponKind Kind => CurrentWeapon?.Kind ?? 0;
	public virtual IWeapon.WeaponUsage Usage => CurrentWeapon?.Usage ?? 0;
	public virtual IWeapon.WeaponSize Size => CurrentWeapon?.Size ?? 0;

	public abstract StyleState Style { get; }
	public abstract StyleState MaxStyle { get; }

	public abstract IWeapon? CurrentWeapon { get; }

	protected abstract void UpdateWeapons();


	public virtual IEnumerable<Action.Wrapper> GetAttacks(Entity target) => [];

	public virtual Dictionary<string, ICustomization> GetCustomizations() => [];
	public virtual IEnumerable<IUIObject> GetSubObjects() => [];


	public override void _Ready() {
		base._Ready();
		UpdateWeapons();
	}
	public override void _Notification(int what) {
		base._Notification(what);
		if (!IsNodeReady()) return;

		switch ((ulong)what) {
			case NotificationChildOrderChanged:
				if (!_lockBackingList) {
					UpdateWeapons();
				}
				break;
		}
	}

	public void OnBeforeSerialize() { }
	public void OnAfterDeserialize() {
		UpdateWeapons();
	}

	public abstract IPersistenceData<WeaponCollection> Save();
}