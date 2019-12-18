namespace FakeItEasy.Tools
{
    using System;
    using System.IO;

    using static FakeItEasy.Tools.ToolHelpers;

    public static class GitHubTokenSource
    {
        public static string GetAccessToken()
        {
            var tokenFilePath = Path.Combine(GetCurrentScriptDirectory(), ".githubtoken");
            var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
            if (string.IsNullOrEmpty(token))
            {
                if (File.Exists(tokenFilePath))
                {
                    token = File.ReadAllText(tokenFilePath)?.Trim();
                }
            }

            if (string.IsNullOrEmpty(token))
            {
                throw new Exception($"GitHub access token is missing; please put it in '{tokenFilePath}', or in the GITHUB_TOKEN environment variable.");
            }

            return token;
        }
    }
}
