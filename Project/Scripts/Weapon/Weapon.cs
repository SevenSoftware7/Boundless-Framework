using System;
using Godot;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class Weapon : Loadable3D, IWeapon {

    [Export] public WeaponData Data {
        get => _data;
        private set => _data ??= value;
    }
    private WeaponData _data;
    
    [Export] public IWeapon.Handedness WeaponHandedness { get; set; }
    [Export] public IWeapon.Type WeaponType {
        get => Data?.Type ?? 0;
        set {}
    }
    


    [ExportGroup("Costume")]
    [Export] private WeaponModel? WeaponModel;

    [Export] public WeaponCostume? Costume {
        get => WeaponModel?.Costume;
        set => SetCostume(value);
    }


    [ExportGroup("Dependencies")]
    [Export]
    public Skeleton3D? Skeleton { 
        get => _skeleton;
        private set => Inject(value);
    }
    private Skeleton3D? _skeleton;
    

    [Signal] public delegate void CostumeChangedEventHandler(WeaponCostume? newCostume, WeaponCostume? oldCostume);



    public Weapon() : base() {
        _data ??= null !;
    }
    public Weapon(WeaponData data, WeaponCostume? costume, Node3D root) : base(root) {        
        ArgumentNullException.ThrowIfNull(data);
        
        _data = data;
        SetCostume(costume);
    }


    public void SetCostume(WeaponCostume? costume) {
        if ( this.IsEditorGetSetter() ) return;
        
        WeaponCostume? oldCostume = Costume;
        if ( costume == oldCostume ) return;

        LoadableExtensions.UpdateLoadable(ref WeaponModel)
            .WithConstructor(() => costume?.Instantiate(this))
            .BeforeLoad(() => WeaponModel!.Inject(Skeleton))
            .Execute();

        EmitSignal(SignalName.CostumeChanged, costume!, oldCostume!);
    }


    public void Inject(Skeleton3D? skeleton) {
        _skeleton = skeleton;
        if ( this.IsEditorGetSetter() ) {
            return;
        }
        WeaponModel?.Inject(skeleton); 
        ReloadModel();
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
