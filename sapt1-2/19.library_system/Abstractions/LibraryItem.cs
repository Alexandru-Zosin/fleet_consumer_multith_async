using System;
namespace Library.Abstractions;

public abstract class LibraryItem
{
    public Guid Id { get; }
    public string Title { get; }
    protected LibraryItem(string title)
    {
        if (string.IsNullOrEmpty(title))
            throw new ArgumentException("Valid title for the item required.");
        Id = Guid.NewGuid();
        Title = title;
    }

    public virtual string GetDescription()
    {
        return $"Item: {Title}"
    }
}