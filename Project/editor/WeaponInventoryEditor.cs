#if TOOLS

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

namespace LandlessSkies.Core;


public partial class WeaponInventory {
    [Export] private Array<WeaponData> WeaponDatas {
        get => [.. _weaponDatas];
        set {
            if ( this.IsEditorGetSetter() ) {
                _weaponDatas = [.. value];
                return;
            }
            if (_weaponDatas.SequenceEqual(value)) return;

            int minLength = Math.Min(value.Count, _weaponDatas.Count);
            for (int i = 0; i < Math.Max(value.Count, _weaponDatas.Count); i++) {
                if (i < minLength) {
                    if (_weaponDatas[i] != value[i]) {
                        SetWeapon(i, value[i]);
                    }
                } else if (i < value.Count) {
                    AddWeapon(value[i]);
                } else {
                    RemoveWeapon(i);
                }
            }
            NotifyPropertyListChanged();
        }
    }
    private List<WeaponData> _weaponDatas = [];



    private void ResetData() {
        _weaponDatas = [.. _weapons.Select(weapon => weapon?.Data!)];
        NotifyPropertyListChanged();
    }


    public override void _Ready() {
        base._Ready();
        
        ResetData();
	}

}

#endif