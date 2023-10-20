using Godot;
using System;


namespace LandlessSkies.Core;

[Tool]
[GlobalClass]
public partial class CharacterModel : Model {

    [Export] private MeshInstance3D Model;
    [Export] public CharacterCostume Costume { get; private set; }
    [Export(PropertyHint.NodePathValidTypes, nameof(Skeleton3D))] public NodePath SkeletonPath { get; private set; }



    private CharacterModel() : base() {;}
    public CharacterModel(Node3D root, Skeleton3D skeleton, CharacterCostume costume) : base(root) {
        Name = nameof(CharacterModel);
        
        Costume = costume;
        SkeletonPath = GetPathTo(skeleton);
    }



    protected override bool LoadModelImmediate() {
        if ( SkeletonPath == null ) return false;
        if ( Costume == null ) return false;

        Model = Costume.ModelScene?.Instantiate() as MeshInstance3D;
        if ( Model is not null ) {
            Model.Name = nameof(Model);

            this.AddChildSetOwner(Model);

            if ( this.TryGetNode(SkeletonPath, out Skeleton3D skeleton) ) {
                Model.Skeleton = Model.GetPathTo(skeleton);
            }
        }

        return true;
    }

    protected override bool UnloadModelImmediate() {
        Model?.QueueFree();
        Model = null;

        return true;
    }

    public override void _Process(double delta) {
        base._Process(delta);

        if ( this.TryGetNode(SkeletonPath, out Skeleton3D skeleton) ) {
            Transform = new(skeleton.Transform.Basis, skeleton.Transform.Origin);
        }
    }
}
