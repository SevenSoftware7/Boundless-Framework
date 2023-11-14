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
        _data ??= null !;

        Name = nameof(Weapon);
    }
    public Weapon(WeaponData data, WeaponCostume? costume, Node3D root) : base(root) {        
        ArgumentNullException.ThrowIfNull(data);
        
        _data = data;
        SetCostume(costume);

        Name = nameof(Weapon);
    }



    public override void SetSkeleton(Skeleton3D? skeleton) {
        _skeletonPath = skeleton is not null ? GetPathTo(skeleton) : new();
        WeaponModel?.SetSkeleton(skeleton);
        ReloadModel();
    }


    public void SetCostume(WeaponCostume? costume) {
        if ( this.IsEditorGetSetter() ) return;
        
        WeaponCostume? oldCostume = WeaponCostume;
        if ( costume == oldCostume ) return;

        LoadableExtensions.UpdateLoadable(ref WeaponModel)
            .WithConstructor(() => costume?.Instantiate(this))
            .BeforeLoad(() => WeaponModel?.SetSkeleton(GetNodeOrNull<Skeleton3D>(SkeletonPath)))
            .WhenFinished(() => EmitSignal(SignalName.CostumeChanged, costume!, oldCostume!))
            .Execute();
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
