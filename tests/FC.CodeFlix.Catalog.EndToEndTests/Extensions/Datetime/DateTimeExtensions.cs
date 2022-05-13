
namespace FC.CodeFlix.Catalog.EndToEndTests.Extensions.Datetime;

internal static class DateTimeExtensions
{
    public static System.DateTime TrimMilliseconds(this System.DateTime dateTime)
   => new(dateTime.Year,
            dateTime.Month,
            dateTime.Day,
            dateTime.Hour,
            dateTime.Minute,
            dateTime.Second,
            0,
            dateTime.Kind);
}
