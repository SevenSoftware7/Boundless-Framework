

using System;
using Godot;


namespace LandlessSkies.Core;


public ref struct LoadableDestructor<TLoadable> where TLoadable : ILoadable {
    private ref TLoadable? loadable;
    private Loadable3D.LoadedUnloadedEventHandler? onLoadUnload;
    private Action? onBeforeUnload;
    private Action? onAfterUnload;



    internal LoadableDestructor(ref TLoadable? loadable) {
        this.loadable = ref loadable;
    }



    public LoadableDestructor<TLoadable> OnLoadUnload(Loadable3D.LoadedUnloadedEventHandler onLoadUnload) =>
        this with {onLoadUnload = onLoadUnload};

    public LoadableDestructor<TLoadable> BeforeUnload(Action onBeforeUnload) =>
        this with {onBeforeUnload = onBeforeUnload};

    public LoadableDestructor<TLoadable> AfterUnload(Action onAfterUnload) =>
        this with {onAfterUnload = onAfterUnload};


    public readonly void Execute() {
        if ( loadable is not null ) {
            onBeforeUnload?.Invoke();

            loadable.LoadUnloadEvent -= onLoadUnload;
            loadable.UnloadModel();

            onAfterUnload?.Invoke();

            loadable.Destroy();
            loadable = default;
        }
    }
}


public ref struct LoadableUpdater<TLoadable> where TLoadable : ILoadable {
    private ref TLoadable? loadable;
    private Func<TLoadable?>? instantiator;
    private Loadable3D.LoadedUnloadedEventHandler? onLoadUnload;
    private Action? onBeforeLoad;
    private Action? onAfterLoad;
    private LoadableDestructor<TLoadable> destructor;



    internal LoadableUpdater(ref TLoadable? loadable) {
        this.loadable = ref loadable;
        destructor = new(ref loadable);
    }



    public LoadableUpdater<TLoadable> OnLoadUnloadEvent(Loadable3D.LoadedUnloadedEventHandler onLoadUnload) =>
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



    public readonly void Execute() {
        destructor.Execute();

        if ( instantiator is not null && instantiator() is TLoadable instantiated ) {
            loadable = instantiated;

            onBeforeLoad?.Invoke();

            loadable.LoadModel();
            loadable.LoadUnloadEvent += onLoadUnload;

            onAfterLoad?.Invoke();
        }
    }
}

public static class LoadableExtensions {
    public static LoadableDestructor<TLoadable> DestroyLoadable<TLoadable>(ref TLoadable? loadable) where TLoadable : ILoadable =>
        new(ref loadable);

    public static LoadableUpdater<TLoadable> UpdateLoadable<TLoadable>(ref TLoadable? loadable) where TLoadable : ILoadable =>
        new(ref loadable);

}