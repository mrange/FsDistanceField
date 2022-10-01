#nullable enable
#addin nuget:?package=Cake.Git&version=2.0.0

using System.Text.RegularExpressions;

var target        = Argument("target", "Test");
var configuration = Argument("configuration", "Release");
var slnPath       = "./src/FsDistanceField.sln";

var nugetApi      = "https://api.nuget.org/v3/index.json";
var nugetApiKey   = EnvironmentVariable("NUGET_API_KEY");
//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

record BuildData(
        string? Version
    );

Setup(ctx =>
    {
        var tip = GitLogTip(".");
        var tags = GitTags(".", true);
        var tipTag = tags
            .FirstOrDefault(tag => tag.Target.Sha == tip.Sha)
            ;
        string? version = null;

        if (tipTag is not null)
        {
            var tagName = tipTag.FriendlyName;
            var match   = Regex.Match(tagName, @"^v(?<version>\d+\.\d+\.\d+)$");
            if (match.Success)
            {
                version = match.Groups["version"].Value;
                Information($"Tip is tagged with version: {version}");
            }
            else
            {
                Warning($"Tip is tagged, but the tag doesn't match the version schema: {tagName}");
            }
        }
        else
        {
            Information("Tip is not tagged with version");
        }

        return new BuildData(version);
    });

Task("Clean")
    .WithCriteria(c => HasArgument("rebuild"))
    .Does(() =>
{
    DotNetClean(slnPath, new()
    {
        Configuration = configuration,
    });
});

Task("Build")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetBuild(slnPath, new()
    {
        Configuration = configuration,
    });
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    DotNetTest(slnPath, new()
    {
        Configuration = configuration,
        NoBuild       = true,
        NoRestore     = true,
    });
});

Task("Pack")
    .WithCriteria<BuildData>((ctx, bd) => bd.Version is not null)
    .IsDependentOn("Test")
    .Does<BuildData>((ctx, bd) =>
{
    var bs = new DotNetMSBuildSettings()
        .SetVersion(bd.Version)
        ;

    DotNetPack(slnPath, new()
    {
        Configuration   = configuration,
        NoBuild         = true,
        NoRestore       = true,
        MSBuildSettings = bs
    });
});

Task("PublishToNuGet")
    .WithCriteria<BuildData>((ctx, bd) => bd.Version is not null)
    .IsDependentOn("Pack")
    .Does<BuildData>((ctx, bd) =>
{
    var packPath = $"./src/FsDistanceField/nupkg/FsDistanceField.{bd.Version}.nupkg";
    Information($"Publishing package: {packPath}");
    DotNetNuGetPush(packPath, new()
    {
            ApiKey = nugetApiKey,
            Source = nugetApi
    });
});

Task("GithubAction")
    .IsDependentOn("PublishToNuGet")
    ;

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);