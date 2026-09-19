namespace Swasthya.CoreLabs.TestSupport;

public static class TestCategory
{
    public const string Db = "Db";
}

public static class TestScratch
{
    public static string? GetConnectionString()
    {
        string? connection = Environment.GetEnvironmentVariable("SCL_PG_TEST_CONNECTION");
        if (!string.IsNullOrWhiteSpace(connection))
        {
            return connection;
        }

        string connectionFile = Path.Combine(
            Path.GetTempPath(), "scl-pg-test", "connection.txt");
        if (File.Exists(connectionFile))
        {
            return File.ReadAllText(connectionFile);
        }

        return null;
    }
}
