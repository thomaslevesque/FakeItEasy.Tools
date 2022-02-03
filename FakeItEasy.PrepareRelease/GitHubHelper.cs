namespace FakeItEasy.PrepareRelease;

using Octokit;

internal static class GitHubHelper
{
    private const string RepoOwner = "FakeItEasy";
    private static string? repoName;

    public static string RepoName
    {
        get
        {
            if (string.IsNullOrEmpty(repoName))
            {
                throw new InvalidOperationException($"{nameof(RepoName)} is not set");
            }

            return repoName;
        }
        set => repoName = value;
    }

    public static bool IsPreRelease(string version)
    {
        return version.Contains('-', StringComparison.Ordinal);
    }

    public static async Task<Milestone> GetExistingMilestone(this IGitHubClient gitHubClient, string existingMilestoneTitle)
    {
        Console.WriteLine($"Fetching milestone '{existingMilestoneTitle}'...");
        var milestoneRequest = new MilestoneRequest { State = ItemStateFilter.Open };
        var existingMilestone = (await gitHubClient.Issue.Milestone.GetAllForRepository(RepoOwner, RepoName, milestoneRequest))
            .Single(milestone => milestone.Title == existingMilestoneTitle);
        Console.WriteLine($"Fetched milestone '{existingMilestone.Title}'");
        return existingMilestone;
    }

    public static async Task<IReadOnlyCollection<Release>> GetAllReleases(this IGitHubClient gitHubClient)
    {
        Console.WriteLine("Fetching all GitHub releases...");
        var allReleases = await gitHubClient.Repository.Release.GetAll(RepoOwner, RepoName);
        Console.WriteLine("Fetched all GitHub releases");
        return allReleases;
    }

    public static async Task<IList<Issue>> GetIssuesInMilestone(this IGitHubClient gitHubClient, Milestone milestone)
    {
        Console.WriteLine($"Fetching issues in milestone '{milestone.Title}'...'");
        var issueRequest = new RepositoryIssueRequest { Milestone = milestone.Number.ToString(), State = ItemStateFilter.All };
        var issues = (await gitHubClient.Issue.GetAllForRepository(RepoOwner, RepoName, issueRequest)).ToList();
        Console.WriteLine($"Fetched {issues.Count} issues in milestone '{milestone.Title}'");
        return issues;
    }

    public static Issue GetExistingReleaseIssue(IList<Issue> issues, string existingReleaseName)
    {
        var issue = issues.Single(i => i.Title == $"Release {existingReleaseName}");
        Console.WriteLine($"Found release issue #{issue.Number}: '{issue.Title}'");
        return issue;
    }

    public static async Task RenameMilestone(this IGitHubClient gitHubClient, Milestone existingMilestone, string version)
    {
        var milestoneUpdate = new MilestoneUpdate { Title = version };
        Console.WriteLine($"Renaming milestone '{existingMilestone.Title}' to '{milestoneUpdate.Title}'...");
        var updatedMilestone = await gitHubClient.Issue.Milestone.Update(RepoOwner, RepoName, existingMilestone.Number, milestoneUpdate);
        Console.WriteLine($"Renamed milestone '{existingMilestone.Title}' to '{updatedMilestone.Title}'");
    }

    public static async Task<Milestone> CreateNextMilestone(this IGitHubClient gitHubClient, string nextReleaseName)
    {
        var newMilestone = new NewMilestone(nextReleaseName);
        Console.WriteLine($"Creating new milestone '{newMilestone.Title}'...");
        var nextMilestone = await gitHubClient.Issue.Milestone.Create(RepoOwner, RepoName, newMilestone);
        Console.WriteLine($"Created new milestone '{nextMilestone.Title}'");
        return nextMilestone;
    }

    public static async Task UpdateRelease(this IGitHubClient gitHubClient, Release existingRelease, string version)
    {
        var releaseUpdate = new ReleaseUpdate { Name = version, TagName = version, Prerelease = IsPreRelease(version) };
        Console.WriteLine($"Renaming GitHub release '{existingRelease.Name}' to {releaseUpdate.Name}...");
        var updatedRelease = await gitHubClient.Repository.Release.Edit(RepoOwner, RepoName, existingRelease.Id, releaseUpdate);
        Console.WriteLine($"Renamed GitHub release '{existingRelease.Name}' to {updatedRelease.Name}");
    }

    public static async Task CreateNextRelease(this IGitHubClient gitHubClient, string nextReleaseName)
    {
        const string newReleaseBody = @"
### Changed

### New
* Issue Title (#12345)

### Fixed

### Additional Items

### With special thanks for contributions to this release from:
* Real Name - @githubhandle
";

        var newRelease = new NewRelease(nextReleaseName) { Draft = true, Name = nextReleaseName, Body = newReleaseBody.Trim() };
        Console.WriteLine($"Creating new GitHub release '{newRelease.Name}'...");
        var nextRelease = await gitHubClient.Repository.Release.Create(RepoOwner, RepoName, newRelease);
        Console.WriteLine($"Created new GitHub release '{nextRelease.Name}'");
    }

    public static async Task UpdateIssue(this IGitHubClient gitHubClient, Issue existingIssue, Milestone existingMilestone, string version)
    {
        var issueUpdate = new IssueUpdate { Title = $"Release {version}", Milestone = existingMilestone.Number };
        Console.WriteLine($"Renaming release issue '{existingIssue.Title}' to '{issueUpdate.Title}'...");
        var updatedIssue = await gitHubClient.Issue.Update(RepoOwner, RepoName, existingIssue.Number, issueUpdate);
        Console.WriteLine($"Renamed release issue '{existingIssue.Title}' to '{updatedIssue.Title}'");
    }

    public static async Task CreateNextIssue(this IGitHubClient gitHubClient, Issue existingIssue, Milestone nextMilestone, string nextReleaseName)
    {
        var newIssue = new NewIssue($"Release {nextReleaseName}")
        {
            Milestone = nextMilestone.Number,
            Body = existingIssue.Body.Replace("[x]", "[ ]", StringComparison.OrdinalIgnoreCase),
        };
        foreach (var label in existingIssue.Labels)
        {
            newIssue.Labels.Add(label.Name);
        }

        Console.WriteLine($"Creating new release issue '{newIssue.Title}'...");
        var nextIssue = await gitHubClient.Issue.Create(RepoOwner, RepoName, newIssue);
        Console.WriteLine($"Created new release issue #{nextIssue.Number}: '{newIssue.Title}'");
    }
}
