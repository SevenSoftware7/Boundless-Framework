using Godot;
using SevenGame.Utility;
using System;
using System.Linq;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class Weapon : Model {

    [Export] public WeaponData Data { get; private set; }
    [Export(PropertyHint.NodePathValidTypes, nameof(Skeleton3D))] public NodePath SkeletonPath { get; set; }
    [Export] public WeaponModel WeaponModel { get; private set; }


    public Basis WeaponRotation { get; private set; } = Basis.Identity;


    [Export] public WeaponCostume WeaponCostume {
        get => _weaponCostume;
        private set => this.CallDeferredIfTools( Callable.From( () => SetCostume(value) ) );
    }
    private WeaponCostume _weaponCostume;



    public Weapon() : base() {;}
    public Weapon(Node3D root, Skeleton3D skeleton, WeaponData data) : base(root) {
        Name = nameof(Weapon);
        
        Data = data;
        SkeletonPath = GetPathTo(skeleton);
    }



    public void SetCostume(WeaponCostume costume) {
        if ( this.IsInvalidTreeCallback() ) return;
        if ( _weaponCostume == costume ) return;

        WeaponModel?.QueueFree();
        WeaponModel = null;

        _weaponCostume = costume;

        ReloadModel();
    }



    protected override bool LoadModelImmediate() {
        // if ( Parent is null ) return false;
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
