namespace cont_bancar;
using System;

public class BankAccount
{
    public Guid AccountId = Guid.NewGuid();
    public decimal Balance { get; private set; }
    public BankAccount(decimal openingBalance)
    {
        if (openingBalance < 0)
            throw new ArgumentOutOfRangeException(
                "Invalid opening balance, sum can not be lower than 0 before opening the account");
        Balance = openingBalance;
    }

    public void Withdraw(decimal sum)
    {
        if (sum < 0)
            throw new ArgumentOutOfRangeException(
                paramName: nameof(sum), message: "Sum must be positive",
                actualValue: sum
            );

        if (sum > Balance)
            throw new ArgumentOutOfRangeException(
                    paramName: nameof(sum), message: "Sum must be lower or equal to the Balance",
                    actualValue: sum
            );
     
        Balance -= sum;
    }

    public void Deposit(decimal sum)
    {
        if (sum < 0)
            throw new ArgumentOutOfRangeException(
                paramName: nameof(sum), message: "Sum must be positive",
                actualValue: sum
            );


        Balance += sum;
    }
}