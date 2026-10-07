
using System.Globalization;

namespace ControleFinanceiroMulti.Models;

public class Movimentacao
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public bool EhReceita { get; set; }

    public string Categoria { get; set; } = string.Empty;

    public override string ToString()
    {
        string tipo = EhReceita ? "Receita" : "Despesa";

        string valorFormatado = Valor.ToString(
            "C2",
            CultureInfo.GetCultureInfo("pt-BR")
        );

        return $"{Descricao} - {tipo} ({Categoria}): {valorFormatado}";
    }
}