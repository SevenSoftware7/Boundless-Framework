

using System;
using Godot;


namespace LandlessSkies.Core;


public ref struct LoadableDestructor<TLoadable> where TLoadable : Loadable {
    private ref TLoadable? loadable;
    private Loadable.LoadedUnloadedEventHandler? onLoadUnload;
    private Action? onBeforeUnload;
    private Action? onAfterUnload;
    private Action? onFinished;


    internal LoadableDestructor(ref TLoadable? loadable) {
        this.loadable = ref loadable;
    }


    public LoadableDestructor<TLoadable> OnLoadUnload(Loadable.LoadedUnloadedEventHandler onLoadUnload) =>
        this with {onLoadUnload = onLoadUnload};

    public LoadableDestructor<TLoadable> BeforeUnload(Action onBeforeUnload) =>
        this with {onBeforeUnload = onBeforeUnload};

    public LoadableDestructor<TLoadable> AfterUnload(Action onAfterUnload) =>
        this with {onAfterUnload = onAfterUnload};

    public LoadableDestructor<TLoadable> WhenFinished(Action onFinished) =>
        this with {onFinished = onFinished};


    public readonly void Execute() {
        if ( loadable is not null ) {
            onBeforeUnload?.Invoke();

            loadable.LoadedUnloaded -= onLoadUnload;
            loadable.UnloadModel();

            onAfterUnload?.Invoke();

            loadable.UnparentAndQueueFree();
            loadable = null !;
        }

        onFinished?.Invoke();
    }
}


public ref struct LoadableUpdater<TLoadable> where TLoadable : Loadable {
    private ref TLoadable? loadable;
    private Func<TLoadable?>? instantiator;
    private Loadable.LoadedUnloadedEventHandler? onLoadUnload;
    private Action? onBeforeLoad;
    private Action? onAfterLoad;
    private Action? onFinished;
    private LoadableDestructor<TLoadable> destructor;



    internal LoadableUpdater(ref TLoadable? loadable) {
        this.loadable = ref loadable;
        destructor = new(ref loadable);
    }



    public LoadableUpdater<TLoadable> OnLoadUnloadEvent(Loadable.LoadedUnloadedEventHandler onLoadUnload) =>
        this with {onLoadUnload = onLoadUnload, destructor = destructor.OnLoadUnload(onLoadUnload)};

    public LoadableUpdater<TLoadable> BeforeUnload(Action onBeforeUnload) =>
        this with {destructor = destructor.BeforeUnload(onBeforeUnload)};

    public LoadableUpdater<TLoadable> AfterUnload(Action onAfterUnload) =>
        this with {destructor = destructor.AfterUnload(onAfterUnload)};

    public LoadableUpdater<TLoadable> WithConstructor(Func<TLoadable?> instantiator) =>
        this with {instantiator = instantiator};

    public LoadableUpdater<TLoadable> BeforeLoad(Action onBeforeLoad) =>
        this with {onBeforeLoad = onBeforeLoad};

    public LoadableUpdater<TLoadable> AfterLoad(Action onAfterLoad) =>
        this with {onAfterLoad = onAfterLoad};

    public LoadableUpdater<TLoadable> WhenFinished(Action onFinished) =>
        this with {onFinished = onFinished};


    public readonly void Execute() {
        destructor.Execute();

        if ( instantiator is not null && instantiator() is TLoadable instantiated ) {
            loadable = instantiated;

            onBeforeLoad?.Invoke();

            loadable.LoadModel();
            loadable.LoadedUnloaded += onLoadUnload;

            onAfterLoad?.Invoke();
        }

        onFinished?.Invoke();
    }
}

public static class LoadableExtensions {
    public static LoadableDestructor<TLoadable> DestroyLoadable<TLoadable>(ref TLoadable? loadable) where TLoadable : Loadable =>
        new(ref loadable);

    public static LoadableUpdater<TLoadable> UpdateLoadable<TLoadable>(ref TLoadable? loadable) where TLoadable : Loadable =>
        new(ref loadable);

}