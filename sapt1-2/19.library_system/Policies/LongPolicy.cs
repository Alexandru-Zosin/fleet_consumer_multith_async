using System;
namespace Library.Abstractions;

public class LongPolicy: ILoanPolicy
{
    public TimeSpan GetLoanPeriod(ILoanable item)
    {
        return item switch
        {
            Book => TimeSpan.FromDays(14),
            Magazine => TimeSpan.FromDays(3),
            _ => throw new NotSupportedException(item.GetType().Name())
        };
    }
}