using Godot;
using SevenGame.Utility;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class Weapon : Model {

    [Export(PropertyHint.NodePathValidTypes, nameof(Skeleton3D))] public NodePath SkeletonPath { get; set; } = new();
    [Export] public WeaponModel? WeaponModel { get; private set; }


    [Export] public WeaponData Data { 
        get => _data;
        private set => _data ??= value;
    }
    private WeaponData _data;

    [Export] public WeaponCostume? WeaponCostume {
        get => _weaponCostume ??= WeaponModel?.Costume;
        private set => this.CallDeferredIfTools( Callable.From( () => SetCostume(value) ) );
    }
    private WeaponCostume? _weaponCostume;



    public Weapon() : base() {
        _data ??= null !;

        Name = nameof(Weapon);
    }
    public Weapon(Node3D root, Skeleton3D skeleton, [NotNull]WeaponData data) : base(root) {        
        _data = data;
        SkeletonPath = skeleton is not null ? GetPathTo(skeleton) : new();

        SetCostume(data.BaseCostume);

        Name = nameof(Weapon);
    }



    public void SetCostume(WeaponCostume? costume) {
        if ( this.IsInvalidTreeCallback() ) return;
        if ( _weaponCostume == costume ) return;

        WeaponModel?.QueueFree();
        WeaponModel = null;

        _weaponCostume = costume;

        ReloadModel();
    }



    protected override bool LoadModelImmediate() {
        if ( WeaponCostume is null ) return false;
        if ( Data is null ) return false;

        if ( ! this.TryGetNode(SkeletonPath, out Skeleton3D armature) ) return false;

        WeaponModel = WeaponCostume.Instantiate(this, armature);
        WeaponModel?.LoadModel();

        return true;
    }

    protected override bool UnloadModelImmediate() {
        WeaponModel?.QueueFree();
        WeaponModel = null;

        return true;
    }


}
