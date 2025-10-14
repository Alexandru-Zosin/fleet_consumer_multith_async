using System;
namespace Library.Abstractions;

public abstract class Person(Guid guid, string name)
{
    public Guid Guid { get; } // immutable after construction
    public string Name { get; }

    protected Person(Name name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Invalid name. A name is required.");

        Guid = Guid.NewGuid();
        Name = name;
    }

    public abstract string GetDescription();
}