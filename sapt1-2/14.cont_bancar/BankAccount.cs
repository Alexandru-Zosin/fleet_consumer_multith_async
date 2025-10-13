namespace cont_bancar;
public class BankAccount
{
    private readonly object _syncLock = new();
    public Guid AccountId = Guid.NewGuid();
    public decimal Balance { get; private set; }
    public BankAccount(decimal openingBalance)
    {
        if (openingBalance < 0)

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
                    paramName: nameof(sum), message: "Sum must be positive",
                    actualValue: sum
            );
        lock (_syncLock)
        {
            Balance -= sum;
        }
    }

    public void Deposit(decimal sum)
    {
        if (sum < 0)
            throw new ArgumentOutOfRangeException(
                paramName: nameof(sum), message: "Sum must be positive",
                actualValue: sum
            );


        lock (_syncLock)
        {
            Balance += sum;
        }
    }
}