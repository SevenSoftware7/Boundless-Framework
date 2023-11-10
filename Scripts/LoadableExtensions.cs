

using System;
using Godot;


namespace LandlessSkies.Core;


public record struct LoadableDestructor<TLoadable> where TLoadable : Loadable {
    private readonly Node caller;
    private Loadable.LoadedUnloadedEventHandler? onLoadUnload;
    private Action<TLoadable>? onBeforeUnload;
    private Action<TLoadable>? onAfterUnload;
    private Action<TLoadable?>? onFinished;



    internal LoadableDestructor(Node caller) {
        this.caller = caller;
    }



    public LoadableDestructor<TLoadable> OnLoadUnload(Loadable.LoadedUnloadedEventHandler onLoadUnload) =>
        this with {onLoadUnload = onLoadUnload};

    public LoadableDestructor<TLoadable> BeforeUnload(Action<TLoadable> onBeforeUnload) =>
        this with {onBeforeUnload = onBeforeUnload};

    public LoadableDestructor<TLoadable> AfterUnload(Action<TLoadable> onAfterUnload) =>
        this with {onAfterUnload = onAfterUnload};

    public LoadableDestructor<TLoadable> WhenFinished(Action<TLoadable?> onFinished) =>
        this with {onFinished = onFinished};


    public readonly void Execute(ref TLoadable? loadable) {
        if ( ! caller.IsNodeReady() ) return;

        if ( loadable is not null ) {
            onBeforeUnload?.Invoke(loadable);

            loadable.LoadedUnloaded -= onLoadUnload;
            loadable.UnloadModel();

            onAfterUnload?.Invoke(loadable);

            loadable.UnparentAndQueueFree();
            loadable = null !;
        }

        onFinished?.Invoke(loadable);
    }
}


public record struct LoadableUpdater<TLoadable, TProvider> where TLoadable : Loadable where TProvider : Resource {

    private readonly Node caller;
    private Func<TLoadable?>? instantiator;
    private Loadable.LoadedUnloadedEventHandler? onLoadUnload;
    private Action<TLoadable>? onBeforeLoad;
    private Action<TLoadable>? onAfterLoad;
    private Action<TLoadable?>? onFinished;
    private LoadableDestructor<TLoadable> destructor;



    internal LoadableUpdater(Node caller) {
        this.caller = caller;
        destructor = new(caller);
    }



    public LoadableUpdater<TLoadable, TProvider> OnLoadUnloadEvent(Loadable.LoadedUnloadedEventHandler onLoadUnload) =>
        this with {onLoadUnload = onLoadUnload, destructor = destructor.OnLoadUnload(onLoadUnload)};

    public LoadableUpdater<TLoadable, TProvider> BeforeUnload(Action<TLoadable> onBeforeUnload) =>
        this with {destructor = destructor.BeforeUnload(onBeforeUnload)};

    public LoadableUpdater<TLoadable, TProvider> AfterUnload(Action<TLoadable> onAfterUnload) =>
        this with {destructor = destructor.AfterUnload(onAfterUnload)};

    public LoadableUpdater<TLoadable, TProvider> WithConstructor(Func<TLoadable?> instantiator) =>
        this with {instantiator = instantiator};

    public LoadableUpdater<TLoadable, TProvider> BeforeLoad(Action<TLoadable> onBeforeLoad) =>
        this with {onBeforeLoad = onBeforeLoad};

    public LoadableUpdater<TLoadable, TProvider> AfterLoad(Action<TLoadable> onAfterLoad) =>
        this with {onAfterLoad = onAfterLoad};

    public LoadableUpdater<TLoadable, TProvider> WhenFinished(Action<TLoadable?> onFinished) =>
        this with {onFinished = onFinished};


    public readonly void Execute(ref TLoadable? loadable) {
        if ( ! caller.IsNodeReady() ) return;

        destructor.Execute(ref loadable);

        if ( instantiator is not null && instantiator() is TLoadable instantiated ) {
            loadable = instantiated;

            onBeforeLoad?.Invoke(loadable);

            loadable.LoadModel();
            loadable.LoadedUnloaded += onLoadUnload;

            onAfterLoad?.Invoke(loadable);
        }

        onFinished?.Invoke(loadable);
    }
}

public static class LoadableExtensions {
    public static LoadableDestructor<TLoadable> DestroyLoadable<TLoadable>(this Node caller) where TLoadable : Loadable =>
        new(caller);

    public static LoadableUpdater<TLoadable, TProvider> UpdateLoadable<TLoadable, TProvider>(this Node caller) where TLoadable : Loadable where TProvider : Resource =>
        new(caller);
}