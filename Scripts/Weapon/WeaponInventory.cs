// #undef TOOLS

using System;
using System.Linq;
using Godot;
using Godot.Collections;


namespace LandlessSkies.Core;


[Tool]
[GlobalClass]
public partial class WeaponInventory : Model {

    [Export(PropertyHint.NodePathValidTypes, nameof(Skeleton3D))] public NodePath SkeletonPath { 
        get => _skeletonPath;
        set {
            _skeletonPath = value;
            
            bool isValidPath = this.TryGetNode(SkeletonPath, out Skeleton3D armature);

            for ( int i = 0; i < _weapons?.Count; i++ ) {
                Weapon weapon = _weapons[i];
                if ( weapon is null ) continue;

                weapon.SkeletonPath = isValidPath ? weapon.GetPathTo(armature) : null;
            }
        }
    }
    private NodePath _skeletonPath;


    [Export] private Array<Weapon> _weapons { get; set; } = new Array<Weapon>();


#if TOOLS
    [Export] private Array<WeaponData> WeaponDatas {
        get {
            if (_weapons.Count != 0 && _weaponDatas.Count < _weapons.Count) {
                _weaponDatas = new(_weapons.Select(w => w?.Data));
            }
            return _weaponDatas;
        }
        set {
            this.CallDeferredIfTools(Callable.From(UpdateWeaponDatas));

            void UpdateWeaponDatas() {

                if ( _weaponDatas is not null && value is not null && _weaponDatas.RecursiveEqual(value) ) return;

                if ( _weaponDatas is null ) {
                    _weaponDatas = new();
                    for ( int i = 0; i < value.Count; i++ ) {
                        SetWeapon(i, value[i]);
                    }
                }

                if ( value == null || value.Count == 0 ) {
                    for ( int i = 0; i < _weapons.Count; i++ ) {
                        _weapons[i].QueueFree();
                    }
                    _weapons?.Clear();
                    _weaponDatas?.Clear();
                    NotifyPropertyListChanged();
                    return;
                }


                Array<WeaponData> current = WeaponDatas;
                int maxCount = Math.Max(current.Count, value.Count);
                for ( int i = 0; i < maxCount; i++ ) {
                    if ( i >= value.Count ) {
                        RemoveWeapon(i);
                    } else if ( i >= current.Count || current[i] != value[i] ) {
                        SetWeapon(i, value[i]);
                    }
                }
                NotifyPropertyListChanged();
            }
        }
    }
    private Array<WeaponData> _weaponDatas { get; set; } = null;
#endif



    private WeaponInventory() : base() {;}

    public WeaponInventory(Node3D root, Skeleton3D skeleton) : base(root) {
        SkeletonPath = GetPathTo(skeleton);

        Name = nameof(WeaponInventory);
    }



    public void SetWeapon(int index, WeaponData data, WeaponCostume costume = null) {
        if ( this.IsInvalidTreeCallback() ) return;

    #if TOOLS
        if ( _weaponDatas.Count > index ) {
            _weaponDatas.RemoveAt(index);
        }
        _weaponDatas.Insert(index, data);
    #endif

        Weapon weapon = _weapons.Count > index ? _weapons[index] : null;
        if ( weapon is not null && weapon.Data != data ) {
            weapon.UnloadModel();
            weapon.QueueFree();
            _weapons.RemoveAt(index);
        }

        if ( data == null ) return;

        // if ( ! this.TryGetNode(SkeletonPath, out Skeleton3D armature) ) return;

        Skeleton3D skeleton = SkeletonPath is null ? null : GetNodeOrNull<Skeleton3D>(SkeletonPath);
        _weapons.Insert(index, data?.Instantiate(this, skeleton));
        _weapons[index]?.LoadModel();

        SetCostume(index, costume ?? data?.BaseCostume);
    }

    public void RemoveWeapon(int index) {
        Weapon weapon = _weapons.Count > index ? _weapons[index] : null;
        
    #if TOOLS
        _weaponDatas.RemoveAt(index);
    #endif

        if ( weapon is not null ) {
            weapon.UnloadModel();
            weapon.QueueFree();
            _weapons.RemoveAt(index);
        }
    }

    public void SetCostume(int index, WeaponCostume costume) {
        try {
            _weapons[index]?.SetCostume(costume);
        } catch {

        }
    }

    public override void ReloadModel(bool forceLoad = false) {
        for ( int i = 0; i < _weapons.Count; i++ ) {
            Weapon weapon = _weapons[i];
            if ( weapon is null ) continue;

            weapon.ReloadModel(forceLoad);
        }
    }

    protected override bool LoadModelImmediate() {
        for ( int i = 0; i < _weapons.Count; i++ ) {
            Weapon weapon = _weapons[i];
            if ( weapon is null ) continue;

            weapon.LoadModel();
        }
        return true;
    }

    protected override bool UnloadModelImmediate() {
        for ( int i = 0; i < _weapons.Count; i++ ) {
            Weapon weapon = _weapons[i];
            if ( weapon is null ) continue;

            weapon.UnloadModel();
        }
        return true;
    }

}