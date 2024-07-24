using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Octokit;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            // Initialize configuration
            var appConfig = new AppConfiguration();

            // Extract owner and repo from URL
            var (owner, repo) = ExtractOwnerAndRepo(appConfig.RepoUrl);

            // Create a new GitHub client 
            var client = new GitHubClient(new ProductHeaderValue("LodashStatsApp"));

            // Set my personal access token for authentication
            var tokenAuth = new Credentials(appConfig.Token);
            client.Credentials = tokenAuth;

            // Get letter statistics for the specified repository
            var stats = await GetLetterStatisticsAsync(client, owner, repo);

            // Print the statistics in decreasing order of frequency
            foreach (var stat in stats.OrderByDescending(s => s.Value))
            {
                Console.WriteLine($"{stat.Key}: {stat.Value}");
            }
        }
        catch (Exception ex)
        {
            // Print any exceptions that occur to the console
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    static (string owner, string repo) ExtractOwnerAndRepo(string url)
    {
        // Regular expression to parse the owner and repo from the GitHub URL
        var match = Regex.Match(url, @"https:\/\/github\.com\/([^\/]+)\/([^\/]+)");
        if (!match.Success)
        {
            throw new ArgumentException("Invalid GitHub URL");
        }
        return (match.Groups[1].Value, match.Groups[2].Value);
    }

    static async Task<Dictionary<char, int>> GetLetterStatisticsAsync(GitHubClient client, string owner, string repo)
    {
        var fileStats = new Dictionary<char, int>();

        try
        {
            //var contents = await client.Repository.Content.GetAllContents(owner, repo);
            //var allJSFiles = contents.Where(c => c.Name.EndsWith(".js") || c.Name.EndsWith(".cjs") || c.Name.EndsWith(".ts")).ToList();


            // List all files in the repository
            var allJSFiles = await ListAllFilesAsync(client, owner, repo);

            foreach (var file in allJSFiles)
            {
                // Get the content of each JavaScript/TypeScript file
                var fileContent = await client.Repository.Content.GetAllContents(owner, repo, file);
                foreach (var content in fileContent)
                {
                    // Count the occurrences of each letter in the file content
                    foreach (var c in content.Content)
                    {
                        if (char.IsLetter(c))
                        {
                            if (!fileStats.ContainsKey(c))
                            {
                                fileStats[c] = 0;
                            }
                            fileStats[c]++;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Print any exceptions that occur to the console
            Console.WriteLine($"An error occurred while fetching file statistics: {ex.Message}");
        }

        return fileStats;
    }

    static async Task<List<string>> ListAllFilesAsync(GitHubClient client, string owner, string repo)
    {
        var filePaths = new List<string>();

        // Get repository content at the root
        var contents = await client.Repository.Content.GetAllContents(owner, repo);

        // Process the initial contents
        await ProcessDirectoryAsync(client, owner, repo, contents, filePaths);

        return filePaths;
    }

    static async Task ProcessDirectoryAsync(GitHubClient client, string owner, string repo, IReadOnlyList<RepositoryContent> contents, List<string> filePaths)
    {
        foreach (var content in contents)
        {
            if (content.Type == ContentType.Dir)
            {
                // Recursively process subdirectories
                var subContents = await client.Repository.Content.GetAllContents(owner, repo, content.Path);
                await ProcessDirectoryAsync(client, owner, repo, subContents, filePaths);
            }
            else if (content.Type == ContentType.File && (content.Name.EndsWith(".js") || content.Name.EndsWith(".cjs") || content.Name.EndsWith(".ts")))
            {
                // Add file paths for JavaScript and TypeScript files
                filePaths.Add(content.Path);
            }
        }
    }

}