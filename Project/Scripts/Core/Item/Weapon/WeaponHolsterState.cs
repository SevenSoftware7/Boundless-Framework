namespace LandlessSkies.Core;

public struct WeaponHolsterState {
	public static readonly WeaponHolsterState Unholstered = new(false);
	public static readonly WeaponHolsterState Holstered = new(true);

	public bool IsHolstered = false;

	private WeaponHolsterState(bool isHolstered) {
		IsHolstered = isHolstered;
	}


	// public static implicit operator bool(WeaponHolsterState state) => state.IsHolstered;
	public static implicit operator WeaponHolsterState(bool isHolstered) => isHolstered ? Holstered : Unholstered;
};