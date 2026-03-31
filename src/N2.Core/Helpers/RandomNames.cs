using System.Security.Cryptography;

namespace N2.Core.Helpers;

public static class RandomNames
{

    public static string CreateRandomName()
    {
#if NETSTANDARD2_1_OR_GREATER
        return names[RandomNumberGenerator.GetInt32(names.Length)];
#else
        return names[Extensions.RandomStringGenerator.GetInt32(names.Length)];
#endif
    }

    private static readonly string[] names = [
            "Angelique",
            "Balthazar",
            "Cassandra",
            "Dante",
            "Evangeline",
            "Felix",
            "Gabrielle",
            "Hector",
            "Isolde",
            "Jasper",
            "Kassandra",
            "Lysander",
            "Morgana",
            "Nathaniel",
            "Ophelia",
            "Percival",
            "Quintessa",
            "Raphael",
            "Seraphina",
            "Tristan",
            "Ursula",
            "Valerian",
            "Wolfgang",
            "Xanthe",
            "Ysabel",
            "Zephyr"
    ];
}
