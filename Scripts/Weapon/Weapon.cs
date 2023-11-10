using System;
using Godot;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class Weapon : Loadable, IWeapon {

    [Export] public WeaponData Data { 
        get => _data;
        private set => _data ??= value;
    }
    private WeaponData _data;
    
    [Export] public IWeapon.Handedness WeaponHandedness { get; set; }


    [ExportGroup("Costume")]
    [Export] private WeaponModel? WeaponModel;

    [Export] public WeaponCostume? WeaponCostume {
        get => WeaponModel?.Costume;
        private set => SetCostume(value);
    }


    [ExportGroup("Dependencies")]
    [Export(PropertyHint.NodePathValidTypes, nameof(Skeleton3D))] public NodePath SkeletonPath { 
        get => _skeletonPath;
        set => SetSkeleton(GetNodeOrNull<Skeleton3D>(value));
    }
    private NodePath _skeletonPath = new();
    

    [Signal] public delegate void CostumeChangedEventHandler(WeaponCostume? newCostume, WeaponCostume? oldCostume);



    public Weapon() : base() {
        Data ??= null !;

        Name = nameof(Weapon);
    }
    public Weapon(WeaponData data, WeaponCostume? costume) : base() {        
        ArgumentNullException.ThrowIfNull(data);
        
        Data = data;
        SetCostume(costume);

        Name = nameof(Weapon);
    }



    public override void SetSkeleton(Skeleton3D? skeleton) {
        _skeletonPath = skeleton is not null ? GetPathTo(skeleton) : new();
        WeaponModel?.SetSkeleton(skeleton);
        ReloadModel();
    }


    public void SetCostume(WeaponCostume? costume) {
        WeaponCostume? oldCostume = WeaponCostume;
        if ( costume == oldCostume ) return;

#if TOOLS
        Callable.From(SetCostume).CallDeferred();
        void SetCostume() =>
#endif
        this.UpdateLoadable<WeaponModel, WeaponCostume>()
            .WithConstructor(() => costume?.Instantiate().SetOwnerAndParentTo(this))
            .BeforeLoad((model) => model.SetSkeleton(GetNodeOrNull<Skeleton3D>(SkeletonPath)))
            .WhenFinished((_) => EmitSignal(SignalName.CostumeChanged, costume!, oldCostume!))
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
