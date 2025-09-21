using System.Text.Json.Serialization;

namespace Advisor.Api.Domain.Models
{
    public class Carteira
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; } = default!;
        public DateTime CriadaEm { get; set; } = DateTime.UtcNow;
        public List<Posicao> Posicoes { get; set; } = new();
        public string Explicacao { get; set; } = "";
    }

    public class Posicao
    {
        public int Id { get; set; }
        public int CarteiraId { get; set; }

        [JsonIgnore]                      // 👈 evita o ciclo Carteira→Posições→Carteira
        public Carteira Carteira { get; set; } = default!;

        public int AtivoId { get; set; }
        public Ativo Ativo { get; set; } = default!;
        public decimal Percentual { get; set; } // 0..1
    }
}
