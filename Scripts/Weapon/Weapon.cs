using System;
using Godot;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class Weapon : Loadable, IWeapon {

    [Export(PropertyHint.NodePathValidTypes, nameof(Skeleton3D))] public NodePath SkeletonPath { 
        get => _skeletonPath;
        set => SetSkeleton(GetNodeOrNull<Skeleton3D>(value));
    }
    private NodePath _skeletonPath = new();

    [Export] private WeaponModel? WeaponModel;


    [Export] public WeaponData Data { 
        get => _data;
        private set => _data ??= value;
    }
    private WeaponData _data;

    [Export] public WeaponCostume? WeaponCostume {
        get => WeaponModel?.Costume;
        private set => SetCostume(value);
    }
    [Export] public IWeapon.Handedness WeaponHandedness { get; set; }
    

    [Signal] public delegate void CostumeChangedEventHandler(WeaponCostume? newCostume, WeaponCostume? oldCostume);



    public Weapon() : base() {
        _data ??= null !;

        Name = nameof(Weapon);
    }
    public Weapon(WeaponData data, WeaponCostume? costume) : base() {        
        ArgumentNullException.ThrowIfNull(data);
        
        _data = data;
        SetCostume(costume);

        Name = nameof(Weapon);
    }



    public override void SetSkeleton(Skeleton3D? skeleton) {
        SkeletonPath = skeleton is not null ? GetPathTo(skeleton) : new();
        WeaponModel?.SetSkeleton(skeleton);
        ReloadModel();
    }


    public void SetCostume(WeaponCostume? costume) {
        if ( costume == WeaponCostume ) return;

#if TOOLS
        Callable.From(SetCostume).CallDeferred();
        void SetCostume() =>
#endif
        this.UpdateLoadable<WeaponModel, WeaponCostume>()
            .WithConstructor(() => costume?.Instantiate().SetOwnerAndParentTo(this))
            .BeforeLoad((model) => model.SetSkeleton(GetNodeOrNull<Skeleton3D>(SkeletonPath)))
            .WhenFinished((_) => EmitSignal(SignalName.CostumeChanged, costume!, WeaponCostume!))
            .Execute(ref WeaponModel);
    }



    protected override bool LoadModelImmediate() {
        WeaponModel?.LoadModel();

        return true;
    }

    protected override bool UnloadModelImmediate() {
        WeaponModel?.UnloadModel();

        return true;
    }


}
