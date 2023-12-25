// using System;
// using Godot;
// using Godot.Collections;


// namespace SevenGame.Utility;

// public abstract partial class InterfaceWrapper : Resource {
// 	public abstract string NodePathHintString { get; }
// 	public abstract string ResourceHintString { get; }

// 	public event Action? ValueChanged;

// 	private NodePath Path {
// 		get => _path;
// 		set {
// 			_path = value;
// 			ValueChanged?.Invoke();
// 		}
// 	}
// 	private NodePath _path = new();

// 	private Resource? Resource {
// 		get => _resource;
// 		set {
// 			_resource = value;
// 			ValueChanged?.Invoke();
// 		}
// 	}
// 	private Resource? _resource = null;

// 	[Export]
// 	private bool IsResource {
// 		get => _isResource;
// 		set {
// 			_isResource = value;
// 			NotifyPropertyListChanged();
// 		}
// 	}
// 	private bool _isResource = false;



// 	public InterfaceWrapper(Action? onPathChanged = null) : base() {
// 		ValueChanged = onPathChanged;
// 	}


// 	public void SetChangeCallback(Action callback) {
// 		ValueChanged = callback;
// 	}



// 	protected T? Get<T>(Node root) where T : class {
// 		if (IsResource) {
// 			return Resource as T;
// 		}
// 		return root.GetNodeOrNull(Path) as T;
// 	}

// 	protected void Set<T>(Node root, T? val) where T : class {
// 		if (val is null) {
// 			Path = "";
// 			Resource = null;
// 			return;
// 		}

// 		if ( val is Resource resource ) {
// 			Resource = resource;
// 			IsResource = true;
// 		}

// 		if ( val is Node node ) {
// 			Path = root.GetPathTo(node);
// 			IsResource = false;
// 		}
// 	}


// 	public override Array<Dictionary> _GetPropertyList() {
// 		if ( IsResource ) {
// 			return [new Dictionary() {
// 				{ "name", PropertyName.Resource },
// 				{ "type", (int)Variant.Type.Object },
// 				{ "hint", (int)PropertyHint.ResourceType },
// 				{ "hint_string", ResourceHintString },
// 				{ "usage", (int)PropertyUsageFlags.Default },
// 			}];
// 		}
// 		return [new Dictionary() {
// 			{ "name", PropertyName.Path },
// 			{ "type", (int)Variant.Type.NodePath },
// 			{ "hint", (int)PropertyHint.NodePathValidTypes },
// 			{ "hint_string", NodePathHintString },
// 			{ "usage", (int)PropertyUsageFlags.Default },
// 		}];
// 	}

// 	public override Variant _Get(StringName property) {
// 		if (property == PropertyName.Resource) {
// 			return Resource!;
// 		}
// 		if (property == PropertyName.Path) {
// 			return Path;
// 		}
// 		return base._Get(property);
// 	}

// 	public override bool _Set(StringName property, Variant value) {
// 		if (property == PropertyName.Resource) {
// 			if ( value.VariantType == Variant.Type.Object && value.As<Resource>() is Resource resource) {
// 				Resource = resource;
// 				return true;
// 			}
// 		} else if (property == PropertyName.Path) {
// 			if ( value.VariantType == Variant.Type.NodePath && value.As<NodePath>() is NodePath nodePath) {
// 				Path = nodePath;
// 				return true;
// 			}
// 			return false;
// 		}
// 		return base._Set(property, value);
// 	}
// }