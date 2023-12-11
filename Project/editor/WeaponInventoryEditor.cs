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
            if (this.IsEditorGetSetter()) {
                _weaponDatas = [.. value];
                return;
            }
            if (_weaponDatas.SequenceEqual(value)) return;

            UpdateWeaponDatas(value);
        }
    }
    private List<WeaponData> _weaponDatas = [];



    private void UpdateWeaponDatas(IList<WeaponData> value) {
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

    private void ResetData() {
        _weaponDatas = [.. _weapons.Select(weapon => weapon?.Get(this)?.Data!)];
        NotifyPropertyListChanged();
        ConnectEvents();
    }

    private void ConnectEvents() {
        for (int i = 0; i < _weapons.Count; i++) {
            _weapons[i].SetChangeCallback(ResetData);
        }
    }


    public override void _Ready() {
        base._Ready();
        
        ResetData();
	}

    public override void _Notification(int what) {
        base._Notification(what);
        if (what == NotificationWMWindowFocusIn) {
            // NotificationWMWindowFocusIn is also called on Rebuilding the project;
            // Reconnect to signal on Recompile
            ConnectEvents();
        }
    }

}

#endif