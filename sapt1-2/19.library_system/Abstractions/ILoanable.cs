using System;

namespace Library.Abstractions;

public interface ILoanable
{
    bool IsBorrowed { get; set; }
    DateTimeOffset? BorrowMoment { get; set; }
    DateTimeOffset? ReturnMoment { get; set; }
    TimeSpan? DurationOfBorrow { get; }

    bool Borrow()
    {
        if (IsBorrowed)
            return false;
        BorrowMoment = DateTimeOffset.UtcNow;
        ReturnMoment = null;
        return true;
    }

    bool Return()
    {
        if (!IsBorrowed)
            return false;
        ReturnMoment = DateTimeOffset.UtcNow;
        return true;
    }

    bool IsPastDue()
    {
        if (BorrowMoment is null || DurationOfBorrow is null)
            return false;

        var comparisonTime = ReturnMoment ?? DateTimeOffset.UtcNow;
        var elapsed = comparisonTime - BorrowMoment.Value;

        return elapsed > DurationOfBorrow.Value;
    }
}