using Godot;
using System;


namespace LandlessSkies.Core;

[Tool]
public abstract partial class Loadable : Node3D, ILoadable {
    [Export] public bool IsLoaded { 
        get => _isLoaded;
        set {
            if ( ! IsNodeReady() ) {
                _isLoaded = value;
                return;
            }
            
            if ( value ) {
                LoadModel();
            } else {
                UnloadModel();
            }
        }
    }
    private bool _isLoaded = false;


    [Signal] public delegate void LoadedUnloadedEventHandler(bool isLoaded);



    protected Loadable() : base() {;}


    public virtual void SetSkeleton(Skeleton3D? skeleton) {;}

    public void LoadModel() {
        if ( IsLoaded ) return;

        if ( ! LoadModelImmediate() ) return;

        _isLoaded = true;
        EmitSignal(SignalName.LoadedUnloaded, true);
    }
    
    public void UnloadModel() {
        if ( ! IsLoaded ) return;

        if ( ! UnloadModelImmediate() ) return;

        _isLoaded = false;
        EmitSignal(SignalName.LoadedUnloaded, false);
    }

    public virtual void ReloadModel(bool forceLoad = false) {
        bool wasLoaded = IsLoaded;
        UnloadModel();

        if ( wasLoaded || forceLoad ) {
            LoadModel();
        }
    }

    /// <summary>
    /// Loads the model immediately, without checking if it's already loaded.
    /// </summary>
    /// <returns>
    /// Returns true if the model was loaded, false if it wasn't.
    /// </returns>
    protected abstract bool LoadModelImmediate();

    /// <summary>
    /// Unloads the model immediately, without checking if it's already unloaded.
    /// </summary>
    /// <returns>
    /// Returns true if the model was unloaded, false if it wasn't.
    /// </returns>
    protected abstract bool UnloadModelImmediate();



    public override void _EnterTree() {
        base._EnterTree();
        if ( this.IsInvalidEnterTree() ) return;

#if TOOLS
        Callable.From(LoadModel).CallDeferred();
#else
        LoadModel();
#endif
    }

    public override void _ExitTree() {
        base._ExitTree();
        if ( this.IsInvalidExitTree() ) return;

        UnloadModel();
    }
}
