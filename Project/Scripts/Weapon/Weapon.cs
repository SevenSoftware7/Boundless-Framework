using System;
using System.Collections.Generic;
using Godot;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public abstract partial class Weapon : Loadable3D, IWeapon, IInjectable<Entity?> {

    [Export]
    public abstract WeaponData Data { get; protected set; }
    
    [Export]
    public abstract IWeapon.Handedness WeaponHandedness { get; set; }

    [Export]
    public IWeapon.Type WeaponType {
        get => Data?.Type ?? 0;
        set {}
    }
    


    [ExportGroup("Costume")]
    [Export]
    public abstract WeaponCostume? Costume { get; set; }
    

    [Signal] public delegate void CostumeChangedEventHandler(WeaponCostume? newCostume, WeaponCostume? oldCostume);


    public Weapon() : base() {}
    public Weapon(WeaponData data, WeaponCostume? costume, Node3D root) : base(root) {        
        ArgumentNullException.ThrowIfNull(data);
        
        Data = data;
        SetCostume(costume);
    }



    public abstract IEnumerable<IAttack.AttackInfo> GetAttacks(Entity target);


    public virtual void Inject(Entity? owner) {}

    public abstract void SetCostume(WeaponCostume? costume);


}
