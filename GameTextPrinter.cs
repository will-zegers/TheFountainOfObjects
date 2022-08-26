namespace TFoO;

public static class GameTextPrinter
{
    public static void Write(string text, TextType type)
    {
        ConsoleColor foregroundColor = type switch
        {
            TextType.Dead => ConsoleColor.DarkRed,
            TextType.EmptyRoom => ConsoleColor.DarkGray,
            TextType.Entrance => ConsoleColor.Yellow,
            TextType.Fatal => ConsoleColor.Red,
            TextType.Fountain => ConsoleColor.Blue,
            TextType.Narrative => ConsoleColor.Magenta,
            TextType.UserInput => ConsoleColor.Cyan,
            TextType.Victory => ConsoleColor.Green,
            TextType.Warning => ConsoleColor.DarkYellow,
            _ => ConsoleColor.White,
        };

        Console.ForegroundColor = foregroundColor;
        Console.WriteLine($"{text}\n");
        Console.ForegroundColor = ConsoleColor.White;
    }

    public static string GetUserInputString(string outputText)
    {
        Console.Write($"{outputText} ");
        Console.ForegroundColor = ConsoleColor.Cyan;
        string input = Console.ReadLine() ?? "";
        Console.ForegroundColor = ConsoleColor.White;

        return input;
    }
}

public enum TextType
{
    Dead,
    Descriptive,
    EmptyRoom,
    Entrance,
    Fatal,
    Fountain,
    Narrative,
    UserInput,
    Victory,
    Warning,
}
