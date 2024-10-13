namespace LandlessSkies.Core;

public interface IInstantiable {
	public IItemData? Data { get; }
}

public interface IItem<out T> : IInstantiable where T : IItem<T> {
	IItemData? IInstantiable.Data => Data;
	public new IItemData<T>? Data { get; }
}