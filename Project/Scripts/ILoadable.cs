

using Godot;

namespace LandlessSkies.Core;

public interface ILoadable : IInjectable<Skeleton3D?> {
    
    bool IsLoaded { get; set; }


    
    void LoadModel();
    
    void UnloadModel();

    void ReloadModel(bool forceLoad = false);
}