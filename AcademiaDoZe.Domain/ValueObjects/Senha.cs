// Aline Duarte Sutil

namespace AcademiaDoZe.Domain.ValueObjects;

public record Senha
{
    public string Valor { get; }

    public Senha(string valor)
    {
        Valor = valor;
    }
}