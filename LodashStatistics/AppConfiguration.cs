using Microsoft.Extensions.Configuration;

public class AppConfiguration
{
    public string RepoUrl { get; set; } 
    public string Token { get; set; }


    public AppConfiguration()
    {
        // Build configuration to read from appsettings.json
        IConfigurationBuilder builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)  
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        IConfigurationRoot configuration = builder.Build();

        // Get the GitHub repository URL and token from the configuration
        RepoUrl = configuration["GitHub:RepoUrl"] ?? throw new ArgumentNullException(nameof(RepoUrl), "RepoURL cannot be null.");
        Token = configuration["GitHub:Token"] ?? throw new ArgumentNullException(nameof(Token), "Token cannot be null.");

    }
}