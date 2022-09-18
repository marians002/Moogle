namespace MoogleEngine;

public class SearchItem
{
    public SearchItem(string title, string snippet, double score, string path)
    {
        this.Title = title;
        this.Snippet = snippet;
        this.Score = score;
        this.Path = path;
    }

    public string Title { get; private set; }

    public string Snippet { get; private set; }

    public double Score { get; private set; }

    public string Path {get; private set; }
}
