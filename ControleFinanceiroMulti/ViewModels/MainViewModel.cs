
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ControleFinanceiroMulti.Data;
using ControleFinanceiroMulti.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;

namespace ControleFinanceiroMulti.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly CultureInfo _cultura = CultureInfo.GetCultureInfo("pt-BR");

    private decimal _saldoAtual;
    private decimal _totalReceitas;
    private decimal _totalDespesas;

    [ObservableProperty]
    public partial string Descricao { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ValorTexto { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int TipoSelecionado { get; set; } = 0;

    [ObservableProperty]
    public partial string CategoriaSelecionada { get; set; } = "Outros";

    public ObservableCollection<string> Categorias { get; } = new()
    {
        "Alimentação",
        "Transporte",
        "Moradia",
        "Saúde",
        "Lazer",
        "Salário",
        "Investimentos",
        "Outros"
    };

    [ObservableProperty]
    public partial string SaldoAtual { get; set; } = "R$ 0,00";

    [ObservableProperty]
    public partial string TotalReceitas { get; set; } = "R$ 0,00";

    [ObservableProperty]
    public partial string TotalDespesas { get; set; } = "R$ 0,00";

    [ObservableProperty]
    public partial string Mensagem { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ObservableCollection<Movimentacao> Movimentacoes { get; set; } = new();

    [ObservableProperty]
    public partial Movimentacao? MovimentacaoSelecionada { get; set; }

    public MainViewModel()
    {
        CarregarMovimentacoes();
    }

    private void CarregarMovimentacoes()
    {
        try
        {
            using var db = new AppDbContext();

            var registros = db.Movimentacoes
                .AsNoTracking()
                .OrderBy(m => m.Id)
                .ToList();

            foreach (var movimentacao in registros)
            {
                Movimentacoes.Add(movimentacao);

                if (movimentacao.EhReceita)
                {
                    _totalReceitas += movimentacao.Valor;
                    _saldoAtual += movimentacao.Valor;
                }
                else
                {
                    _totalDespesas += movimentacao.Valor;
                    _saldoAtual -= movimentacao.Valor;
                }
            }

            AtualizarTotais();
        }
        catch (Exception ex)
        {
            Mensagem = $"Erro ao carregar movimentações: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task Cadastrar()
    {
        if (string.IsNullOrWhiteSpace(Descricao))
        {
            Mensagem = "Digite uma descrição.";
            return;
        }

        if (!decimal.TryParse(
                ValorTexto,
                NumberStyles.Number,
                _cultura,
                out decimal valor) || valor <= 0)
        {
            Mensagem = "Digite um valor válido maior que zero.";
            return;
        }

        if (string.IsNullOrWhiteSpace(CategoriaSelecionada))
        {
            Mensagem = "Selecione uma categoria.";
            return;
        }

        bool ehReceita = TipoSelecionado == 1;

        var movimentacao = new Movimentacao
        {
            Descricao = Descricao.Trim(),
            Valor = valor,
            EhReceita = ehReceita,
            Categoria = CategoriaSelecionada
        };

        try
        {
            using var db = new AppDbContext();

            db.Movimentacoes.Add(movimentacao);
            await db.SaveChangesAsync();

            Movimentacoes.Add(movimentacao);

            if (ehReceita)
            {
                _totalReceitas += valor;
                _saldoAtual += valor;
            }
            else
            {
                _totalDespesas += valor;
                _saldoAtual -= valor;
            }

            AtualizarTotais();

            Mensagem = "Movimentação cadastrada com sucesso!";

            Descricao = string.Empty;
            ValorTexto = string.Empty;
            TipoSelecionado = 0;
            CategoriaSelecionada = "Outros";
        }
        catch (Exception ex)
        {
            Mensagem = $"Erro ao salvar movimentação: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task Remover()
    {
        if (MovimentacaoSelecionada is null)
        {
            Mensagem = "Selecione uma movimentação para remover.";
            return;
        }

        var movimentacao = MovimentacaoSelecionada;

        try
        {
            using var db = new AppDbContext();

            db.Movimentacoes.Remove(movimentacao);
            await db.SaveChangesAsync();

            if (movimentacao.EhReceita)
            {
                _totalReceitas -= movimentacao.Valor;
                _saldoAtual -= movimentacao.Valor;
            }
            else
            {
                _totalDespesas -= movimentacao.Valor;
                _saldoAtual += movimentacao.Valor;
            }

            Movimentacoes.Remove(movimentacao);
            MovimentacaoSelecionada = null;

            AtualizarTotais();

            Mensagem = "Movimentação removida.";
        }
        catch (Exception ex)
        {
            Mensagem = $"Erro ao remover movimentação: {ex.Message}";
        }
    }

    private void AtualizarTotais()
    {
        TotalReceitas = _totalReceitas.ToString("C2", _cultura);
        TotalDespesas = _totalDespesas.ToString("C2", _cultura);
        SaldoAtual = _saldoAtual.ToString("C2", _cultura);
    }
}