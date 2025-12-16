using Octokit;

namespace service_csharp.Services;

public class GithubService
{
    private readonly GitHubClient _client;
    private readonly ILogger<GithubService> _logger;

    public GithubService(IConfiguration config, ILogger<GithubService> logger)
    {
        _logger = logger;
        _client = new GitHubClient(new ProductHeaderValue("Paradox-AI"));
        
        var token = config["GITHUB_TOKEN"];
        if (!string.IsNullOrEmpty(token))
        {
            _client.Credentials = new Credentials(token);
        }
        else
        {
            _logger.LogWarning("GITHUB_TOKEN is not configured. AI GitHub actions will fail.");
        }
    }

    public async Task CreateIssueCommentAsync(long repoId, int issueNumber, string comment)
    {
        try 
        {
            await _client.Issue.Comment.Create(repoId, issueNumber, comment);
            _logger.LogInformation("Commented on issue {IssueNumber} in repo {RepoId}", issueNumber, repoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error commenting on GitHub issue");
            throw;
        }
    }

    public async Task<PullRequest> CreatePullRequestAsync(long repoId, string title, string head, string baseRef, string body = "")
    {
        try
        {
            var prRequest = new NewPullRequest(title, head, baseRef) { Body = body };
            var pr = await _client.PullRequest.Create(repoId, prRequest);
            _logger.LogInformation("Created PR #{PrNumber} in repo {RepoId}", pr.Number, repoId);
            return pr;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Pull Request");
            throw;
        }
    }
}
