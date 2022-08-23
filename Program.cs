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
