// using System;
// using Godot;
// using SevenGame.Utility;


// namespace LandlessSkies.Core;

// [Tool]
// [GlobalClass]
// public partial class IWeaponWrapper : InterfaceWrapper, IInterfaceWrapper<IWeapon> {
//     public override string NodePathHintString => IWeaponInfo.NodeHintString;
//     public override string ResourceHintString => IWeaponInfo.ResourceHintString;

//     public IWeaponWrapper() : base() {} 
//     public IWeaponWrapper(Action? onPathChanged = null) : base(onPathChanged) {} 

//     public IWeapon? Get(Node root) => Get<IWeapon>(root);
//     public void Set(Node root, IWeapon? value) => Set<IWeapon>(root, value);
// }