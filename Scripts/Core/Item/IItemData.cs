namespace LandlessSkies.Core;

using System.Collections.Generic;
using Godot;

public interface IItemData : IUIObject {
	protected static readonly Dictionary<string, IItemData> Registry = [];
	public IDataKeyProvider KeyProvider { get; }

	public static IItemData? GetData(IDataKeyProvider? keyProvider) => keyProvider is not null && Registry.TryGetValue(keyProvider.Key, out IItemData? data) ? data : null;
	public static bool RegisterData(IItemData data, bool overwrite = false) {
		if (Registry.ContainsKey(data.KeyProvider.Key) && !overwrite) {
			GD.PrintErr($"Data with key {data.KeyProvider.Key} already exists.");
			return false;
		}
		Registry[data.KeyProvider.Key] = data;
		GD.Print($"Registered {data.KeyProvider.Key} => {data}");
		return true;
	}

	public static void UnregisterData(IItemData data) {
		Registry.Remove(data.KeyProvider.Key);
		GD.Print($"Unregistered {data.KeyProvider.Key} => {data}");
	}


	public void Register() => RegisterData(this);
	public void Unregister() => UnregisterData(this);

	public object? Instantiate();
}

public interface IItemData<out T> : IItemData where T : IItem<T> {
	private static readonly Dictionary<string, IItemData<T>> TypedRegistry = [];

	IDataKeyProvider IItemData.KeyProvider => KeyProvider;
	public new IDataKeyProvider<T> KeyProvider { get; }


	public static IItemData<T>? GetData(IDataKeyProvider<T>? keyProvider) => keyProvider is not null && TypedRegistry.TryGetValue(keyProvider.Key, out IItemData<T>? data) ? data : null;
	public static bool RegisterData(IItemData<T> data, bool overwrite = false) {
		if (!IItemData.RegisterData(data, overwrite)) return false;

		if (TypedRegistry.ContainsKey(data.KeyProvider.Key) && !overwrite) {
			GD.PrintErr($"Data with key {data.KeyProvider.Key} (type {typeof(T)}) already exists.");
			return false;
		}
		TypedRegistry[data.KeyProvider.Key] = data;
		GD.Print($"Registered {data.KeyProvider.Key} => {data} (type {typeof(T)})");
		return true;
	}
	public static void UnregisterData(IItemData<T> data) {
		IItemData.UnregisterData(data);

		TypedRegistry.Remove(data.KeyProvider.Key);
		GD.Print($"Unregistered {data.KeyProvider.Key} => {data} (type {typeof(T)})");
	}

	void IItemData.Register() => Register();
	public new sealed void Register() => RegisterData(this);

	void IItemData.Unregister() => Unregister();
	public new sealed void Unregister() => UnregisterData(this);

	object? IItemData.Instantiate() => Instantiate();
	public new T Instantiate();
}