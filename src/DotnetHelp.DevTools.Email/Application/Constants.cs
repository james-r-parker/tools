namespace DotnetHelp.DevTools.Email.Application;

public static class Constants
{
    public static readonly string TableName = 
        Environment.GetEnvironmentVariable("DB_TABLE_NAME") ??
           throw new ApplicationException("TABLE_NAME environment variable not set");
}