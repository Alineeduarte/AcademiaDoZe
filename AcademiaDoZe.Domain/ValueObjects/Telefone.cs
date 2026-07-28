// Aline Duarte Sutil

namespace AcademiaDoZe.Domain.ValueObjects;

public record Telefone
{
    public string Valor { get; }

    public Telefone(string valor)
    {
        Valor = valor;
    }
}