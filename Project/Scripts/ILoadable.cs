

using Godot;

namespace LandlessSkies.Core;

public interface ILoadable : IDestroyable, IEnablable {

	bool IsLoaded { get; set; }

	public sealed void SetLoaded(bool loaded) {
		IsLoaded = loaded;
	}
	public sealed bool LoadModel() {
		if ( IsLoaded ) return false;

		if ( ! LoadModelBehaviour() ) return false;
		return true;
	}
	public sealed void UnloadModel() {
		if ( ! IsLoaded ) return;

		UnloadModelBehaviour();
	}

	public sealed void LoadUnload(bool loaded) {
		if (loaded) {
			LoadModel();
		} else {
			UnloadModel();
		}
	}
	
	/// <summary>
	/// Reloads the model by unloading it then loading it back again.
	/// </summary>
	virtual void ReloadModel(bool forceLoad = false) {
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
	protected bool LoadModelBehaviour() => true;

	/// <summary>
	/// Unloads the model immediately, without checking if it's already unloaded.
	/// </summary>
	protected void UnloadModelBehaviour() {}
}