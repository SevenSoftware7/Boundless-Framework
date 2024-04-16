using Godot;
using System;

namespace LandlessSkies.Core;

[Tool]
public abstract partial class Model : Loadable3D, ICustomizable {

	[Export] public Costume Costume { get; protected set; } = null!;

	public virtual IUIObject UIObject => Costume;
	public virtual ICustomizable[] Children => [];
	public virtual ICustomizationParameter[] Customizations => [];



	protected Model() : base() { }
	public Model(Costume costume) : this() {
		ArgumentNullException.ThrowIfNull(costume);

		Costume = costume;
		Name = $"{nameof(Model)} - {Costume.DisplayName}";
	}


	public abstract Aabb GetAabb();
}