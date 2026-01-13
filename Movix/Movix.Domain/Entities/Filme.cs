using System.ComponentModel.DataAnnotations;
using Movix.Domain.Entities;

namespace Movix.Domain.Entities;

public class Filme
{
    public int Id { get; set; }
    public string Titulo { get; set; } = default!;
    public string? Sinopse { get; set; }
    public int Ano { get; set; }
    public string? ImagemCapaUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int GeneroId { get; set; }
    public Genero Genero { get; set; } = default!;

    public int ClassificacaoId { get; set; }
    public Classificacao Classificacao { get; set; } = default!;

    [MaxLength(1000)]
    public string? UrlTrailer { get; set; }

}