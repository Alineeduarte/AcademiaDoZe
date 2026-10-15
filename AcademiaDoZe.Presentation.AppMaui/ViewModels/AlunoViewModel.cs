//Aline Duarte Sutil
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

[QueryProperty(nameof(AlunoId), "Id")]
public partial class AlunoViewModel : BaseViewModel
{
    private readonly IAlunoService _alunoService;
    private readonly ILogradouroService _logradouroService;

    private AlunoDto _aluno = CriarAlunoPadrao();

    public AlunoDto Aluno
    {
        get => _aluno;
        set => SetProperty(ref _aluno, value);
    }

    private int _alunoId;

    public int AlunoId
    {
        get => _alunoId;
        set => SetProperty(ref _alunoId, value);
    }

    private bool _isEditMode;

    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    private string _confirmarSenha = string.Empty;

    public string ConfirmarSenha
    {
        get => _confirmarSenha;
        set => SetProperty(ref _confirmarSenha, value);
    }

    public bool HasEnderecoVinculado =>
        Aluno.Endereco?.Id > 0;

    public bool HasNoEnderecoVinculado =>
        !HasEnderecoVinculado;

    public AlunoViewModel(
        IAlunoService alunoService,
        ILogradouroService logradouroService)
    {
        _alunoService = alunoService;
        _logradouroService = logradouroService;
        Title = "Aluno";
    }

    private static AlunoDto CriarAlunoPadrao() => new()
    {
        Nome = string.Empty,
        Cpf = string.Empty,
        DataNascimento = DateOnly.FromDateTime(
            DateTime.Today.AddYears(-18)),
        Telefone = string.Empty,
        Email = string.Empty,
        Senha = string.Empty,
        Numero = string.Empty,
        Complemento = string.Empty,
        Endereco = NovoEndereco()
    };

    private static LogradouroDto NovoEndereco() => new()
    {
        Cep = string.Empty,
        Nome = string.Empty,
        Bairro = string.Empty,
        Cidade = string.Empty,
        Estado = string.Empty,
        Pais = "Brasil"
    };

    private void NotificarEndereco()
    {
        OnPropertyChanged(nameof(HasEnderecoVinculado));
        OnPropertyChanged(nameof(HasNoEnderecoVinculado));
    }

    public async Task InitializeAsync()
    {
        ConfirmarSenha = string.Empty;

        if (AlunoId > 0)
        {
            IsEditMode = true;
            Title = "Editar aluno";
            await LoadAlunoAsync();
        }
        else
        {
            IsEditMode = false;
            Title = "Novo aluno";
            Aluno = CriarAlunoPadrao();
            NotificarEndereco();
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    public async Task LoadAlunoAsync()
    {
        if (AlunoId <= 0 || IsBusy)
            return;

        try
        {
            IsBusy = true;

            using var cts = new CancellationTokenSource(
                TimeSpan.FromSeconds(15));

            var resultado = await _alunoService.ObterPorIdAsync(
                AlunoId,
                cts.Token);

            if (resultado == null)
            {
                await Shell.Current.DisplayAlertAsync(
                    "Aviso",
                    "Aluno não encontrado.",
                    "OK");
                return;
            }

            PrepararAlunoParaEdicao(resultado);
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync(
                "Tempo esgotado",
                "Não foi possível carregar o aluno a tempo.",
                "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Erro",
                $"Erro ao carregar aluno: {ex.Message}",
                "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void PrepararAlunoParaEdicao(AlunoDto resultado)
    {
        resultado.Endereco ??= NovoEndereco();

        // A senha atual não deve aparecer na tela.
        resultado.Senha = string.Empty;
        ConfirmarSenha = string.Empty;

        Aluno = resultado;
        AlunoId = resultado.Id;
        IsEditMode = true;
        Title = "Editar aluno";

        NotificarEndereco();
    }

    [RelayCommand]
    public async Task SearchByCpfAsync()
    {
        var cpf = SomenteDigitos(Aluno.Cpf);

        if (cpf.Length != 11)
        {
            await Shell.Current.DisplayAlertAsync(
                "Validação",
                "Informe os 11 dígitos do CPF.",
                "OK");
            return;
        }

        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            using var cts = new CancellationTokenSource(
                TimeSpan.FromSeconds(15));

            var encontrado = await _alunoService.ObterPorCpfAsync(
                cpf,
                cts.Token);

            if (encontrado == null)
            {
                await Shell.Current.DisplayAlertAsync(
                    "CPF não encontrado",
                    "Você pode continuar o cadastro deste aluno.",
                    "OK");
                return;
            }

            // Carrega o endereço completo do aluno.
            var completo = await _alunoService.ObterPorIdAsync(
                encontrado.Id,
                cts.Token);

            PrepararAlunoParaEdicao(completo ?? encontrado);

            await Shell.Current.DisplayAlertAsync(
                "Aluno encontrado",
                "Os dados foram carregados para edição.",
                "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Erro",
                $"Erro ao buscar CPF: {ex.Message}",
                "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task SearchByCepAsync()
    {
        var cep = SomenteDigitos(
            Aluno.Endereco?.Cep ?? string.Empty);

        if (cep.Length != 8)
        {
            await Shell.Current.DisplayAlertAsync(
                "Validação",
                "Informe um CEP com 8 dígitos.",
                "OK");
            return;
        }

        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            using var cts = new CancellationTokenSource(
                TimeSpan.FromSeconds(15));

            var endereco = await _logradouroService.ObterPorCepAsync(
                cep,
                cts.Token);

            if (endereco == null)
            {
                await Shell.Current.DisplayAlertAsync(
                    "CEP não encontrado",
                    "Cadastre primeiro o logradouro na tela Logradouros.",
                    "OK");
                return;
            }

            Aluno.Endereco = endereco;

            OnPropertyChanged(nameof(Aluno));
            NotificarEndereco();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Erro",
                $"Erro ao buscar CEP: {ex.Message}",
                "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task SaveAlunoAsync()
    {
        if (IsBusy || !await ValidarAlunoAsync())
            return;

        try
        {
            IsBusy = true;

            using var cts = new CancellationTokenSource(
                TimeSpan.FromSeconds(15));

            // Verifica se o logradouro já está cadastrado.
            if (Aluno.Endereco is null ||
                Aluno.Endereco.Id <= 0)
            {
                var cep = SomenteDigitos(
                    Aluno.Endereco?.Cep ?? string.Empty);

                var endereco = await _logradouroService.ObterPorCepAsync(
                    cep,
                    cts.Token);

                if (endereco == null)
                {
                    await Shell.Current.DisplayAlertAsync(
                        "Endereço obrigatório",
                        "Cadastre o logradouro e depois clique em Buscar CEP.",
                        "OK");
                    return;
                }

                Aluno.Endereco = endereco;
                OnPropertyChanged(nameof(Aluno));
                NotificarEndereco();
            }

            // Normalização dos dados.
            Aluno.Nome = Aluno.Nome.Trim();
            Aluno.Cpf = SomenteDigitos(Aluno.Cpf);
            Aluno.Telefone = SomenteDigitos(Aluno.Telefone);
            Aluno.Numero = Aluno.Numero.Trim();

            Aluno.Email = string.IsNullOrWhiteSpace(Aluno.Email)
                ? null : Aluno.Email.Trim();

            Aluno.Complemento = string.IsNullOrWhiteSpace(
                Aluno.Complemento)
                ? null : Aluno.Complemento.Trim();

            Aluno.Senha = string.IsNullOrWhiteSpace(Aluno.Senha)
                ? null : Aluno.Senha.Trim();

            if (IsEditMode)
            {
                await _alunoService.AtualizarAsync(
                    Aluno,
                    cts.Token);

                await Shell.Current.DisplayAlertAsync(
                    "Sucesso",
                    "Aluno atualizado com sucesso!",
                    "OK");
            }
            else
            {
                await _alunoService.AdicionarAsync(
                    Aluno,
                    cts.Token);

                await Shell.Current.DisplayAlertAsync(
                    "Sucesso",
                    "Aluno cadastrado com sucesso!",
                    "OK");
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync(
                "Tempo esgotado",
                "Não foi possível salvar a tempo.",
                "OK");
        }
        catch (InvalidOperationException ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Regra de negócio",
                ex.Message,
                "OK");
        }
        catch (ArgumentException ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Validação",
                ex.Message,
                "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Erro",
                $"Erro ao salvar aluno: {ex.Message}",
                "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task SelecionarFotoAsync()
    {
        try
        {
            var escolha = await Shell.Current.DisplayActionSheetAsync(
                "Foto do aluno",
                "Cancelar",
                "Remover foto",
                "Galeria",
                "Câmera");

            if (escolha == "Remover foto")
            {
                Aluno.Foto = null;
                OnPropertyChanged(nameof(Aluno));
                return;
            }

            FileResult? arquivo = null;

            if (escolha == "Galeria")
            {
                arquivo = await FilePicker.Default.PickAsync(
                    new PickOptions
                    {
                        PickerTitle = "Selecione uma imagem",
                        FileTypes = FilePickerFileType.Images
                    });
            }
            else if (escolha == "Câmera")
            {
                if (!MediaPicker.Default.IsCaptureSupported)
                {
                    await Shell.Current.DisplayAlertAsync(
                        "Câmera indisponível",
                        "Este dispositivo não suporta a captura de fotos.",
                        "OK");
                    return;
                }

                arquivo = await MediaPicker.Default.CapturePhotoAsync();
            }

            if (arquivo == null)
                return;

            using var stream = await arquivo.OpenReadAsync();
            using var memoria = new MemoryStream();

            await stream.CopyToAsync(memoria);

            Aluno.Foto = new ArquivoDto
            {
                Conteudo = memoria.ToArray()
            };

            OnPropertyChanged(nameof(Aluno));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Erro",
                $"Não foi possível obter a imagem: {ex.Message}",
                "OK");
        }
    }

    private async Task<bool> ValidarAlunoAsync()
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(Aluno.Nome))
            erros.Add("• Nome é obrigatório.");

        var cpf = SomenteDigitos(Aluno.Cpf);

        if (cpf.Length != 11)
            erros.Add("• CPF precisa ter 11 dígitos.");

        if (Aluno.DataNascimento == default ||
            Aluno.DataNascimento >
            DateOnly.FromDateTime(DateTime.Today))
        {
            erros.Add("• Informe uma data de nascimento válida.");
        }

        var telefone = SomenteDigitos(Aluno.Telefone);

        if (telefone.Length != 11)
            erros.Add("• Telefone deve ter DDD e 11 dígitos.");

        if (!string.IsNullOrWhiteSpace(Aluno.Email) &&
            !System.Net.Mail.MailAddress.TryCreate(
                Aluno.Email.Trim(), out _))
        {
            erros.Add("• E-mail inválido.");
        }

        // Senha obrigatória no cadastro e opcional na edição.
        if (!IsEditMode ||
            !string.IsNullOrWhiteSpace(Aluno.Senha))
        {
            var senha = Aluno.Senha ?? string.Empty;

            if (senha.Trim().Length < 6 ||
                !senha.Any(char.IsUpper))
            {
                erros.Add(
                    "• Senha: mínimo de 6 caracteres e uma letra maiúscula.");
            }

            if (senha != ConfirmarSenha)
            {
                erros.Add("• A confirmação da senha não confere.");
            }
        }

        if (Aluno.Endereco == null ||
            SomenteDigitos(Aluno.Endereco.Cep).Length != 8)
        {
            erros.Add("• Informe o CEP do endereço.");
        }

        if (string.IsNullOrWhiteSpace(Aluno.Numero))
            erros.Add("• Número do endereço é obrigatório.");

        // Foto exigida para a demonstração da atividade.
        if (Aluno.Foto?.Conteudo is not { Length: > 0 })
        {
            erros.Add("• Selecione uma foto do aluno.");
        }

        if (erros.Count == 0)
            return true;

        await Shell.Current.DisplayAlertAsync(
            "Corrija os dados",
            string.Join("\n", erros),
            "OK");

        return false;
    }

    private static string SomenteDigitos(string? valor)
    {
        return new string(
            (valor ?? string.Empty)
                .Where(char.IsDigit)
                .ToArray());
    }
}
