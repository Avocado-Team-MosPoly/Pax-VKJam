public static class GameLaunchParams
{
    public static string SceneName = "";
    public static int MinPlayersToLaunchCount = 2;
    
    public static void Reset()
    {
        SceneName = "";
        MinPlayersToLaunchCount = 2;
    }
}