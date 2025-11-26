using System.Text.Json;

namespace CodeShowcase.Api;

public sealed class FileCodeRepository : ICodeRepository
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public FileCodeRepository(string filePath)
    {
        _filePath = Path.GetFullPath(filePath);
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (!File.Exists(_filePath))
        {
            File.WriteAllText(_filePath, "[]");
        }
    }

    public async Task<IReadOnlyList<CodeEntry>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            await using var stream = File.OpenRead(_filePath);
            var entries = await JsonSerializer.DeserializeAsync<List<CodeEntry>>(stream, _jsonOptions, cancellationToken)
                          ?? new List<CodeEntry>();
            return entries.OrderByDescending(entry => entry.UpdatedAt).ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<CodeEntry?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        var items = await GetAllAsync(cancellationToken);
        return items.FirstOrDefault(entry => entry.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }

    public async Task AddAsync(CodeEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        await _lock.WaitAsync(cancellationToken);
        try
        {
            var current = (await ReadInternalAsync(cancellationToken)).ToList();
            var index = current.FindIndex(x => x.Id.Equals(entry.Id, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                current[index] = entry;
            }
            else
            {
                current.Add(entry);
            }

            await WriteInternalAsync(current, cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var current = (await ReadInternalAsync(cancellationToken)).ToList();
            var removed = current.RemoveAll(entry => entry.Id.Equals(id, StringComparison.OrdinalIgnoreCase)) > 0;

            if (removed)
            {
                await WriteInternalAsync(current, cancellationToken);
            }

            return removed;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<IReadOnlyList<CodeEntry>> ReadInternalAsync(CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(_filePath);
        return await JsonSerializer.DeserializeAsync<List<CodeEntry>>(stream, _jsonOptions, cancellationToken)
               ?? new List<CodeEntry>();
    }

    private async Task WriteInternalAsync(IEnumerable<CodeEntry> entries, CancellationToken cancellationToken)
    {
        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, entries, _jsonOptions, cancellationToken);
    }
}

