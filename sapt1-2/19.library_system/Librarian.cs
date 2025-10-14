namespace Library;
using System;

namespace Library.Abstractions;
public sealed class Librarian : Person
{
    public string EmployeeId { get; }
    public DateTimeOffset HireDate { get; }

    public Librarian(string name, string employeeId) : base(name)
    {
        if (string.IsNullOrWhiteSpace(employeeId))
            throw new ArgumentException("Employee ID is required.", nameof(employeeId));

        EmployeeId = employeeId;
        HireDate = DateTimeOffset.UtcNow;
    }

    public override string GetDescription() =>
        $"Librarian {Name} (ID: {EmployeeId}) employed since {HireDate:yyyy-MM-dd}";
}
