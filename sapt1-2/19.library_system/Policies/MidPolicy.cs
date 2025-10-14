using System;
namespace Library.Abstractions;

public class MidPolicy : ILoanPolicy
{
    public TimeSpan GetLoanPeriod(ILoanable item)
    {
        return item switch
        {
            Book => TimeSpan.FromDays(10),
            Magazine => TimeSpan.FromDays(2),
            _ => throw new NotSupportedException(item.GetType().Name())
        };
    }
}