using Godot;
using Godot.Collections;
using System;


namespace LandlessSkies.Core;

[Tool]
public abstract partial class Model : Loadable3D, IInjectable<Skeleton3D?>, IInjectable<Handedness>, ICustomizable {

	[Export] protected Costume _costume { get; private set; } = null!;
	public abstract Costume Costume { get; }

	public virtual IUIObject UIObject => _costume;
	public virtual ICustomizable[] Children => [];
	public virtual ICustomizationParameter[] Customizations => [];



	protected Model() : base() {}
	public Model(Costume costume) : this() {
		ArgumentNullException.ThrowIfNull(costume);

		_costume = costume;
		Name = $"{nameof(Model)} - {_costume.DisplayName}";
	}



	public virtual void Inject(Skeleton3D? value) {}
	public virtual void Inject(Handedness value) {}
}