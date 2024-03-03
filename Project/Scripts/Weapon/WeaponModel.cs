using Godot;
using Godot.Collections;
using System;


namespace LandlessSkies.Core;

[Tool]
public abstract partial class WeaponModel : Loadable3D, IInjectable<Skeleton3D?>, IInjectable<IWeapon.Handedness>, ICustomizable {
	[Export] public WeaponCostume Costume { get; private set; } = null!;

	public virtual IUIObject UIObject => Costume;
	public virtual ICustomizable[] Children => [];
	public virtual ICustomizationParameter[] Customizations => [];



	protected WeaponModel() : base() {}
	public WeaponModel(WeaponCostume costume) : this() {
		ArgumentNullException.ThrowIfNull(costume);

		Costume = costume;
		Name = $"{nameof(WeaponModel)} - {Costume.DisplayName}";
	}



	public virtual void Inject(Skeleton3D? value) {}
	public abstract void Inject(IWeapon.Handedness value);
}