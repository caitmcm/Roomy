namespace Roomy.API.Common;

public static class BookingReference
{
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    private const int TokenLength = 6;

    public static string Generate() => $"BK-{new string(Random.Shared.GetItems<char>(Alphabet, TokenLength))}";
}
