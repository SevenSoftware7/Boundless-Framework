namespace LandlessSkies.Core;

public interface IItem {
	public DataKey Key { get; }
}

public interface IItem<out T> : IItem where T : IItem<T>;