using System;
using Godot;

// using UnityEngine;

// using Object = UnityEngine.Object;


namespace LandlessSkies.Core;

// [Tool]
// public partial class InterfaceReference : Resource {
//     [Export] private Node _reference;
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
//     [Export] private Node _reference;
//     private T _value;

//     public T Value {
//         get {
//             if (_value == null && _reference != null) {
//                 _value = _reference as T;
//             }
//             return _value;
//         }
//         set {
//             _value = value;

//             // TODO?: add other checks for other Serialization methods
//             if (value is not Node obj) {
//                 GD.PushWarning($"{typeof(T).Name} is not an Exportable Object; it will not be serialized and will be lost on reload.");
//                 return;
//             }

//             _reference = obj;
//         }
//     }


//     public bool IsAssigned => _value != null || _reference != null;
// }