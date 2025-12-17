namespace TrackWay.Application.Common;

/// <summary>
/// Resultado paginado genérico
/// </summary>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedResult(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public static PagedResult<T> Create(IEnumerable<T> source, int pageNumber, int pageSize)
    {
        var count = source.Count();
        var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        return new PagedResult<T>(items, count, pageNumber, pageSize);
    }
}

/// <summary>
/// Request de paginación
/// </summary>
public record PagedRequest(int PageNumber = 1, int PageSize = 10)
{
    public int PageNumber { get; init; } = PageNumber < 1 ? 1 : PageNumber;
    public int PageSize { get; init; } = PageSize > 100 ? 100 : (PageSize < 1 ? 10 : PageSize);
}

/// <summary>
/// Response estándar de API
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ApiResponse<T> Ok(T data, string? message = null) => new()
    {
        Success = true,
        Data = data,
        Message = message
    };

    public static ApiResponse<T> Fail(string error) => new()
    {
        Success = false,
        Errors = new List<string> { error }
    };

    public static ApiResponse<T> Fail(IEnumerable<string> errors) => new()
    {
        Success = false,
        Errors = errors.ToList()
    };
}

/// <summary>
/// Alerta predictiva del sistema
/// </summary>
public record AlertaPredictivaDto(
    string Tipo,
    string Entidad,
    int EntidadId,
    string Mensaje,
    string Severidad, // Info, Warning, Critical
    DateTime? FechaVencimiento,
    int? DiasRestantes,
    string AccionRecomendada);
