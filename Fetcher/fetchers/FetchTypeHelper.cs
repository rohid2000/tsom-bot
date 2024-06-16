public static class FetchTypeHelper
{
    public static DateTime ConvertStringToDateTime(string dateString)
    {
        return DateTimeOffset.FromUnixTimeSeconds(long.Parse(dateString)).DateTime;
    }
}