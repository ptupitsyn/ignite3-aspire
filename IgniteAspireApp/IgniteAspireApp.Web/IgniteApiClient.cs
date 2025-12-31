namespace IgniteAspireApp.Web;

public class IgniteApiClient(HttpClient httpClient)
{
    public async Task<List<IgniteNode>> GetNodesAsync(int maxItems = 10, CancellationToken cancellationToken = default)
    {
        List<IgniteNode?> igniteNodes = await httpClient
            .GetFromJsonAsAsyncEnumerable<IgniteNode>("/nodes", cancellationToken)
            .ToListAsync(cancellationToken: cancellationToken);

        return igniteNodes!;
    }
}

public record IgniteNode(string Name, string Id, string Address);
