namespace Movix.Application.DTOs;

/// <summary>
/// Pedido de paginação padrão (valores serão saneados no serviço).
/// </summary>
public class PagedRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

public record PagedResult<T>(int Page, int PageSize, int Total, IReadOnlyList<T> Items);