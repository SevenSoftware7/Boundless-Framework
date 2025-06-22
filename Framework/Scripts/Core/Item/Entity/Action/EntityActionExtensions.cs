namespace SevenDev.Boundless;

public static class EntityActionExtensions {
	public static bool CanCancel(this EntityAction? action) => action is null || action.IsCancellable;
}