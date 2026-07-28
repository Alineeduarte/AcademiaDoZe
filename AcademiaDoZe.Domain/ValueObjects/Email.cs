// Aline Duarte Sutil

namespace AcademiaDoZe.Domain.ValueObjects;

public record Email
{
    public string Valor { get; }

    public Email(string valor)
    {
        Valor = valor;
    }
}