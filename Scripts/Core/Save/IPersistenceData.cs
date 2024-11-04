namespace LandlessSkies.Core;

public interface IPersistenceData {
	object? Load();
}

public interface IPersistenceData<out T> : IPersistenceData where T : class {
	object? IPersistenceData.Load() => Load();
	public new T? Load();
}

public abstract class PersistenceData<T>(T item) : IPersistenceData<T> where T : class {
	public CustomizationData? Customization = item is ICustomizable customizable ? CustomizationData.GetFrom(customizable) : null;
	public IPersistenceData<Costume>? CostumeData = item is ICostumable costumable ? costumable.CostumeHolder?.Costume?.Save() : null;

	public T? Load() {
		T? instance = Instantiate();

		if (instance is not null) {
			if (instance is ICostumable costumable && CostumeData is not null) {
				costumable.CostumeHolder?.SetCostume(CostumeData);
			}
			if (instance is ICustomizable customizable && Customization is not null) {
				Customization.ApplyTo(customizable);
			}
			LoadInternal(instance);
		}

		return instance;
	}

	protected abstract T Instantiate();
	protected virtual void LoadInternal(T item) { }
}

public class ItemPersistenceData<T>(T item) : PersistenceData<T>(item) where T : class, IItem<T> {
	private readonly IDataKeyProvider<T> DataKey = item.KeyProvider;
	public IItemData<T> Data => IItemData<T>.GetData(DataKey) ?? throw new System.InvalidOperationException();

	protected sealed override T Instantiate() => Data.Instantiate();
}