string mapSizeString = GameTextPrinter.GetUserInputString("Choose map size (small, medium, large):");
MapSize mapSize = mapSizeString.ToLower() switch
{
    "small" => MapSize.Small,
    "medium" => MapSize.Medium,
    "large" => MapSize.Large,
};
bool debug = Convert.ToBoolean(GameTextPrinter.GetUserInputString("Debug?"));

World world = new World(mapSize);
Player player = new Player(world.EntranceLocation);
TheFountainOfObjectsGame game = new TheFountainOfObjectsGame(player, world, debug);

for (; ;)
{
    Console.Clear();

    game.DisplayStatus();
    if (game.IsGameOver()) break;
    game.GetPlayerAction();
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

public class TheFountainOfObjectsGame
{
    private Player _player;
    private World _world;
    private bool _isPlayerAlive = true;
    private bool _debug = false;
    private readonly string StatusSeparator = new string('-', 100);

    public TheFountainOfObjectsGame(Player player, World world, bool debug = false)
    {
        _world = world;
        _player = player;
        _debug = debug;
    }

    public void DisplayStatus()
    {
        Console.Clear();

        if (_debug)
        {
            GameTextPrinter.Write(GetDebugMap(), TextType.Descriptive);
        }
        GameTextPrinter.Write(StatusSeparator, TextType.Descriptive);
        GameTextPrinter.Write(_player.ToString(), TextType.Descriptive);
        GetCurrentRoomDescription();
    }

    public void GetPlayerAction()
    {
        string selection = GameTextPrinter.GetUserInputString("What do you want to do? ");
        PerformAction(selection);
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
                Direction direction = RoomTypeStringToEnum(action);
                UpdatePlayerLocation(direction);
                break;
        }
    }

    public void GetCurrentRoomDescription()
    {
        _world.GetRoomDescription(_player.Row, _player.Column);

        RoomType roomType = _world.GetRoomType(_player.Row, _player.Column);
        if (roomType == RoomType.Pit)
        {
            GameTextPrinter.Write(
                "At long last, you hit the bottom of the pit to the sound of every bone in your body breaking\n" +
                "and the acute, agonizing pain of every organ rupturing. With your concussed head whirring in the\n" +
                "pitch darkness, you realize in your final moments of clarity that death is now inevitable." , TextType.Fatal);
            _isPlayerAlive = false;
        } else if (_world.GetRoomType(_player.Row, _player.Column) == RoomType.Maelstrom)
        {
            GameTextPrinter.Write(
                "With a sudden, violent upheavel the Maelstrom lifts you off the ground and ingests you in its\n" +
                "raging currents. Shortly after losing any sense of which way is up, you're tossed about in the darkness\n" +
                "like a ragdoll. The winds finally subside; the Maelstrom has let you go somewhere in the darkness, with a\n" +
                "few bruises as souveniours.", TextType.Narrative);
            GameTextPrinter.Write("Press any key to continue...", TextType.Descriptive);
            Console.ReadKey();
            _world.RelocateMaelstrom(_player.Location);
            UpdatePlayerLocation(Direction.North, false);
            UpdatePlayerLocation(Direction.East, false);
            UpdatePlayerLocation(Direction.East, false);
            DisplayStatus();
        }
    }

    private Direction RoomTypeStringToEnum(string str)
    {
        return str switch
        {
            "move north" => Direction.North,
            "move south" => Direction.South,
            "move west" => Direction.West,
            "move east" => Direction.East,
            _ => Direction.Invalid,
        };
    }

    private bool EnableFountain()
    {
        if (_player.Location == _world.FountainLocation)
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

    private bool UpdatePlayerLocation(Direction direction, bool showWallDirection = true)
    {
        string wallDirection = null;
        switch (direction) {
            case Direction.North:
                if (_player.Row == 0) wallDirection = "north";
                else _player.UpdateLocation(Direction.North);
                break;
            case Direction.South:
                if (_player.Row == _world.Rows - 1) wallDirection = "south";
                else _player.UpdateLocation(Direction.South);
                break;
            case Direction.West:
                if (_player.Column < 0) wallDirection = "west";
                else _player.UpdateLocation(Direction.West);
                break;
            case Direction.East:
                if (_player.Column >= _world.Columns - 1) wallDirection = "east";
                else _player.UpdateLocation(Direction.East);
                break;
            default:
                return false;
        }

        if (showWallDirection && wallDirection != null)
        {
            GameTextPrinter.Write($"There is a wall, you cannot go {wallDirection}!", TextType.Warning);
            Console.ReadLine();
            return false;
        }

        return true;
    }
    public bool IsGameOver()
    {
        if (_world.IsFountainEnabled && _player.Location == _world.EntranceLocation)
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
        if ((row, column) == _player.Location) return 'P';

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
    public (int, int) Location { get => (Row, Column); }

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
    public readonly MapSize MapSize;
    private readonly RoomType[] Rooms;

    public World(MapSize mapSize)
    {
        MapSize = mapSize;
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
        }
        else if (roomType == RoomType.Pit)
        {
            GameTextPrinter.Write("As you step into the room, your foot fails to connect to solid ground as you feel your entire body weight fall forward.", TextType.Narrative);

        }
        else if (roomType == RoomType.Maelstrom)
        {
            GameTextPrinter.Write("Within seconds of entering the room, a powerful gust of wind throws you against the wall, and then \n" + "" +
                "slams you against the opposite wall.", TextType.Narrative);
            GameTextPrinter.Write("It seems you've encountered a malevolent, sentient wind - a Maelstrom!", TextType.Narrative);
        }
        else GameTextPrinter.Write("You stand in a dark, empty room.", TextType.EmptyRoom);

        (bool pit, bool maelstrom) nearbyHazards = DetectNearbyHazards(row, column);
        if (nearbyHazards.pit)
        {
            GameTextPrinter.Write("You feel a draft. There is a pit in a nearby room.", TextType.Warning);
        } 
        if (nearbyHazards.maelstrom) { 
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

    public void RelocateMaelstrom((int row, int column) currentLocation)
    {
        Rooms[currentLocation.row * Rows + currentLocation.column] = RoomType.Empty;
        int newRow = currentLocation.row + 1 < Rows ? currentLocation.row + 1 : Rows - 1;
        int newColumn = currentLocation.column - 2 < 0 ? 0 : currentLocation.column - 2;
        Rooms[newRow * Rows + newColumn] = RoomType.Maelstrom;
    }

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
    Invalid,
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
