

using Godot;

namespace LandlessSkies.Core;

public interface ILoadable : IDestroyable, IEnablable {
    
    bool IsLoaded { get; set; }

    event Loadable3D.LoadedUnloadedEventHandler LoadUnloadEvent;

    
    void LoadModel();
    
    void UnloadModel();

    void ReloadModel(bool forceLoad = false);
}