namespace Advisor.Api.Domain.Models;

public class Cliente
{
    public int Id { get; set; }
    public string Nome { get; set; } = default!;
    public Perfil Perfil { get; set; }
    public decimal AporteInicial { get; set; }     // R$
    public decimal LiquidezDesejada { get; set; }  // 0..1 (ex.: 0.3 = 30%)
    public string Objetivo { get; set; } = "";
    public DateTime? PrazoObjetivo { get; set; }
}
