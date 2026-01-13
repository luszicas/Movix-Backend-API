namespace Movix.Domain.Entities;

public class Classificacao
{
    public int Id { get; set; }
    public string Nome { get; set; } = default!;
    public string? Descricao { get; set; }
    public ICollection<Filme> Filmes { get; set; } = new List<Filme>();
}
