using Godot;

namespace LandlessSkies.Core;

[Tool]
public abstract partial class Loadable3D : ExtendedNode3D, ILoadable {
	private bool _isLoaded = false;



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

	public event LoadedUnloadedEventHandler LoadUnloadEvent {
		add => LoadedUnloaded += value;
		remove => LoadedUnloaded -= value;
	}



	[Signal] public delegate void LoadedUnloadedEventHandler(bool isLoaded);



	protected Loadable3D() : base() {
		Name = GetType().Name;
	}
	public Loadable3D(Node3D root) : this() {
		root.AddChildAndSetOwner(this, Engine.IsEditorHint());
	}



	public bool LoadModel() {
		if ( IsLoaded ) return false;

		if ( ! LoadModelImmediate() ) return false;

		_isLoaded = true;
		EmitSignal(SignalName.LoadedUnloaded, true);
		return true;
	}
	public bool UnloadModel() {
		if ( ! IsLoaded ) return false;

		if ( ! UnloadModelImmediate() ) return false;

		_isLoaded = false;
		EmitSignal(SignalName.LoadedUnloaded, false);
		return true;
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
		UnloadModel();
		this.UnparentAndQueueFree();
	}



	public override void _Ready() {
		base._Ready();
		LoadModel();
	}

	// public override void _ExitTree() {
	// 	base._ExitTree();
	// 	UnloadModel();
	// }
}