namespace Tests.Internal;

public static class TestSettings
{
    //NOTE:
    //I am using Environment variables
    //It is possible to use TestRunParameters
    //TestRunParameters can be configure in the .runsettings file and in the "dotnet test" command (https://github.com/dotnet/dotnet/blob/3a29b786732fe6f22627567b62692855055189ea/src/vstest/docs/RunSettingsArguments.md)
    //To read a TestrunParameter from NUnit: TestContext.Parameters["ParameterName"]
    public static string Get(string key)
    {
        return Environment.GetEnvironmentVariable(key)!;//TestContext.Parameters["QueueUrl"]
    }
}