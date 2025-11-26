namespace CodeShowcase.Api;

public sealed record CodeEntry(
    string Id,
    string Title,
    string Language,
    string Content,
    DateTimeOffset UpdatedAt);

