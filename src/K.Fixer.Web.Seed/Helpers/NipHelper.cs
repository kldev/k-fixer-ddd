namespace K.Fixer.Web.Seed.Helpers;

public static class NipHelper
{
    private static readonly int[] Weights = [6, 5, 7, 2, 3, 4, 5, 6, 7];

    /// <summary>
    /// Generates a valid Polish NIP number (10 digits with correct checksum).
    /// </summary>
    public static string GenerateValidNip(Random random)
    {
        while (true)
        {
            var digits = new int[10];
            for (var i = 0; i < 9; i++)
                digits[i] = random.Next(0, 10);

            var sum = 0;
            for (var i = 0; i < 9; i++)
                sum += Weights[i] * digits[i];

            var checksum = sum % 11;
            if (checksum == 10)
                continue; // invalid checksum digit, regenerate

            digits[9] = checksum;
            return string.Concat(digits);
        }
    }
}
