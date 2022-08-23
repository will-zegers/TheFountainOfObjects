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
        (Rows, Columns, int maxPitCount, int maxMaelstromCount, int maxAmarokCount) = mapSize switch
        {
            MapSize.Small => (4, 4, 1, 1, 1),
            MapSize.Medium => (6, 6 , 2, 1, 2),
            MapSize.Large => (8, 8, 4, 2, 3),
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
        AddHazards(maxAmarokCount, RoomType.Amarok);
    }

    public void GetRoomDescription(int row, int column)
    {
        (bool pit, bool maelstrom, bool amarok) nearbyHazards = DetectNearbyHazards(row, column);
        if (nearbyHazards.pit)
        {
            GameTextPrinter.Write("You feel a draft. There is a pit in a nearby room.", TextType.Warning);
        } 
        if (nearbyHazards.maelstrom) { 
            GameTextPrinter.Write("You hear the growling and groaning of a maelstrom nearby.", TextType.Warning);
        }
        if (nearbyHazards.amarok)
        {
            GameTextPrinter.Write("You can smell the rotten stench of an amarok in a nearby room.", TextType.Warning);
        }

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
            GameTextPrinter.Write(
                "As you step into the room, your foot fails to connect to solid ground as you feel your\n" +
                "entire body weight fall forward.", TextType.Narrative);

        }
        else if (roomType == RoomType.Maelstrom)
        {
            GameTextPrinter.Write(
                "Within seconds of entering the room, a powerful gust of wind throws you against\n" +
                "the wall, and just as quickly, slams you against the opposite wall.", TextType.Narrative);
            GameTextPrinter.Write("It seems you've encountered a malevolent, sentient wind - a Maelstrom!", TextType.Narrative);
        }
        else if (roomType == RoomType.Amarok)
        {
            GameTextPrinter.Write(
                "Stepping across the threshold of the room, your senses are overwhelmed by a fetid odor\n" +
                "of organic rot and decay.", TextType.Narrative);
            GameTextPrinter.Write(
                "Somewhere in the darkness nearby, you hear the approach of plodding footsteps\n" +
                "and wheezing that could only belong to an amarok", TextType.Narrative);
        }
        else GameTextPrinter.Write("You stand in a dark, empty room.", TextType.EmptyRoom);
    }

    public (bool, bool, bool) DetectNearbyHazards(int row, int column)
    {
        int rMin = row - 1 >= 0 ? row - 1 : 0;
        int rMax = row + 2 <= Rows ? row + 2 : Rows;
        int cMin = column - 1 >= 0 ? column - 1 : 0;
        int cMax = column + 2 <= Columns ? column + 2 : Columns;

        (bool pit, bool maelstrom, bool amarok) hazards = (false, false, false);
        for (int r = rMin; r < rMax; ++r)
        {
            for (int c = cMin; c < cMax; ++c)
            {
                if ((r, c) == (row, column)) continue;
                else if (Rooms[r * Rows + c] == RoomType.Pit) hazards.pit = true;
                else if (Rooms[r * Rows + c] == RoomType.Maelstrom) hazards.maelstrom = true;
                else if (Rooms[r * Rows + c] == RoomType.Amarok) hazards.amarok = true;
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
            if (GetRoomType(hazardLocation.row, hazardLocation.column) != RoomType.Empty) continue;

            int hazardIndex = hazardLocation.row * Rows + hazardLocation.column;
            Rooms[hazardIndex] = hazardType;
            hazardCount++;
        }
    }
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
    Amarok,
}
