namespace SevenDev.Boundless;

using System.Collections.Generic;
using System.Linq;
using Godot;
using SevenDev.Boundless.Persistence;

[Tool]
[GlobalClass]
public abstract partial class CompositeWeapon : Node, IWeapon, ISerializationListener {
	protected bool _listLock = false;


	public virtual string DisplayName => CurrentWeapon?.DisplayName ?? "";
	public virtual Texture2D? DisplayPortrait => CurrentWeapon?.DisplayPortrait;

	public virtual WeaponKind Kind => CurrentWeapon?.Kind ?? 0;
	public virtual WeaponUsage Usage => CurrentWeapon?.Usage ?? 0;
	public virtual WeaponSize Size => CurrentWeapon?.Size ?? 0;

	public abstract StyleState Style { get; }
	public abstract StyleState MaxStyle { get; }

	public abstract IWeapon? CurrentWeapon { get; }

	private void RefreshWeapons() {
		if (_listLock) return;
		_listLock = true;
		_RefreshWeapons();
		_listLock = false;
	}
	protected abstract void _RefreshWeapons();


	public abstract IEnumerable<IWeapon> GetWeapons();
	public virtual IEnumerable<EntityAction.Wrapper> GetAttacks(Entity target) => GetWeapons().SelectMany(w => w.GetAttacks(target));
	public virtual Dictionary<string, ICustomization> GetCustomizations() => [];
	public virtual IEnumerable<IUIObject> GetSubObjects() => GetWeapons();



	public override void _Ready() {
		base._Ready();
		RefreshWeapons();
	}
	public override void _Notification(int what) {
		base._Notification(what);
		if (!IsNodeReady()) return;

		switch ((ulong)what) {
			case NotificationChildOrderChanged:
				RefreshWeapons();
				break;
		}
	}

	public void OnBeforeSerialize() { }
	public void OnAfterDeserialize() {
		RefreshWeapons();
	}

	public abstract IPersistenceData<CompositeWeapon> Save();
}