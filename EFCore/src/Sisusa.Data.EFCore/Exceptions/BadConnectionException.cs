namespace Sisusa.Data.EFCore.Exceptions
{
    public class BadConnectionException(
        string message = "Could not establish connection to the database. Please verify connection string and try again.")
        : Exception(message)
    {
    }
}
