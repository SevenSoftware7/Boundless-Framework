

using Godot;

namespace LandlessSkies.Core;

public interface ICostumable<T> where T : Resource {

    T? Costume { get; set; }
}