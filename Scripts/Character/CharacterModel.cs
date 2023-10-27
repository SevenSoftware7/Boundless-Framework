using Godot;
using System;


namespace LandlessSkies.Core;

[Tool]
// [GlobalClass]
public abstract partial class CharacterModel : Model {

    
    [Export] public CharacterCostume Costume { 
        get => _costume;
        private set {;}
    }
    private CharacterCostume _costume;



    protected CharacterModel() : base() {
        _costume ??= null !;
        
        Name = nameof(CharacterModel);
    }
    public CharacterModel(Node3D? root, Skeleton3D? skeleton, CharacterCostume costume) : base(root) {
        ArgumentNullException.ThrowIfNull(costume);
        
        _costume = costume;
        
        Name = nameof(CharacterModel);
    }
}
