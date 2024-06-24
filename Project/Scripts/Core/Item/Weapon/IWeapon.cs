namespace LandlessSkies.Core;

using System.Collections.Generic;
using Godot;


public interface IWeapon : ICustomizable, ISaveable<IWeapon>, IInjectable<WeaponHolsterState> {
	WeaponHolsterState HolsterState { get; set; }

	WeaponType Type { get; }
	WeaponUsage Usage { get; }
	WeaponSize Size { get; }

	public abstract int Style { get; set; }
	public virtual int StyleCount => 1;


	void IInjectable<WeaponHolsterState>.Inject(WeaponHolsterState value) => HolsterState = value;

	IEnumerable<AttackBuilder> GetAttacks(Entity target);
}