

using Godot;

namespace LandlessSkies.Core;

public interface ILoadable : IDestroyable, IEnablable {

	bool IsLoaded { get; set; }



	bool LoadModel();

	bool UnloadModel();

	void ReloadModel(bool forceLoad = false);
}