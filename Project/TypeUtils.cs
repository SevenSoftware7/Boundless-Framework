

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;


namespace LandlessSkies.Core;

public static class TypeUtils {

    private static readonly Type nodeType = typeof(Node);

    public static IEnumerable<Type> GetNodesImplementing<TInterface>() {
        Type interfaceType = typeof(TInterface);

        // Get all types in the current compilation context
        Assembly? assembly = Assembly.GetExecutingAssembly();
        Type[]? allTypes = assembly.GetTypes();

        // Filter types that inherit from the specified interface and existing class
        IEnumerable<Type>? filteredTypes = allTypes
            .Where(type =>
                interfaceType.IsAssignableFrom(type) &&
                nodeType.IsAssignableFrom(type) &&
                type != nodeType); // Exclude the base type itself

        return filteredTypes;
    }

}