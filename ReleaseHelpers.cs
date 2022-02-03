namespace FakeItEasy.Tools;

using System.Globalization;
using System.Text.RegularExpressions;
using Octokit;

public static class ReleaseHelpers
{
    public static ICollection<int> GetIssueNumbersReferencedFromReleases(IEnumerable<Release> releases)
    {
        if (releases is null)
        {
            throw new ArgumentNullException(nameof(releases));
        }

        var issuesReferencedFromRelease = new HashSet<int>();
        foreach (var release in releases)
        {
            foreach (var capture in Regex.Matches(release.Body, @"\(\s*#(?<issueNumber>[0-9]+)(,\s*#(?<issueNumber>[0-9]+))*\s*\)")
                         .SelectMany(match => match.Groups["issueNumber"].Captures))
            {
                issuesReferencedFromRelease.Add(int.Parse(capture.Value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo));
            }
        }

        return issuesReferencedFromRelease;
    }
}
