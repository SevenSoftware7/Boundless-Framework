using Godot;
using System;


namespace LandlessSkies.Core;

[Tool]
public abstract partial class WeaponModel : Loadable {
    
    [Export] public WeaponCostume Costume { 
        get => _costume;
        private set => _costume ??= value;
    }
    private WeaponCostume _costume;



    protected WeaponModel() : base() {
        _costume ??= null !;

        Name = nameof(WeaponModel);
    }
    public WeaponModel(WeaponCostume costume, Node3D root) : base(root) {
        ArgumentNullException.ThrowIfNull(costume);

        _costume = costume;

        Name = nameof(WeaponModel);
    }

}
