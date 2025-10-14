using System;

namespace Library;

public class DVD : LibraryItem
{
    public string UPC { get; }
    public ItemType Type = ItemType.DVD;

    public DVD(string title, string upc) : base(title)
    {
        if (string.IsNullOrEmpty(upc))
            throw new ArgumentException("Needs a valid upc");

        UPC = upc;
    }

    public override string GetDescription()
    {
        return $"DVD: {Title} with UPC: {UPC}"
    }
}