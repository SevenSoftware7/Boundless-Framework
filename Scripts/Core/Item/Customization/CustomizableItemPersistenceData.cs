using SevenDev.Boundless.Persistence;

namespace LandlessSkies.Core;

public class CustomizableItemPersistenceData<T>(T item) : ItemPersistenceData<T>(item) where T : class, IItem, ICustomizable {
	public readonly CustomizationData Customization = CustomizationData.GetFrom(item);

	protected override void LoadInternal(T item, IItemDataProvider registry) {
		base.LoadInternal(item, registry);

		Customization.ApplyTo(item);
	}
}