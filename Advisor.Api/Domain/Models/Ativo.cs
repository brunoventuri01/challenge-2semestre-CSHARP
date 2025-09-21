namespace Advisor.Api.Domain.Models
{
    public class Ativo
    {
        public int Id { get; set; }              // PK para o EF Core
        public string Codigo { get; set; } = ""; // Ex: "TESOURO_SELIC"
        public string Nome { get; set; } = "";   // Ex: "Tesouro Selic 2027"
        public int Classe { get; set; }          // 0=renda fixa, 1=renda variável, etc.
        public double Risco { get; set; }        // ex: 0.05
        public double Liquidez { get; set; }     // ex: 0.95
        public double RetornoEsperado { get; set; } // ex: 0.11
        public bool ESG { get; set; }            // true/false
    }
}
