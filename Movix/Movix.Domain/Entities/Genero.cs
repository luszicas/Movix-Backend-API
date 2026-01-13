namespace Movix.Domain.Entities;

public class Genero
{
    public int Id { get; set; }
    public string Nome { get; set; } = default!;
    public ICollection<Filme> Filmes { get; set; } = new List<Filme>();
}
