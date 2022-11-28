namespace FakeItEasy.Deploy;

internal class CommandLineOptions
{
    public string Repo { get; private set; } = string.Empty;
    public string TagName { get; private set; } = string.Empty;
    public string ArtifactsFolder { get; private set; } = string.Empty;

    public static CommandLineOptions Parse(string[] args)
    {
        Action<string>? nextArgAction = null;
        var options = new CommandLineOptions();
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
                    nextArgAction = value => options.Repo = value;
                    break;
                case "-t":
                case "--tag-name":
                    nextArgAction = value => options.TagName = value;
                    break;
                case "-a":
                case "--artifacts-folder":
                    nextArgAction = value => options.ArtifactsFolder = value;
                    break;
                default:
                    ShowUsage();
                    throw new Exception($"Unknown command line options '{arg}'");
            }
        }

        return options;
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
