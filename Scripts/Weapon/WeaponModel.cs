using Godot;
using System;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public abstract partial class WeaponModel : Model {
    
    [Export] public WeaponCostume Costume { 
        get => _costume;
        private set {;}
    }
    private WeaponCostume _costume;



    protected WeaponModel() : base() {
        _costume ??= null !;

        Name = nameof(WeaponModel);
    }
    public WeaponModel(Node3D? root, Skeleton3D? skeleton, WeaponCostume costume) : base(root) {
        ArgumentNullException.ThrowIfNull(costume);

        _costume = costume;

        Name = nameof(WeaponModel);
    }

}
