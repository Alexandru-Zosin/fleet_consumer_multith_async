using System;
namespace Library.Abstractions;

public interface ILoanPolicy
{
    public TimeSpan GetLoanPeriod(ILoanable item);
}