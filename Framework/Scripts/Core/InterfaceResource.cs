using System;
using Godot;

namespace SevenDev.Boundless;

[Tool]
public partial class InterfaceResource<T> : Resource where T : class {
	private readonly Func<T?, T?> Getter;
	public event Action<T?, T?> OnSet;

	public T? Value {
		get => Getter(_value);
		set => OnSet(_value, value);
	}
	private T? _value;

	[Export] public Resource? Resource {
		get => Value as Resource;
		private set {
			if (value is T interfaceValue) {
				Value = interfaceValue;
			}
		}
	}


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