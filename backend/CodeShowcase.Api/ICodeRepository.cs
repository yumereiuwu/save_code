namespace CodeShowcase.Api;

public interface ICodeRepository
{
    Task<IReadOnlyList<CodeEntry>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CodeEntry?> GetAsync(string id, CancellationToken cancellationToken = default);
    Task AddAsync(CodeEntry entry, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}

