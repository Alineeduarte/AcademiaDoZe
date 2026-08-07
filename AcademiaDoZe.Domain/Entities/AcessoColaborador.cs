// Aline Duarte Sutil

using AcademiaDoZe.Domain.Common;

namespace AcademiaDoZe.Domain.Entities;

public class AcessoColaborador : Entity
{
    public Colaborador Colaborador { get; private set; }

    public DateTime DataHoraEntrada { get; private set; }

    public DateTime? DataHoraSaida { get; private set; }

    private AcessoColaborador(
        int id,
        Colaborador colaborador,
        DateTime dataHoraEntrada,
        DateTime? dataHoraSaida)
        : base(id)
    {
        Colaborador = colaborador;
        DataHoraEntrada = dataHoraEntrada;
        DataHoraSaida = dataHoraSaida;
    }

    public static Result<AcessoColaborador> Criar(
        int id,
        Colaborador colaborador,
        DateTime dataHoraEntrada,
        DateTime? dataHoraSaida = null)
    {
        var notifications =
            new List<Notification>();

        if (colaborador == null)
        {
            notifications.Add(
                new Notification(
                    "Colaborador",
                    "COLABORADOR_OBRIGATORIO"));
        }

        if (dataHoraEntrada == default)
        {
            notifications.Add(
                new Notification(
                    "DataHoraEntrada",
                    "DATA_HORA_ENTRADA_OBRIGATORIA"));
        }

        if (dataHoraSaida.HasValue &&
            dataHoraEntrada != default &&
            dataHoraSaida.Value < dataHoraEntrada)
        {
            notifications.Add(
                new Notification(
                    "DataHoraSaida",
                    "DATA_HORA_SAIDA_INVALIDA"));
        }

        if (notifications.Count != 0)
        {
            return Result<AcessoColaborador>.Failure(
                notifications);
        }

        var acesso =
            new AcessoColaborador(
                id,
                colaborador!,
                dataHoraEntrada,
                dataHoraSaida);

        return Result<AcessoColaborador>.Success(acesso);
    }

    public TimeSpan? TempoPermanencia()
    {
        if (!DataHoraSaida.HasValue)
            return null;

        return DataHoraSaida.Value -
               DataHoraEntrada;
    }
}