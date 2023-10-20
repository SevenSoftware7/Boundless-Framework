using Godot;


namespace LandlessSkies.Core;

public static class EngineUtils {

    /// <summary>
    /// Returns true if the Node is Ready but the Project is still loading.
    /// This can be used to avoid calling _EnterTree and _ExitTree after _Ready on Project load.
    /// </summary>
    /// <example>Return out of an "redundant" _EnterTree call:
    /// <code>
    /// public override void _EnterTree() {
    ///    if ( this.IsInvalidTreeCallback() ) return;
    ///    base._EnterTree();
    /// }
    /// </code>
    /// </example>
    /// <param name="node"></param>
    /// <returns></returns>
    public static bool IsInvalidTreeCallback(this Node node) => 
        node.IsNodeReady() && Engine.GetProcessFrames() == 0;

    public static void AddChildSetOwner(this Node obj, Node child) {
        obj.AddChildSetOwner(child);
    }

    public static T GetNodeByTypeName<T>(this Node obj) where T : Node {
        return obj.GetNodeOrNull<T>(typeof(T).Name);
    }

    public static T GetOrCreateNode<T>(this Node obj, ref T node, string name) where T : Node, new() {
        node ??= obj.GetNodeOrNull<T>(name);

        if ( node is null ) {
            node = new() {
                Name = name
            };
            obj.AddChildSetOwner(node);
        }

        return node;
    }

    public static bool TryGetNode<T>(this Node obj, NodePath nodePath, out T node) where T : Node {
        if ( nodePath is null ) {
            node = null;
            return false;
        }
        node = obj.GetNodeOrNull<T>(nodePath);
        return node is not null;

    }

    public static void CallDeferredIfTools(this Node obj, StringName method, params Variant[] args) {
        #if TOOLS
            obj.CallDeferred(method, args);
        #else
            obj.Call(method, args);
        #endif
    }

    public static void CallDeferredIfTools(this Node obj, Callable method) {
        #if TOOLS
            method.CallDeferred();
        #else
            method.Call();
        #endif
    }
}