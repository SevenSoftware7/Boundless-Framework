using Godot;


namespace EndlessSkies.Core;

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
}