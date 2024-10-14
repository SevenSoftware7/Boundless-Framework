namespace LandlessSkies.Core;

using System.Collections.Generic;
using Godot;

public interface IItemData : IUIObject {
	public DataKey Key { get; }

	public object? Instantiate();
	public void Register();
	public void Unregister();
}

public interface IItemData<out T> : IItemData where T : IItem<T> {
	private static readonly Dictionary<string, IItemData<T>> Registry = [];

	public static IItemData<T>? GetData(DataKey? key) => key is not null && Registry.TryGetValue(key.String, out IItemData<T>? data) ? data : null;
	public static void RegisterData(IItemData<T> data) {
		Registry[data.Key.String] = data;
		GD.Print($"Registered {data.Key.String} => {data}");
	}
	public static void UnregisterData(IItemData<T> data) {
		Registry.Remove(data.Key.String);
		GD.Print($"Unregistered {data.Key.String} => {data}");
	}

	void IItemData.Register() => Register();
	public new sealed void Register() => RegisterData(this);

	void IItemData.Unregister() => Unregister();
	public new sealed void Unregister() => UnregisterData(this);

	object? IItemData.Instantiate() => Instantiate();
	public new T Instantiate();
}