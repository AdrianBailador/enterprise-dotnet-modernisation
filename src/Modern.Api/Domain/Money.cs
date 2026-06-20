namespace Modern.Api.Domain;

public readonly record struct Money(decimal Amount, string Currency = "EUR")
{
    public static readonly Money Zero = new(0m);

    public static Money Of(decimal amount, string currency = "EUR")
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative.");
        return new Money(amount, currency);
    }

    public override string ToString() => $"{Amount:F2} {Currency}";
}
