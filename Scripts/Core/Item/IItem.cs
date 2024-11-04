namespace LandlessSkies.Core;

public interface IItem {
	public IDataKeyProvider KeyProvider { get; }
}

public interface IItem<out T> : IItem where T : IItem<T> {
	IDataKeyProvider IItem.KeyProvider => KeyProvider;
	public new IDataKeyProvider<T> KeyProvider { get; }
}