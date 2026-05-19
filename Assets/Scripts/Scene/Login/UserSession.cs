public static class UserSession
{
    public static string Username { get; private set; } = "guest";

    public static void SetUsername(string username)
    {
        Username = string.IsNullOrWhiteSpace(username) ? "guest" : username.Trim();
    }
}
