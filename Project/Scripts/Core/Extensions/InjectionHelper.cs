namespace LandlessSkies.Core;

using Godot;

public static class InjectionHelper {
	public static void PropagateInject<T>(this IInjectionProvider<T> parent) {
		if (parent is not Node nodeParent) return;

		T value = parent.GetInjection();
		foreach (Node child in nodeParent.GetChildren()) {
			child.PropagateInject(value);
		}
	}

	private static void PropagateInject<T>(this Node parent, T value) {
		if (parent is IInjectable<T> injectableParent) injectableParent.Inject(value);
		if (parent is IInjectionBlocker<T>) return;

		foreach (Node child in parent.GetChildren()) {

			T childValue = parent is IInjectionInterceptor<T> injectorParent ? injectorParent.Intercept(child) : value;
			child.PropagateInject(childValue);
		}
	}


	public static void RequestInjection<T>(this IInjectable<T> requester) {
		if (requester is not Node nodeRequester) return;

		requester.Inject(default!);
		nodeRequester.PropagateInject<T>(default!);

		nodeRequester.GetParent().RequestInjection<T>();
	}

	private static void RequestInjection<T>(this Node requestLink) {
		if (requestLink is null) return;
		if (requestLink is not IInjectionProvider<T> provider) {
			requestLink.GetParent().RequestInjection<T>();
			return;
		}

		provider.PropagateInject();
	}
}