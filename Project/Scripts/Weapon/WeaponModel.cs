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
	public WeaponModel(WeaponCostume costume, Node3D root) : this() {
		ArgumentNullException.ThrowIfNull(costume);
		root.AddChildAndSetOwner(this);

		Costume = costume;
	}
	
	

	public virtual void Inject(Skeleton3D? value) {}
	public abstract void Inject(IWeapon.Handedness value);


	
	public override void _ValidateProperty(Dictionary property) {
		base._ValidateProperty(property);

		StringName name = property["name"].AsStringName();
		
		if (name == PropertyName.Costume && Costume is not null) {
			property["usage"] = (int)(property["usage"].As<PropertyUsageFlags>() | PropertyUsageFlags.ReadOnly);
		
		}
	}
}