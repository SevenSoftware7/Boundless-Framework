using Godot;
using System;


namespace EndlessSkies.Core;

[Tool]
public abstract partial class Model : Node3D {
    [Export] public bool IsLoaded { get; private set; }
    [Export] protected Node3D Parent;



    protected Model() : base() {;}
    public Model(IModelAttachment modelAttachment) : this() {
        Parent = modelAttachment.RootAttachment;

        // Parent.AddChild(this);
        // Owner = Parent.Owner;
    }



    public void LoadModel() {
        if ( IsLoaded ) return;

        if ( !LoadModelImmediate() ) return;

        IsLoaded = true;
    }
    public void UnloadModel() {
        if ( !IsLoaded ) return;

        if ( !UnloadModelImmediate() ) return;

        IsLoaded = false;
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
        if ( this.IsInvalidTreeCallback() ) return;

        GD.Print($"{GetType().Name} : EnterTree");

        #if TOOLS
            CallDeferred(MethodName.LoadModel);
        #else
            LoadModel();
        #endif
    }

    public override void _ExitTree() {
        base._ExitTree();
        if ( this.IsInvalidTreeCallback() ) return;

        GD.Print($"{GetType().Name} : ExitTree");

        UnloadModel();
    }
}
