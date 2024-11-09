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

	public virtual WeaponType Type => CurrentWeapon?.Type ?? 0;
	public virtual WeaponUsage Usage => CurrentWeapon?.Usage ?? 0;
	public virtual WeaponSize Size => CurrentWeapon?.Size ?? 0;

	public abstract uint Style { get; set; }
	public abstract uint MaxStyle { get; }

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