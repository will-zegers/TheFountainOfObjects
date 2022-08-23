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

public enum Direction
{
    East,
    North,
    South,
    West,
    Invalid,
}
