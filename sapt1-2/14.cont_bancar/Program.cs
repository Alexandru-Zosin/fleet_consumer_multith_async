using cont_bancar;

BankAccount acc1 = new BankAccount(300);
acc1.Deposit(200);
acc1.Withdraw(100);
Console.WriteLine(acc1.Balance);   