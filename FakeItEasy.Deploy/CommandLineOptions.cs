namespace FakeItEasy.Deploy;

internal record CommandLineOptions(string Repo, string TagName, string ArtifactsFolder, bool DryRun)
{
    public static CommandLineOptions Parse(string[] args)
    {
        Action<string>? nextArgAction = null;
        string repo = string.Empty;
        string tagName = string.Empty;
        string artifactsFolder = string.Empty;
        bool dryRun = false;
        foreach (var arg in args)
        {
            if (nextArgAction is not null)
            {
                nextArgAction(arg);
                nextArgAction = null;
                continue;
            }

            switch (arg)
            {
                case "-r":
                case "--repo":
                    nextArgAction = value => repo = value;
                    break;
                case "-t":
                case "--tag-name":
                    nextArgAction = value => tagName = value;
                    break;
                case "-a":
                case "--artifacts-folder":
                    nextArgAction = value => artifactsFolder = value;
                    break;
                case "-d":
                case "--dry-run":
                    dryRun = true;
                    break;
                default:
                    ShowUsage();
                    throw new Exception($"Unknown command line options '{arg}'");
            }
        }

        return new CommandLineOptions(repo, tagName, artifactsFolder, dryRun);
    }

    public static void ShowUsage()
    {
        Console.WriteLine("Usage: <program> <options>");
        Console.WriteLine("  -r|--repo              [REQUIRED] Repository (owner/repo)");
        Console.WriteLine("  -t|--tag-name          [REQUIRED] Tag name");
        Console.WriteLine("  -a|--artifacts-folder  [REQUIRED] Artifacts folder");
    }

    public bool Validate()
    {
        var errors = new List<string>();
        if (string.IsNullOrEmpty(this.Repo))
        {
            errors.Add("Repository must be specified");
        }

        if (string.IsNullOrEmpty(this.TagName))
        {
            errors.Add("Tag name must be specified");
        }

        if (string.IsNullOrEmpty(this.ArtifactsFolder))
        {
            errors.Add("Artifacts folder must be specified");
        }

        if (errors.Any())
        {
            foreach (var error in errors)
            {
                Console.Error.WriteLine(error);
            }

            return false;
        }

        return true;
    }
}
