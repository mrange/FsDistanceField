var target        = Argument("target", "Test");
var configuration = Argument("configuration", "Release");
var slnPath       = "./src/FsDistanceField.sln";
// TODO: How to not hard code the version but actually publish with a new version?
var packPath      = "./src/FsDistanceField/nupkg/FsDistanceField.0.0.2.nupkg";

var nugetApi      = "https://api.nuget.org/v3/index.json";
var nugetApiKey   = EnvironmentVariable("NUGET_API_KEY");
//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .WithCriteria(c => HasArgument("rebuild"))
    .Does(() =>
{
    DotNetClean(slnPath, new DotNetCleanSettings
    {
        Configuration = configuration,
    });
});

Task("Build")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetBuild(slnPath, new DotNetBuildSettings
    {
        Configuration = configuration,
    });
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    DotNetTest(slnPath, new DotNetTestSettings
    {
        Configuration = configuration,
        NoBuild       = true,
        NoRestore     = true,
    });
});

Task("Pack")
    .IsDependentOn("Test")
    .Does(() =>
{
    DotNetPack(slnPath, new DotNetPackSettings
    {
        Configuration = configuration,
        NoBuild       = true,
        NoRestore     = true,
    });
});

Task("PublishToNuGet")
    .IsDependentOn("Pack")
    .Does(() =>
{
    DotNetNuGetPush(packPath, new DotNetNuGetPushSettings
    {
            ApiKey = nugetApiKey,
            Source = nugetApi
    });
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);