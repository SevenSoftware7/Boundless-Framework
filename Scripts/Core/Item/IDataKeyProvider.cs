namespace LandlessSkies.Core;

public interface IDataKeyProvider {
	string Key { get; }
}

public interface IDataKeyProvider<out T> : IDataKeyProvider where T : IItem<T> {
	public IItemData<T>? GetData() => IItemData<T>.GetData(this);
}