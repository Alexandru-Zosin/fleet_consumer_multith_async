namespace Library;

using System;

public sealed class Member : Person
{
    public string MembershipId { get; }
    public DateTimeOffset JoinedOn { get; }

    public Member(string name, string membershipId) : base(name)
    {
        if (string.IsNullOrWhiteSpace(membershipId))
            throw new ArgumentException("Membership ID is required.", nameof(membershipId));

        MembershipId = membershipId;
        JoinedOn = DateTimeOffset.UtcNow;
    }

    public override string GetDescription()
    {
        return $"Member {Name} (Membership: {MembershipId}) joined on {JoinedOn:yyyy-MM-dd}";
    }
}
