using Godot;
using System;


namespace LandlessSkies.Core;

[Tool]
public abstract partial class Model : Node3D {
    [Export] public bool IsLoaded { 
        get => _isLoaded;
        private set {
            // if ( value ) {
            //     LoadModel();
            // } else {
            //     UnloadModel();
            // }
            _isLoaded = value;
        }
    }
    private bool _isLoaded = false;



    protected Model() : base() {;}
    public Model(Node3D root) : this() {
        root.AddChildSetOwner(this);
    }



    public void LoadModel() {
        if ( IsLoaded ) return;

        if ( ! LoadModelImmediate() ) return;

        _isLoaded = true;
    }
    
    public void UnloadModel() {
        if ( ! IsLoaded ) return;

        if ( ! UnloadModelImmediate() ) return;

        _isLoaded = false;
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
        if ( Engine.GetProcessFrames() == 0 ) return;

        this.CallDeferredIfTools( Callable.From(LoadModel) );
    }

    public override void _ExitTree() {
        base._ExitTree();
        if ( Engine.GetProcessFrames() == 0 ) return;

        UnloadModel();
    }
}
