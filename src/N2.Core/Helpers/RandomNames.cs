namespace N2.Core.Helpers;

public static class RandomNames
{
    private static readonly Random random = new();

    public static string CreateRandomName()
    {
        return names[random.Next(names.Length)];
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
