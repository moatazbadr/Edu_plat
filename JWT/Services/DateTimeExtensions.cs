namespace Edu_plat.Services;
public static class DateTimeExtensions
{
    public static DateTime ToEgyptTime(this DateTime utcDateTime)
    {
        TimeZoneInfo egyptTimeZone;

        try
        {
            egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");

        }
        catch (TimeZoneNotFoundException)
        {
            egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
        }

        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, egyptTimeZone);
    }
}