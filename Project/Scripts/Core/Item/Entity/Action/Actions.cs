namespace LandlessSkies.Core;

public static class Actions {
	public static bool CanCancel(this EntityAction? action) => action is null || action.IsCancellable;
}