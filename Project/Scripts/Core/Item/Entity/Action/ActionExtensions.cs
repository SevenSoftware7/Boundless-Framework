namespace LandlessSkies.Core;

public static class ActionExtensions {
	public static bool CanCancel(EntityAction? action) => action is null || action.IsCancellable;
}