using System;

//This script is public Utility about caculate time.
public class TimeUtility
{
    public static string GetOneMonthAgoDate()
    {
        DateTime now = DateTime.Now.AddDays(-30);

        return now.ToString(("yyyy-MM-dd"));
    }

    public static string GetCurrentDate()
    {
        return DateTime.Now.ToString(("yyyy-MM-dd"));
    }

    public static string GetCurrentDateTime()
    {
        return DateTime.Now.ToString(("yyyy-MM-dd HH:mm:ss"));
    }

    public static string GetFutureDate()
    {
        return "2035-12-01";
    }

    public static string GetPastDate()
    {
        return "2022-01-01";
    }
}
