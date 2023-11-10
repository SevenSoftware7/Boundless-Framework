// #undef TOOLS

using System;
using System.Collections;
using System.Linq;
using Godot;
using Godot.Collections;


namespace LandlessSkies.Core;


[Tool]
[GlobalClass]
public partial class WeaponInventory : Loadable, IWeapon {

    [Export(PropertyHint.NodePathValidTypes, nameof(Skeleton3D))] public NodePath SkeletonPath { 
        get => _skeletonPath;
        set => SetSkeleton(GetNodeOrNull<Skeleton3D>(value));
    }
    private NodePath _skeletonPath = new();

    [Export] private uint CurrentIndex {
        get => _currentIndex;
        set {
            if ( ! IsNodeReady() ) {
                _currentIndex = value;
                return;
            }

            SwitchTo(value);
        }
    }
    private uint _currentIndex = 0;

    [Export] private Weapon? CurrentWeapon {
        get => IndexInBounds(CurrentIndex) ? _weapons[(int)CurrentIndex] : null;
        set {
            if (value is not null && _weapons.Contains(value)) {
                CurrentIndex = (uint)_weapons.IndexOf(value);
            }
        }
    }

    [Export] public IWeapon.Handedness WeaponHandedness {
        get => IndexInBounds(CurrentIndex) ? _weapons[(int)CurrentIndex].WeaponHandedness : IWeapon.Handedness.Right;
        set {
            if ( CurrentWeapon is Weapon currentWeapon ) {
                currentWeapon.WeaponHandedness = value;
            }
        }
    }
    

    [Export] private Array<Weapon> _weapons = new();


#if TOOLS
    [ExportGroup("Set Weapons")]
    [Export] private Array<WeaponData> WeaponDatas {
        get {
            if (_weaponDatas is null || (_weapons.Count != 0 && _weaponDatas.Count < _weapons.Count)) {
                _weaponDatas = new();
                for ( int i = 0; i < _weapons.Count; i++ ) {
                    Weapon weapon = _weapons[i];
                    if ( weapon is not null && weapon.Data is not null )

                    _weaponDatas.Add(weapon.Data);
                }
            }
            return _weaponDatas;
        }
        set {
            if ( ! IsNodeReady() ) return;
            Callable.From( UpdateWeaponDatas ).CallDeferred();

            void UpdateWeaponDatas() {

                if ( _weaponDatas is not null && value is not null && _weaponDatas.RecursiveEqual(value) ) return;

                if ( _weaponDatas is null && value is not null ) {
                    _weaponDatas = new();
                    for ( int i = 0; i < value.Count; i++ ) {
                        SetWeapon(i, value[i]);
                    }
                }

                if ( value is null || value.Count == 0 ) {
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
    private Array<WeaponData>? _weaponDatas = null;
    // [ExportGroup("")]

#endif



    private WeaponInventory() : base() {
        Name = nameof(WeaponInventory);
    }

    public WeaponInventory(Skeleton3D skeleton) : base() {
        SetSkeleton(skeleton);
    }



    private bool IndexInBounds(uint index) {
        return index < _weapons.Count;
    }

    public override void SetSkeleton(Skeleton3D? skeleton) {
        _skeletonPath = skeleton is not null ? GetPathTo(skeleton) : new();
        for ( int i = 0; i < _weapons?.Count; i++ ) {
            if ( _weapons[i] is Weapon weapon ) {
                weapon.SetSkeleton(skeleton);
            }
        }
    }

    public void SwitchTo(uint index) {
        if ( _weapons.Count == 0 ) {
            _currentIndex = 0;
            return;
        }

        uint maxCount = (uint)(_weapons.Count - 1);
        _currentIndex = index > maxCount ? maxCount : index;

        for (int i = 0; i < _weapons.Count; i++) {
            if ( _weapons[i] is Weapon weapon ) {
                weapon.SetProcess(false);
                weapon.Visible = false;
            }
        }

        if ( _weapons[(int)index] is Weapon currWeapon ) {
            currWeapon.SetProcess(true);
            currWeapon.Visible = true;
        }
    }

    public void SetWeapon(int index, WeaponData? data, WeaponCostume? costume = null) {
        if ( ! IsNodeReady() ) return;
        Weapon? weapon = _weapons.Count > index ? _weapons[index] : null;

        if ( weapon?.Data == data ) return;

        this.UpdateLoadable<Weapon, WeaponData>()
            .WithConstructor(() => data?.Instantiate(costume).SetOwnerAndParentTo(this))
            .AfterUnload((_) => _weapons.RemoveAt(index))
            .BeforeLoad((weapon) => weapon.SetSkeleton(GetNodeOrNull<Skeleton3D>(SkeletonPath)))
            .AfterLoad((weapon) => _weapons.Insert(index, weapon))
            .Execute(ref weapon);

    #if TOOLS
        Array<WeaponData> datas = WeaponDatas;
        if ( index < datas.Count ) {
            datas.RemoveAt(index);
        }
        datas.Insert(index, data!);
    #endif
    }

    public void RemoveWeapon(int index) {
    #if TOOLS
        WeaponDatas.RemoveAt(index);
    #endif
        
        Weapon? weapon = _weapons.Count > index ? _weapons[index] : null;
        this.DestroyLoadable<Weapon>()
            .AfterUnload((_) => _weapons.RemoveAt(index))
            .Execute(ref weapon);
    }

    public void SetCostume(int index, WeaponCostume? costume) {
        _weapons[index]?.SetCostume(costume);
    }

    public override void ReloadModel(bool forceLoad = false) {
        for ( int i = 0; i < _weapons.Count; i++ ) {
            if ( _weapons[i] is Weapon weapon ) {
                weapon.ReloadModel(forceLoad);
            }
        }
    }

    protected override bool LoadModelImmediate() {
        for ( int i = 0; i < _weapons.Count; i++ ) {
            if ( _weapons[i] is Weapon weapon ) {
                weapon.LoadModel();
            }
        }
        return true;
    }

    protected override bool UnloadModelImmediate() {
        for ( int i = 0; i < _weapons.Count; i++ ) {
            if ( _weapons[i] is Weapon weapon ) {
                weapon.UnloadModel();
            }
        }
        return true;
    }

}