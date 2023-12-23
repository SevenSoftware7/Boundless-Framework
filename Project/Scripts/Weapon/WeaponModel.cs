using Godot;
using System;


namespace LandlessSkies.Core;

[Tool]
public abstract partial class WeaponModel : Loadable3D, IInjectable<Skeleton3D?>, IInjectable<IWeapon.Handedness> {

	private WeaponCostume _costume = null!;


	[Export] public WeaponCostume Costume {
		get => _costume;
		private set => _costume ??= value;
	}



	protected WeaponModel() : base() {
		Name = nameof(WeaponModel);
	}
	public WeaponModel(WeaponCostume costume, Node3D root) : base(root) {
		ArgumentNullException.ThrowIfNull(costume);

		_costume = costume;

		Name = nameof(WeaponModel);
	}
	
	

	public virtual void Inject(Skeleton3D? value) {}
	public abstract void Inject(IWeapon.Handedness value);

}
