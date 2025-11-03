using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DTO;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class SchemaVersionAttribute : Attribute
{
    public string Version { get; }
    public SchemaVersionAttribute(string version) => Version = version;
}

public static class DTORegistry
{
    public static readonly IReadOnlyDictionary<string, Type> Schemas =
        Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.GetCustomAttribute<SchemaVersionAttribute>() != null)
            .ToDictionary(
                t => t.GetCustomAttribute<SchemaVersionAttribute>()!.Version,
                t => t
            );
}