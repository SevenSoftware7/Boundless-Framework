using Godot;
using Godot.Collections;
using System;


namespace LandlessSkies.Core;

[Tool]
public abstract partial class CharacterModel : Loadable3D, IInjectable<Skeleton3D?>, ICustomizable {
	
	[Export] public CharacterCostume Costume { get; private set; } = null!;

	public virtual IUIObject UIObject => Costume;
	public virtual ICustomizable[] Children => [];
	public virtual ICustomizationParameter[] Customizations => [];



	protected CharacterModel() : base() {}
	public CharacterModel(CharacterCostume costume) : this() {
		ArgumentNullException.ThrowIfNull(costume);
		
		Costume = costume;
		Name = $"{nameof(CharacterModel)} - {Costume.DisplayName}";
	}
	
	
	
	public virtual void Inject(Skeleton3D? value) {}
}