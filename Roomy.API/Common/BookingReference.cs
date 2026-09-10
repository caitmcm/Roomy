namespace Roomy.API.Common;

public static class BookingReference
{
    private const string UnambiguousAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    private const int TokenLength = 6;

    public static string Generate()
    {
        var token = new char[TokenLength];

        for (var index = 0; index < token.Length; index++)
        {
            token[index] = UnambiguousAlphabet[Random.Shared.Next(UnambiguousAlphabet.Length)];
        }

        return $"BK-{new string(token)}";
    }
}
