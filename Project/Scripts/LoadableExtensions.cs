using System;

namespace LandlessSkies.Core;

public ref struct LoadableDestructor<TLoadable> where TLoadable : ILoadable {
	private ref TLoadable? loadable;
	private Action<TLoadable>? onBeforeUnload;
	private Action<TLoadable>? onAfterUnload;



	internal LoadableDestructor(ref TLoadable? loadable) {
		this.loadable = ref loadable;
	}



	public LoadableDestructor<TLoadable> BeforeUnload(Action<TLoadable> onBeforeUnload) =>
		this with { onBeforeUnload = this.onBeforeUnload + onBeforeUnload };

	public LoadableDestructor<TLoadable> AfterUnload(Action<TLoadable> onAfterUnload) =>
		this with { onAfterUnload = this.onAfterUnload + onAfterUnload };


	public readonly void Execute() {
		if (loadable is null)
			return;

		onBeforeUnload?.Invoke(loadable);

		loadable.Unload();

		onAfterUnload?.Invoke(loadable);

		loadable.Destroy();
		loadable = default;
	}
}


public ref struct LoadableUpdater<TLoadable> where TLoadable : ILoadable {
	private ref TLoadable? loadable;
	private readonly Func<TLoadable?>? instantiator;
	private Action<TLoadable>? onBeforeLoad;
	private Action<TLoadable>? onAfterLoad;
	private LoadableDestructor<TLoadable> destructor;



	internal LoadableUpdater(ref TLoadable? loadable, Func<TLoadable?>? instantiator = null) {
		this.loadable = ref loadable;
		this.instantiator = instantiator;
		destructor = new(ref loadable);
	}



	public LoadableUpdater<TLoadable> BeforeUnload(Action<TLoadable> onBeforeUnload) =>
		this with { destructor = destructor.BeforeUnload(onBeforeUnload) };

	public LoadableUpdater<TLoadable> AfterUnload(Action<TLoadable> onAfterUnload) =>
		this with { destructor = destructor.AfterUnload(onAfterUnload) };

	public LoadableUpdater<TLoadable> BeforeLoad(Action<TLoadable> onBeforeLoad) =>
		this with { onBeforeLoad = this.onBeforeLoad + onBeforeLoad };

	public LoadableUpdater<TLoadable> AfterLoad(Action<TLoadable> onAfterLoad) =>
		this with { onAfterLoad = this.onAfterLoad + onAfterLoad };



	public readonly void Execute() {
		destructor.Execute();
		if (instantiator is not null && instantiator() is TLoadable instantiated) {
			loadable = instantiated;
		}

		if (loadable is null)
			return;

		onBeforeLoad?.Invoke(loadable);

		loadable.Load();

		onAfterLoad?.Invoke(loadable);
	}
}