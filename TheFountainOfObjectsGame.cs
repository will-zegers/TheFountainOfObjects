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
