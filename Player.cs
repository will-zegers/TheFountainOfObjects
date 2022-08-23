public class Player
{
    public int Row { get; private set; }
    public int Column { get; private set; }
    public (int, int) Location { get => (Row, Column); }

    private uint _arrows = 5;

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

    // TODO: probably implement an Option type as the return value
    public (int, int) Shoot(Direction direction)
    {
        if (_arrows > 0)
        {
            _arrows--;
            return direction switch
            {
                Direction.North => (Row - 1, Column),
                Direction.South => (Row + 1, Column),
                Direction.East => (Row, Column + 1),
                Direction.West => (Row, Column - 1),
                _ => (-1, -1),
            };
        }
        GameTextPrinter.Write("You are out of arrows! (Press any key to continue...)", TextType.Warning);
        Console.ReadKey();

        return (-1, -1);
    }

    public override string ToString()
    {
        string arrowPlural = _arrows == 1 ? "arrow" : "arrows";
        return $"You are in the room at (Row={Row}, Column={Column}) with {_arrows} {arrowPlural} in your quiver.";
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
