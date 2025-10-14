namespace Library;
using System;

public class Book : LibraryItem, ILoanable
{
    public string ISBN { get; }
    public ILoanPolicy Policy { get; set; }
    public ItemType Type = ItemType.Book;

    public bool IsBorrowed { get; set; }
    public DateTimeOffset? BorrowMoment { get; set; }
    public DateTimeOffset? ReturnMoment { get; set; }
    public TimeSpan? DurationOfBorrow { get; private set; }

    public Book(string title, string isbn, ILoanPolicy policy) : base(title)
    {
        if (string.IsNullOrEmpty(isbn))
            throw new ArgumentException("Needs a valid ISBN");
        ISBN = isbn;
        Policy = policy;
        DurationOfBorrow = Policy.GetLoanPeriod(self);
    }

    public override string GetDescription()
    {
        return $"Book: {Title} with ISBN: {ISBN}";
    }
}