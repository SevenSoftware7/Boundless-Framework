using System;
using Godot;

namespace LandlessSkies.Core;

[Tool]
public partial class InterfaceResource<T> : Resource where T : class {
	private readonly Func<T?, T?> Getter;
	public event Action<T?, T?> OnSet;

	public T? Value {
		get => Getter(_value);
		set => OnSet(_value, value);
	}
	private T? _value;

	[Export] public StringName? Resource {
		get => _resourcePath ?? string.Empty;
		private set {
			_resourcePath = value;

			try {
				T? resource = ResourceLoader.Load<T>(_resourcePath);
				if (resource is T interfaceValue) {
					Value = interfaceValue;
					GD.Print($"Found Resource Value : {interfaceValue}. Resource Path : {_resourcePath}");
				}
			}
			catch (Exception e) {
				GD.PushError($"Error loading resource: {e.Message}. Resource Path : {_resourcePath}");
			}
		}
	}
	private StringName? _resourcePath;


	public InterfaceResource() : this(null, null) { }
	public InterfaceResource(Func<T?, T?>? getter = null, Action<T?, T?>? onSet = null) {
		Getter = value => value;
		if (getter is not null) {
			Getter += getter;
		}

		OnSet = (oldValue, value) => _value = value;
		if (onSet is not null) {
			OnSet += onSet;
		}
	}
}