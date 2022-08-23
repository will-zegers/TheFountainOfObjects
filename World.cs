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
        else if (roomType == RoomType.Amarok || roomType == RoomType.DeadAmarok)
        {
            GameTextPrinter.Write(
                "Stepping across the threshold of the room, your senses are overwhelmed by a fetid odor\n" +
                "of organic rot and decay.", TextType.Narrative);
            if (roomType == RoomType.Amarok)
            {
                GameTextPrinter.Write(
                    "Somewhere in the darkness nearby, you hear the approach of plodding footsteps\n" +
                    "and wheezing that could only belong to an amarok", TextType.Narrative);
            }
            else
            {
                GameTextPrinter.Write(
                    "Lying in the center of the room is the unmistakable shape of large, dead creature.\n" +
                    "It looks like your arrow found its mark; the fletching sticking out of what remains\n" +
                    "of its right eye, and the arrowhead embedded deep in the amarok's tiny brain.", TextType.Narrative);
            }
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

    public bool ShootIntoRoom((int row, int column) targetRoom)
    {
        GameTextPrinter.Write("The arrow flies into the darkness of the next room.", TextType.Narrative);

        int roomIndex = targetRoom.row * Rows + targetRoom.column;
        if (Rooms[roomIndex] == RoomType.Amarok)
        {
            GameTextPrinter.Write(
                "Before you can lower your bow, you hear a sickening *SQUISH* followed by a\n" +
                "thunderous, glottal scream from beyond the darkness. Your bow arm falls by your\n" +
                "side, and a tremor shakes the stone floor as whatever creature was in the arrow's\n" +
                "path falls over dead.", TextType.Narrative);
            Rooms[roomIndex] = RoomType.DeadAmarok;
            return true;
        } else if (Rooms[roomIndex] == RoomType.Maelstrom)
        {
            GameTextPrinter.Write(
                "In an instant, you feel a blast of wind from the direction of your shot. In the\n" +
                "midst of the nearby roaring tempest, you can hear the unmistakable sound of an\n" +
                "arrow be thrown around the room, clattering as it bounces off of wall after wall.\n" +
                "It seems your arrow had no effect, save from pissing off the source of the wind.", TextType.Narrative);
            return true;
        }
        GameTextPrinter.Write(
            "A moment later, you hear the sound of the arrow striking stone before clattering\n" +
            "broken and useless to the ground. It seems you fired into an empty room.", TextType.Narrative);
        return false;
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
    DeadAmarok,
    Empty,
    Entrance,
    Fountain,
    Maelstrom,
    Pit,
    Amarok,
}
