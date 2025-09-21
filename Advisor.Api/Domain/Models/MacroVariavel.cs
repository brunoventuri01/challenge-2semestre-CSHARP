namespace Advisor.Api.Domain.Models;

public class MacroVariavel
{
    public int Id { get; set; }
    public decimal Selic { get; set; }     // 0.13 = 13%
    public decimal Inflacao { get; set; }  // 0.05 = 5%
    public decimal Cambio { get; set; }    // R$/US$
    public DateTime Vigencia { get; set; }
}
