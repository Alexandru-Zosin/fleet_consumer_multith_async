using System;

namespace Library;
using Library.Abstractions;

public class Magazine : LibraryItem, ILoanable
{
    public string ISSN { get; }
    public ILoanPolicy Policy { get; set; }
    public ItemType Type = ItemType.Magazine;

    public bool IsBorrowed { get; set; }
    public DateTimeOffset? BorrowMoment { get; set; }
    public DateTimeOffset? ReturnMoment { get; set; }
    public TimeSpan? DurationOfBorrow { get; private set; }

    public Magazine(string title, string issn, ILoanPolicy policy) : base(title)
    {
        if (string.IsNullOrEmpty(issn))
            throw new ArgumentException("Needs a valid ISSN");
        Policy = policy;
        DurationOfBorrow = Policy.GetLoanPeriod(self);
    }

    public override string GetDescription()
    {
        return $"Magazine: {Title} with ISSN: {ISSN}";
    }
}