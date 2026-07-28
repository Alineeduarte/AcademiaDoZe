// Aline Duarte Sutil

using AcademiaDoZe.Domain.Enums;

namespace AcademiaDoZe.Domain.Entities;

public class Matricula : Entity
{
    public Aluno Aluno { get; private set; }
    public MatriculaPlano Plano { get; private set; }
    public DateOnly DataInicio { get; private set; }
    public DateOnly DataFinal { get; private set; }
    public string Objetivo { get; private set; }
    public MatriculaRestricoes Restricoes { get; private set; }
    public string? Observacoes { get; private set; }

    private Matricula(
        int id,
        Aluno aluno,
        MatriculaPlano plano,
        DateOnly dataInicio,
        DateOnly dataFinal,
        string objetivo,
        MatriculaRestricoes restricoes,
        string? observacoes)
        : base(id)
    {
        Aluno = aluno;
        Plano = plano;
        DataInicio = dataInicio;
        DataFinal = dataFinal;
        Objetivo = objetivo;
        Restricoes = restricoes;
        Observacoes = observacoes;
    }
}
