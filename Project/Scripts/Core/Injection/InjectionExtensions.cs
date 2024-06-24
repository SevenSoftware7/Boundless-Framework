namespace LandlessSkies.Core;

using Godot;

public static class InjectionExtensions {
	public static void PropagateInject<T>(this IInjectionProvider<T> parent) {
		if (parent is not Node nodeParent) return;

		T value = parent.GetInjection();
		if (parent is IInjectable<T> injectableParent) injectableParent.Inject(value);

		foreach (Node child in nodeParent.GetChildren()) {
			T childValue = parent is IInjectionInterceptor<T> injectorParent ? injectorParent.Intercept(child, value) : value;
			child.PropagateInject(childValue);
		}
	}

	public static void PropagateInject<T>(this Node parent, T value) {
		if (parent is IInjectable<T> injectableParent) injectableParent.Inject(value);
		IInjectionInterceptor<T>? injectorParent = parent as IInjectionInterceptor<T>;
		IInjectionBlocker<T>? blockerParent = parent as IInjectionBlocker<T>;

		foreach (Node child in parent.GetChildren()) {
			if (blockerParent is not null && blockerParent.ShouldBlock(parent, value)) continue;

			T childValue = injectorParent is not null ? injectorParent.Intercept(child, value) : value;
			child.PropagateInject(childValue);
		}
	}


	public static void RequestInjection<T>(this IInjectable<T> requester) {
		if (requester is not Node nodeRequester) return;

		requester.Inject(default!);
		nodeRequester.PropagateInject<T>(default!);

		nodeRequester.GetParent().RequestInjection<T>();
	}

	private static void RequestInjection<T>(this Node requester) {
		if (requester is null) return;
		if (requester is not IInjectionProvider<T> provider) {
			requester.GetParent().RequestInjection<T>();
			return;
		}

		provider.PropagateInject();
	}
}