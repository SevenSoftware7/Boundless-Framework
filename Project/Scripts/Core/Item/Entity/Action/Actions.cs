namespace LandlessSkies.Core;

public static class Actions {
	public static bool CanCancel(EntityAction? action) => action is null || action.IsCancellable;
}