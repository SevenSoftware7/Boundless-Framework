using Godot;
using System;


namespace EndlessSkies.Core;

[Tool]
public partial class Character : Node {

    [Export] public bool IsLoaded { get; private set; }
    [Export] public Node3D Anchor;

    [Export] public Node Model { get; private set; }


    [Export] CharacterData CharacterData {
        get => _characterData;
        set => CallDeferred(MethodName.SetData, value);
    }
    private CharacterData _characterData;



    public Character() : base() {
        Name = nameof(Character);
    }
    public Character(Node3D anchor) : this() {
        Anchor = anchor;
        
        anchor.AddChild(this);
        Owner = anchor.Owner;
    }


    
    private void SetData(CharacterData data) {
        // if ( Engine.GetProcessFrames() != 0 ) {
            UnloadModel();
        // }
        _characterData = data;
        // if ( Engine.GetProcessFrames() != 0 ) {
            LoadModel();
        // }
    }

    private void LoadModel() {
        if ( IsLoaded ) return;
        if ( Anchor == null ) return;
        if ( CharacterData == null ) return;

        Model = CharacterData.BaseCostume.ModelScene.Instantiate();
        Model.Name = nameof(Model);

        Anchor.AddChild(Model);
        Model.Owner = Anchor.Owner;

        IsLoaded = true;
    }

    private void UnloadModel() {
        if ( !IsLoaded ) return;

        Model?.Free();
        Model = null;

        IsLoaded = false;
    }


    public override void _Ready() {
        base._Ready();
    }
    public override void _EnterTree() {
        if ( this.IsInvalidTreeCallback() ) return;
        base._EnterTree();
        // CallDeferred(MethodName.LoadModel);
        LoadModel();
    }
    public override void _ExitTree() {
        if ( this.IsInvalidTreeCallback() ) return;
        base._ExitTree();
        CallDeferred(MethodName.UnloadModel);
        // UnloadModel();
    }
}
