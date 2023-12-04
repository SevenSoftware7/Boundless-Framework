using System;
using Godot;

// using UnityEngine;

// using Object = UnityEngine.Object;


namespace LandlessSkies.Core;

// [Tool]
// public partial class InterfaceReference : Resource {
//     [Export] private Node? _reference;
//     private object _value;

//     public object Value {
//         get {
//             if (_value == null && _reference != null) {
//                 _value = _reference;
//             }
//             return _value;
//         }
//         set {
//             _value = value;

//             // TODO?: add other checks for other Serialization methods
//             if (value is not Node obj) {
//                 GD.PushWarning($"{_value.GetType().Name} is not an Exportable Object; it will not be serialized and will be lost on reload.");
//                 return;
//             }

//             _reference = obj;
//         }
//     }
// }

// public partial class InterfaceReference<T> : Resource where T : class {

//     [Export] public Node? ObjReference {
//         get => _reference;
//         private set {
//             if ( value is null ) {
//                 Value = null;
//             } else if ( value is T t ) {
//                 Value = t;
//             } else {
//                 GD.PushWarning($"Reference ({value?.Name ?? "Null"}) is not assignable to Class {typeof(T).Name}");
//             }
//         }
//     }
//     private Node? _reference;

//     public T? Value {
//         get {
//             if (_value is null && _reference is T t) {
//                 _value = t;
//             }
//             return _value;
//         }
//         set {
//             if ( value is null ) {
//                 _reference = null;
//             } else if ( value is Node obj ) {
//                 _reference = obj;
//             } else {
//                 GD.PushWarning($"{typeof(T).Name} is not an Exportable Object; it will not be serialized and will be lost on reload.");
//                 return;
//             }

//             _value = value;
//         }
//     }
//     private T? _value;


//     public bool IsAssigned => _value != null || _reference != null;
// }