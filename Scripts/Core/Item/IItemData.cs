namespace LandlessSkies.Core;

using System.Collections.Generic;
using Godot;

public interface IItemData : IUIObject {
	public string Key { get; }

	public object? Instantiate();
}

public interface IItemData<out T> : IItemData where T : IItem<T> {
	private static readonly Dictionary<string, IItemData<T>> Registry = [];

	public static IItemData<T>? GetData(string key) => Registry.TryGetValue(key.ToSnakeCase(), out IItemData<T>? data) ? data : null;
	public static void RegisterData(IItemData<T> data) => Registry[data.Key.ToSnakeCase()] = data;

	object? IItemData.Instantiate() => Instantiate();
	public new T Instantiate();
}