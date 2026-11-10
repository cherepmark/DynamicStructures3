namespace DynamicStructures.Infrastructure;

/// <summary>Поиск корня репозитория и стандартных каталогов независимо от каталога запуска.</summary>
public static class ProjectPaths
{
    private static readonly Lazy<string> RootLazy = new(FindRepositoryRootCore);

    public static string RepositoryRoot => RootLazy.Value;

    public static string DataDirectory => Path.Combine(RepositoryRoot, "data");

    public static string ScenariosDirectory => Path.Combine(DataDirectory, "scenarios");

    public static string ReportsDirectory => Path.Combine(RepositoryRoot, "Reports");

    public static string InputFile => Path.Combine(RepositoryRoot, "input.txt");

    public static string FindRepositoryRoot() => RepositoryRoot;

    public static string ResolveExistingFile(params string[] candidates)
    {
        if (candidates.Length == 0)
        {
            throw new ArgumentException("Не передано ни одного имени файла.", nameof(candidates));
        }

        var searched = new List<string>();
        foreach (var candidate in candidates)
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                continue;
            }

            if (Path.IsPathRooted(candidate))
            {
                searched.Add(candidate);
                if (File.Exists(candidate))
                {
                    return Path.GetFullPath(candidate);
                }

                continue;
            }

            var bases = new[]
            {
                Directory.GetCurrentDirectory(),
                AppContext.BaseDirectory,
                RepositoryRoot,
                DataDirectory,
                ScenariosDirectory
            };

            foreach (var baseDirectory in bases)
            {
                var fullPath = Path.GetFullPath(Path.Combine(baseDirectory, candidate));
                searched.Add(fullPath);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }
        }

        throw new FileNotFoundException(
            "Файл не найден. Проверены пути:" + Environment.NewLine +
            string.Join(Environment.NewLine, searched.Distinct(StringComparer.OrdinalIgnoreCase)),
            candidates[0]);
    }

    public static void EnsureDirectories()
    {
        Directory.CreateDirectory(DataDirectory);
        Directory.CreateDirectory(ScenariosDirectory);
        Directory.CreateDirectory(ReportsDirectory);
    }

    private static string FindRepositoryRootCore()
    {
        var starts = new[]
        {
            Directory.GetCurrentDirectory(),
            AppContext.BaseDirectory
        };

        foreach (var start in starts)
        {
            var directory = new DirectoryInfo(start);
            while (directory is not null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "DynamicStructures.sln")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }
        }

        return Directory.GetCurrentDirectory();
    }
}
