// Aline Duarte Sutil

namespace AcademiaDoZe.Domain.ValueObjects;

public record Arquivo
{
    public string Nome { get; }
    public string Tipo { get; }
    public byte[] Conteudo { get; }

    public Arquivo(
        string nome,
        string tipo,
        byte[] conteudo)
    {
        Nome = nome;
        Tipo = tipo;
        Conteudo = conteudo;
    }
}