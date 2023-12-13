using Godot;
using System;


namespace LandlessSkies.Core;

[Tool]
public abstract partial class Loadable3D : Node3D, ILoadable {
    [Export] public bool IsLoaded { 
        get => _isLoaded;
        set {
            if ( this.IsEditorGetSetter() ) {
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




    public event LoadedUnloadedEventHandler LoadUnloadEvent {
        add => LoadedUnloaded += value;
        remove => LoadedUnloaded -= value;
    }
    [Signal] public delegate void LoadedUnloadedEventHandler(bool isLoaded);



    public Loadable3D() : base() {}
    public Loadable3D(Node3D root) : this() {
        root.AddChildAndSetOwner(this, Engine.IsEditorHint());
    }



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

    public virtual void Enable() {
        SetProcess(true);
        Visible = true;
    }

    public virtual void Disable() {
        SetProcess(false);
        Visible = false;
    }

    public virtual void Destroy() {
        this.UnparentAndQueueFree();
    }



    public override void _EnterTree() {
        base._EnterTree();
        if ( this.IsEditorEnterTree() ) return;

        LoadModel();
    }

    public override void _ExitTree() {
        base._ExitTree();
        if ( this.IsEditorExitTree() ) return;

        UnloadModel();
    }
}
