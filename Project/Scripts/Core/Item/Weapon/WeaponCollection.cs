namespace LandlessSkies.Core;

using System.Collections.Generic;
using Godot;

[Tool]
[GlobalClass]
public abstract partial class WeaponCollection : Node, IWeapon, ISerializationListener {
	public virtual WeaponType Type => Weapon?.Type ?? 0;
	public virtual WeaponUsage Usage => Weapon?.Usage ?? 0;
	public virtual WeaponSize Size => Weapon?.Size ?? 0;

	public abstract int Style { get; set; }
	public abstract int StyleCount { get; }

	public virtual string DisplayName => Weapon?.DisplayName ?? string.Empty;
	public virtual Texture2D? DisplayPortrait => Weapon?.DisplayPortrait;

	protected abstract IWeapon? Weapon { get; }


	protected abstract void UpdateWeapons();


	public virtual IEnumerable<AttackBuilder> GetAttacks(Entity target) => [];

	public virtual List<ICustomization> GetCustomizations() => [];
	public virtual List<ICustomizable> GetSubCustomizables() => [];

	public abstract ISaveData<IWeapon> Save();


	public override void _Ready() {
		base._Ready();
		UpdateWeapons();
	}
	public override void _Notification(int what) {
		base._Notification(what);
		if (!IsNodeReady()) return;

		switch ((ulong)what) {
			case NotificationChildOrderChanged:
				UpdateWeapons();
				break;
		}
	}

	public void OnBeforeSerialize() { }
	public void OnAfterDeserialize() {
		UpdateWeapons();
	}
}