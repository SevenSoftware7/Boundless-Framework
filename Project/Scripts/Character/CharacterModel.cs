using Godot;
using Godot.Collections;
using System;


namespace LandlessSkies.Core;

[Tool]
public abstract partial class CharacterModel : Loadable3D, IInjectable<Skeleton3D?>, ICustomizable {
	private CharacterCostume _costume = null!;


	
	[Export] public CharacterCostume Costume { 
		get => _costume;
		private set => _costume ??= value;
	}

	public virtual IUIObject UIObject => Costume;
	public virtual ICustomizable[] Children => [];
	public virtual ICustomizationParameter[] Customizations => [];



	protected CharacterModel() : base() {}
	public CharacterModel(CharacterCostume costume, Node3D root) : this() {
		ArgumentNullException.ThrowIfNull(costume);
		root.AddChildAndSetOwner(this);
		
		_costume = costume;
	}
	
	
	
	public virtual void Inject(Skeleton3D? value) {}

	
	
	public override void _ValidateProperty(Dictionary property) {
		base._ValidateProperty(property);
		
		StringName name = property["name"].AsStringName();
		
		if ( name == PropertyName.Costume && Costume is not null ) {
			property["usage"] = (int)(property["usage"].As<PropertyUsageFlags>() | PropertyUsageFlags.ReadOnly);
		}
	}
}