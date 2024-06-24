namespace LandlessSkies.Core;

using Godot;

public static class InjectionExtensions {
	public static void PropagateInject<T>(this IInjectionProvider<T> parent) {
		if (parent is not Node nodeParent) return;

		nodeParent.PropagateInject(parent.GetInjection(), true);
	}

	public static void PropagateInject<T>(this Node parent, T value, bool ignoreParentBlocker = false) {
		if (parent is IInjectable<T> injectableParent) injectableParent.Inject(value);

		IInjectionInterceptor<T>? interceptorParent = parent as IInjectionInterceptor<T>;
		IInjectionBlocker<T>? blockerParent = parent as IInjectionBlocker<T>;

		foreach (Node child in parent.GetChildren()) {
			if (! ignoreParentBlocker && blockerParent is not null && blockerParent.ShouldBlock(parent, value)) continue;

			T childValue = interceptorParent is not null ? interceptorParent.Intercept(child, value) : value;
			child.PropagateInject(childValue);
		}
	}


	public static void RequestInjection<T>(this IInjectable<T> requester) {
		if (requester is not Node nodeRequester) return;

		requester.Inject(default!);
		nodeRequester.PropagateInject<T>(default!);

		nodeRequester.GetParent()?.RequestInjection<T>();
	}

	private static void RequestInjection<T>(this Node requester) {
		if (requester is not IInjectionProvider<T> provider) {
			requester.GetParent().RequestInjection<T>();
			return;
		}

		provider.PropagateInject();
	}
}