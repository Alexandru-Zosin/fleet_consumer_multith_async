using System;
namespace Library.Abstractions;

public class ShortPolicy : ILoanPolicy
{
    public TimeSpan GetLoanPeriod(ILoanable item)
    {
        return item switch
        {
            Book => TimeSpan.FromDays(7),
            Magazine => TimeSpan.FromDays(1),
            _ => throw new NotSupportedException(item.GetType().Name())
        };
    }
}