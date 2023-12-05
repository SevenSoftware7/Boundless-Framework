using Godot;
using System;


namespace LandlessSkies.Core;

[Tool]
public abstract partial class CharacterModel : Loadable3D, IInjectable<Skeleton3D?> {

    
    [Export] public CharacterCostume Costume { 
        get => _costume;
        private set => _costume ??= value;
    }
    private CharacterCostume _costume;



    protected CharacterModel() : base() {
        _costume ??= null !;
        
        Name = nameof(CharacterModel);
    }
    public CharacterModel(CharacterCostume costume, Node3D root) : base(root) {
        ArgumentNullException.ThrowIfNull(costume);
        
        _costume = costume;
        
        Name = nameof(CharacterModel);
    }
    
    public virtual void Inject(Skeleton3D? value) {}
}
