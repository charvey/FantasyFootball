namespace FantasyFootball.Terminal
{
    public static class UniqueId
    {
        public static string Create() => Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }
}
