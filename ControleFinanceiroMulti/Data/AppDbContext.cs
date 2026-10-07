
using ControleFinanceiroMulti.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace ControleFinanceiroMulti.Data;

public class AppDbContext : DbContext
{
    public DbSet<Movimentacao> Movimentacoes => Set<Movimentacao>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        string pastaDados = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData
            ),
            "ControleFinanceiroMulti"
        );

        Directory.CreateDirectory(pastaDados);

        string caminhoBanco = Path.Combine(
            pastaDados,
            "controlefinanceiro.db"
        );

        options.UseSqlite($"Data Source={caminhoBanco}");
    }

    public void AtualizarBanco()
    {
        Database.OpenConnection();

        try
        {
            bool colunaExiste = false;

            using (var comando = Database.GetDbConnection().CreateCommand())
            {
                comando.CommandText = "PRAGMA table_info('Movimentacoes');";

                using var leitor = comando.ExecuteReader();

                while (leitor.Read())
                {
                    string nomeColuna = leitor["name"].ToString() ?? "";

                    if (nomeColuna == "Categoria")
                    {
                        colunaExiste = true;
                        break;
                    }
                }
            }

            if (!colunaExiste)
            {
                using var comando = Database.GetDbConnection().CreateCommand();

                comando.CommandText =
                    "ALTER TABLE Movimentacoes " +
                    "ADD COLUMN Categoria TEXT NOT NULL DEFAULT 'Outros';";

                comando.ExecuteNonQuery();
            }
        }
        finally
        {
            Database.CloseConnection();
        }
    }
}