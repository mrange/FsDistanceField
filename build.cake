#addin nuget:?package=Cake.Git&version=2.0.0

using System.Text.RegularExpressions;

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

Task("DoYouGitIt")
    .Does(context =>
{
    var tip = GitLogTip(".");
    var tags = GitTags(".", true);
    var tipTag = tags
        .FirstOrDefault(tag => tag.Target.Sha == tip.Sha)
        ;
    if (tipTag is not null)
    {
        var tagName = tipTag.FriendlyName;
        var match   = Regex.Match(tagName, @"^v(?<version>\d+\.\d+\.\d+)$");
        if (match.Success)
        {
            var version = match.Groups["version"].Value;
            Console.WriteLine($"Tip is tagged with version: {version}");
        }
        else
        {
            Console.WriteLine($"Tip is tagged, but the tag doesn't match the version schema: {tagName}");
        }
    }
    else
    {
        Console.WriteLine("Tip is not tagged");
    }
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);