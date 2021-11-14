public static class NoteConverter
{
    public static string[] Notes;
    private static readonly string[] Letters = new[]
        {
                "c","cs","d","ds","e","f","fs","g","gs","a","as","b"
            };

    static NoteConverter()
    {
        Notes = new string[128];
        for (int i = 0; i < 11; i++)
        {
            for (int j = 0; j < 12; j++)
            {
                if (12 * i + j >= 128) return;
                Notes[12 * i + j] = Letters[j] + (i - 2);
            }
        }
    }
}
