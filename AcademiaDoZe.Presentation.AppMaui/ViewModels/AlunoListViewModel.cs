//Aline Duarte Sutil
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class AlunoListViewModel : BaseViewModel
{
    private readonly IAlunoService _alunoService;

    public ObservableCollection<string> FilterTypes { get; } =
        ["Nome", "CPF", "Id", "Email"];

    private ObservableCollection<AlunoDto> _alunos = [];

    public ObservableCollection<AlunoDto> Alunos
    {
        get => _alunos;
        set => SetProperty(ref _alunos, value);
    }

    private string _searchText = string.Empty;

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    private string _selectedFilterType = "Nome";

    public string SelectedFilterType
    {
        get => _selectedFilterType;
        set => SetProperty(ref _selectedFilterType, value);
    }

    public AlunoListViewModel(IAlunoService alunoService)
    {
        _alunoService = alunoService;
        Title = "Alunos";
    }

    [RelayCommand]
    private async Task AddAlunoAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("aluno");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Erro",
                $"Não foi possível abrir o cadastro: {ex.Message}",
                "OK");
        }
    }

    [RelayCommand]
    private async Task EditAlunoAsync(AlunoDto? aluno)
    {
        if (aluno == null)
            return;

        try
        {
            await Shell.Current.GoToAsync(
                $"aluno?Id={aluno.Id}");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Erro",
                $"Não foi possível editar o aluno: {ex.Message}",
                "OK");
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;

        try
        {
            await SearchAlunosAsync();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private Task LoadAlunosAsync()
    {
        return SearchAlunosAsync();
    }

    [RelayCommand]
    private async Task SearchAlunosAsync()
    {
        if (IsBusy)
            return;

        var termo = SearchText?.Trim() ?? string.Empty;

        if (termo.Length > 0 &&
            SelectedFilterType == "CPF" &&
            SomenteDigitos(termo).Length != 11)
        {
            await Shell.Current.DisplayAlertAsync(
                "Validação",
                "Informe os 11 dígitos do CPF.",
                "OK");
            return;
        }

        if (termo.Length > 0 &&
            SelectedFilterType == "Id" &&
            (!int.TryParse(termo, out int idValidado) ||
             idValidado <= 0))
        {
            await Shell.Current.DisplayAlertAsync(
                "Validação",
                "Informe um ID numérico válido.",
                "OK");
            return;
        }

        try
        {
            IsBusy = true;

            using var cts = new CancellationTokenSource(
                TimeSpan.FromSeconds(15));

            IEnumerable<AlunoDto> resultados;

            if (string.IsNullOrWhiteSpace(termo))
            {
                resultados =
                    await _alunoService.ObterTodosAsync(cts.Token)
                    ?? [];
            }
            else if (SelectedFilterType == "CPF")
            {
                var aluno = await _alunoService.ObterPorCpfAsync(
                    SomenteDigitos(termo), cts.Token);

                resultados = aluno == null ? [] : [aluno];
            }
            else if (SelectedFilterType == "Id")
            {
                var aluno = await _alunoService.ObterPorIdAsync(
                    int.Parse(termo), cts.Token);

                resultados = aluno == null ? [] : [aluno];
            }
            else if (SelectedFilterType == "Email")
            {
                var todos =
                    await _alunoService.ObterTodosAsync(cts.Token)
                    ?? [];

                resultados = todos.Where(a =>
                    a.Email?.Contains(
                        termo,
                        StringComparison.OrdinalIgnoreCase
                    ) == true);
            }
            else
            {
                var todos =
                    await _alunoService.ObterTodosAsync(cts.Token)
                    ?? [];

                resultados = todos.Where(a =>
                    a.Nome.Contains(
                        termo,
                        StringComparison.OrdinalIgnoreCase));
            }

            var lista = resultados.ToList();

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Alunos.Clear();

                foreach (var aluno in lista)
                {
                    Alunos.Add(aluno);
                }
            });
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync(
                "Tempo esgotado",
                "A consulta demorou demais. Verifique o banco de dados.",
                "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Erro",
                $"Erro ao buscar alunos: {ex.Message}",
                "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task DeleteAlunoAsync(AlunoDto? aluno)
    {
        if (aluno == null || IsBusy)
            return;

        bool confirmar = await Shell.Current.DisplayAlertAsync(
            "Confirmar exclusão",
            $"Deseja realmente excluir o aluno {aluno.Nome}?",
            "Sim",
            "Não");

        if (!confirmar)
            return;

        try
        {
            IsBusy = true;

            using var cts = new CancellationTokenSource(
                TimeSpan.FromSeconds(15));

            bool excluido = await _alunoService.RemoverAsync(
                aluno.Id,
                cts.Token);

            if (excluido)
            {
                Alunos.Remove(aluno);

                await Shell.Current.DisplayAlertAsync(
                    "Sucesso",
                    "Aluno excluído com sucesso!",
                    "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync(
                    "Aviso",
                    "Não foi possível excluir o aluno.",
                    "OK");
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync(
                "Tempo esgotado",
                "A exclusão excedeu o tempo esperado.",
                "OK");
        }
        catch (Exception ex)
        {
            bool relacionado =
                ex.Message.Contains(
                    "FOREIGN KEY",
                    StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains(
                    "constraint",
                    StringComparison.OrdinalIgnoreCase);

            await Shell.Current.DisplayAlertAsync(
                relacionado ? "Exclusão não permitida" : "Erro",
                relacionado
                    ? "O aluno possui registros vinculados e não pode ser excluído."
                    : $"Erro ao excluir aluno: {ex.Message}",
                "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static string SomenteDigitos(string valor)
    {
        return new string(valor.Where(char.IsDigit).ToArray());
    }
}
