string mapSizeString = GameTextPrinter.GetUserInputString("Choose map size (small, medium, large):");
MapSize mapSize = mapSizeString.ToLower() switch
{
    "small" => MapSize.Small,
    "medium" => MapSize.Medium,
    "large" => MapSize.Large,
};
bool debug = Convert.ToBoolean(GameTextPrinter.GetUserInputString("Debug?"));

World world = new World(mapSize, debug);
Player player = new Player(world.EntranceLocation);
TheFountainOfObjectsGame game = new TheFountainOfObjectsGame(player, world);

string separator = new string('-', 100);
for (; ;)
{
    Console.Clear();

    if (debug) GameTextPrinter.Write(game.GetDebugMap(), TextType.Descriptive);

    GameTextPrinter.Write(separator, TextType.Descriptive);
    GameTextPrinter.Write(player.ToString(), TextType.Descriptive);
    game.GetCurrentRoomDescription();

    if (game.IsGameOver()) break;

    string selection = GameTextPrinter.GetUserInputString("What do you want to do? ");
    game.PerformAction(selection);
}

public static class GameTextPrinter
{
    public static void Write(string text, TextType type)
    {
        ConsoleColor foregroundColor = type switch
        {
            TextType.EmptyRoom => ConsoleColor.DarkGray,
            TextType.Entrance => ConsoleColor.Yellow,
            TextType.Fountain => ConsoleColor.Blue,
            TextType.Narrative => ConsoleColor.Magenta,
            TextType.UserInput => ConsoleColor.Cyan,
            TextType.Warning => ConsoleColor.DarkYellow,
            TextType.Fatal => ConsoleColor.Red,
            _ => ConsoleColor.White,
        };

        Console.ForegroundColor = foregroundColor;
        Console.WriteLine(text);
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

public class TheFountainOfObjectsGame
{
    private Player _player;
    private World _world;
    private bool _isPlayerAlive = true;

    public TheFountainOfObjectsGame(Player player, World world)
    {
        _world = world;
        _player = player;
    }

    public void PerformAction(string action)
    {
        switch (action)
        {
            case "ef":
            case "enable fountain":
                EnableFountain();
                break;
            default:
                UpdatePlayerLocation(action);
                break;
        }
    }

    public void GetCurrentRoomDescription()
    {
        _world.GetRoomDescription(_player.Row, _player.Column);
        if (_world.GetRoomType(_player.Row, _player.Column) == RoomType.Pit)
        {
            GameTextPrinter.Write(
                "At long last, you hit the bottom of the pit to the sound of every bone in your body breaking\n" +
                "and searing pain of every organ rupturing. With your concussed head whirring in the pitch darkness,\n" +
                "in a final moment of clarity you realize that death is now inevitable." , TextType.Fatal);
            _isPlayerAlive = false;
        }
    }

    private bool EnableFountain()
    {
        if ((_player.Row, _player.Column) == _world.FountainLocation)
        {
            if (!_world.IsFountainEnabled)
            {
                _world.IsFountainEnabled = true;
                return true;
            } else
            {
                GameTextPrinter.Write("The fountain has already been enabled", TextType.Fountain);
                Console.ReadLine();
            }
        } else
        {
            GameTextPrinter.Write("There is no fountain in this room", TextType.Warning);
            Console.ReadLine();
        }
        return false;
    }

    private bool UpdatePlayerLocation(string command)
    {
        string wallDirection = null;
        switch (command) {
            case "move north":
            case "mn":
                if (_player.Row == 0) wallDirection = "north";
                else _player.UpdateLocation(Direction.North);
                break;
            case "move south":
            case "ms":
                if (_player.Row == _world.Rows - 1) wallDirection = "south";
                else _player.UpdateLocation(Direction.South);
                break;
            case "move west":
            case "mw":
                if (_player.Column < 0) wallDirection = "west";
                else _player.UpdateLocation(Direction.West);
                break;
            case "move east":
            case "me":
                if (_player.Column >= _world.Columns - 1) wallDirection = "east";
                else _player.UpdateLocation(Direction.East);
                break;
            default:
                return false;
        }

        if (wallDirection != null)
        {
            GameTextPrinter.Write($"There is a wall, you cannot go {wallDirection}!", TextType.Warning);
            Console.ReadLine();
            return false;
        }

        return true;
    }
    public bool IsGameOver()
    {
        if (_world.IsFountainEnabled && (_player.Row, _player.Column) == _world.EntranceLocation)
        {
            GameTextPrinter.Write("You win!", TextType.Narrative);
            return true;
        } else if (!_isPlayerAlive)
        {
            GameTextPrinter.Write("You died!", TextType.Fatal);
            return true;
        }
        return false;
    }

    /* ----------------------------------------- DEBUG MEMBERS ------------------------------------------------------ */
    public string GetDebugMap()
    {
        // Print the north wall and first row of rooms
        string map = "+";
        for (int c = 0; c < _world.Columns; ++c) map += "---+";
        map += "\n|";

        for (int c = 0; c < _world.Columns - 1; ++c) map += $" {GetRoomContents(0, c)}  ";
        map += $" {GetRoomContents(0, _world.Columns - 1)} |\n";

        // Print rows 1 though m, where m is Rows - 1
        for (int r = 1; r < _world.Rows; ++r)
        {

            for (int c = 0; c < _world.Columns; ++c)
            {
                map += "+   ";
            }
            map += "+\n";

            map += "|";
           for (int c = 0; c < _world.Columns - 1; ++c)
            {
                map += $" {GetRoomContents(r, c)}  ";
            } 
            map += $" {GetRoomContents(r, _world.Columns - 1)} |\n";
        }
        map += "+";

        // Finish by printing the south wall
        for (int c = 0; c < _world.Columns; ++c) map += "---+";

        return map;
    }

    private char GetRoomContents(int row, int column)
    {
        if ((row, column) == (_player.Row, _player.Column)) return 'P';

        return _world.GetRoomType(row, column) switch
        {
            RoomType.Fountain => 'F',
            RoomType.Entrance => 'E',
            RoomType.Maelstrom => 'M',
            RoomType.Pit => 'O',
            _ => ' '
        };
    }
}

public class Player
{
    public int Row { get; private set; }
    public int Column { get; private set; }

    public Player((int, int) startingLocation)
    {
        (Row, Column) = startingLocation;
    }

    public void UpdateLocation(Direction direction)
    {
        switch (direction)
        {
            case Direction.East:
                Column += 1;
                break;
            case Direction.North:
                Row -= 1;
                break;
            case Direction.South:
                Row += 1;
                break;
            case Direction.West:
                Column -= 1;
                break;
        }
    }

    public override string ToString() => $"You are in the room at (Row={Row}, Column={Column}).";
}

public class World
{
    public bool IsFountainEnabled = false;
    public readonly (int Row, int Column) FountainLocation;
    public readonly (int Row, int Column) EntranceLocation;
    public readonly int Rows;
    public readonly int Columns;
    private readonly RoomType[] Rooms;

    public World(MapSize mapSize, bool debug = false)
    {
        (Rows, Columns, int maxPitCount, int maxMaelstromCount) = mapSize switch
        {
            MapSize.Small => (4, 4, 1, 1),
            MapSize.Medium => (6, 6 , 2, 1),
            MapSize.Large => (8, 8, 4, 2),
        };
        Rooms = new RoomType[Rows * Columns];

        for (int i = 0; i < Rooms.Length; i++) Rooms[i] = RoomType.Empty;

        Random rng = new Random();
        (FountainLocation.Row, FountainLocation.Column) = (rng.Next(Rows), rng.Next(Columns));
        int fountainIndex = FountainLocation.Row * Rows + FountainLocation.Column;
        Rooms[fountainIndex] = RoomType.Fountain;

        (EntranceLocation.Row, EntranceLocation.Column) = GenerateEntranceLocation();
        int entranceIndex = EntranceLocation.Row * Rows + EntranceLocation.Column;
        Rooms[entranceIndex] = RoomType.Entrance;

        AddHazards(maxPitCount, RoomType.Pit);
        AddHazards(maxMaelstromCount, RoomType.Maelstrom);
    }

    public void GetRoomDescription(int row, int column)
    {
        RoomType roomType = Rooms[row * Rows + column];
        if (roomType == RoomType.Fountain)
        {
            if (IsFountainEnabled)
            {
                GameTextPrinter.Write("You hear the rushing water from the Fountain of Objects. It has been reactivated!", TextType.Fountain);
            }
            else
            {
                GameTextPrinter.Write("You hear water dripping in this room. The Fountain of Objects is here!", TextType.Fountain);
            }

        }
        else if (roomType == RoomType.Entrance)
        {
            if (IsFountainEnabled)
            {
                GameTextPrinter.Write("The Fountain of Objects has been reactivated, and you have escaped with your life!", TextType.Narrative);
            }
            else
            {
                GameTextPrinter.Write("You see light coming from the cavern entrance.", TextType.Entrance);
            }
        } else if (roomType == RoomType.Pit)
        {
            GameTextPrinter.Write("As you step into the room, your foot fails to connect to solid ground as you feel your entire body weight fall forward.", TextType.Narrative);

        } else GameTextPrinter.Write("You stand in a dark, empty room.", TextType.EmptyRoom);

        (bool pit, bool maelstrom) nearbyHazards = DetectNearbyHazards(row, column);
        if (nearbyHazards.pit)
        {
            GameTextPrinter.Write("You feel a draft. There is a pit in a nearby room.", TextType.Warning);
        } else if (nearbyHazards.maelstrom) { 
            GameTextPrinter.Write("You hear the growling and groaning of a maelstrom nearby.", TextType.Warning);
        }
    }

    public (bool, bool) DetectNearbyHazards(int row, int column)
    {
        int rMin = row - 1 >= 0 ? row - 1 : 0;
        int rMax = row + 2 <= Rows ? row + 2 : Rows;
        int cMin = column - 1 >= 0 ? column - 1 : 0;
        int cMax = column + 2 <= Columns ? column + 2 : Columns;

        (bool pit, bool maelstrom) hazards = (false, false);
        for (int r = rMin; r < rMax; ++r)
        {
            for (int c = cMin; c < cMax; ++c)
            {
                if ((r, c) == (row, column)) continue;
                else if (Rooms[r * Rows + c] == RoomType.Pit) hazards.pit = true;
                else if (Rooms[r * Rows + c] == RoomType.Maelstrom) hazards.maelstrom = true;
            }
        }
        return hazards;
    }

    public RoomType GetRoomType(int row, int column) => Rooms[row * Rows + column];

    private (int, int) GenerateEntranceLocation()
    {
        // Randomize if the entrance will be at the north (0), east (1), south (2), or west (3) wall.
        Random rng = new Random();
        return (rng.Next(4)) switch
        {
            0 => (0, rng.Next(Columns)), // north wall
            1 => (rng.Next(Rows), Columns - 1), // east wall
            2 => (Rows - 1, rng.Next(Columns)), // south wall
            _ => (rng.Next(Rows), 0), // west wall
        };
    }

    private void AddHazards(int maxHazardCount, RoomType hazardType)
    {
        Random rng = new Random();

        int hazardCount = 0;
        while (hazardCount < maxHazardCount)
        {
            (int row, int column) hazardLocation = (rng.Next(Rows), rng.Next(Columns));
            if (hazardLocation == EntranceLocation || hazardLocation == FountainLocation) continue;

            int hazardIndex = hazardLocation.row * Rows + hazardLocation.column;
            Rooms[hazardIndex] = hazardType;
            hazardCount++;
        }
    }
}

public enum Direction
{
    East,
    North,
    South,
    West,
}

public enum MapSize
{
    Small,
    Medium,
    Large,
}

public enum RoomType
{
    Empty,
    Entrance,
    Fountain,
    Maelstrom,
    Pit,
}

public enum TextType
{
    Descriptive,
    EmptyRoom,
    Entrance,
    Fatal,
    Fountain,
    Narrative,
    UserInput,
    Warning,
}


