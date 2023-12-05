#if TOOLS

using System;
using Godot;
using Godot.Collections;

namespace LandlessSkies.Core;


public partial class WeaponInventory {
    [Export] private Array<WeaponData> WeaponDatas {
        get => _weaponDatas;
        set {
            if ( this.IsEditorGetSetter() ) {
                _weaponDatas = value;
                return;
            }

            if ( value is not null && _weaponDatas.RecursiveEqual(value) ) return;


            // Initialize on first addition
            if ( _weaponDatas.Count == 0 && value is not null ) {
                for ( int i = 0; i < value.Count; i++ ) {
                    AddWeapon(value[i]);
                }
                NotifyPropertyListChanged();
                return;
            }

            // Clear
            if ( value is null || value.Count == 0 ) {
                for ( int i = 0; i < _weapons.Count; i++ ) {
                    _weapons[i]?.Get(this)?.Destroy();
                }
                _weapons?.Clear();
                _weaponDatas?.Clear();
                NotifyPropertyListChanged();
                return;
            }

            int minLength = Math.Min(value.Count, _weaponDatas.Count);

            for (int i = 0; i < Math.Max(value.Count, _weaponDatas.Count); i++) {
                if (i < minLength) {
                    if (_weaponDatas[i] != value[i]) {
                        SetWeapon(i, value[i]);
                    }
                } else if (i < value.Count) {
                    AddWeapon(value[i]);
                } else {
                    RemoveWeapon(_weaponDatas.Count - 1);
                }
            }
            NotifyPropertyListChanged();
        }
    }
    private Array<WeaponData> _weaponDatas = [];

}

#endif