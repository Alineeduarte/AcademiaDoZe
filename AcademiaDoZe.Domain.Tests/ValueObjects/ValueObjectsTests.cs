//Aline Duarte Sutil
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;
namespace AcademiaDoZe.Domain.Tests.ValueObjects;

public class ValueObjectsTests
{
    [Theory(DisplayName = "Cep: dígitos inválidos -> CEP_DIGITOS")]
    [InlineData
    ("123")]
    [InlineData
    ("12-345")]
    public void Deve_Falhar_Criacao_Quando_CepDigitosInvalidos
    (string input)

    {
        var result = Cep
        .Criar
        (input);

        Assert
        .True
        (result.IsFailure);

        Assert
        .NotEmpty
        (result.Notifications);

    }
    [Theory(DisplayName = "Cep: formatos válidos (com e sem hífen)")]
    [InlineData
    ("12345-678")]
    [InlineData
    ("12345678")]
    public void Deve_Criar_Cep_Quando_Valido
    (string input)

    {
        var result = Cep
        .Criar
        (input);

        Assert
        .True
        (result.IsSuccess);

        Assert
        .Equal
        ("12345678", result.Value!.Valor);

    }
    [Theory(DisplayName = "Cep: obrigatório -> CEP_OBRIGATORIO")]
    [InlineData
    (null)]
    [InlineData
    ("")]
    public void Deve_Falhar_Criacao_Quando_CepNuloOuVazio
    (string? input)

    {
        var result = Cep
        .Criar
        (input!);

        Assert
        .True
        (result.IsFailure);

        Assert
        .Contains
        (result.Notifications,
        n =>
        n.Mensagem == "CEP_OBRIGATORIO");

    }
    [Theory(DisplayName = "Endereco: criação válida com número e complemento")]
    [InlineData
    ("10", "Bloco A")]
    [InlineData
    ("1", "")]
    public void Deve_Criar_Endereco_Quando_Valido

    (string numero, string complemento)

    {
        var logradouro = Logradouro

        .Criar(1, "12345-678", "Rua Teste", "Bairro", "Cidade", "SP", "Brasil").Value!;

        var result = Endereco
        .Criar
        (logradouro, numero, complemento);

        Assert
        .True
        (result.IsSuccess);

        Assert
        .Equal
        (logradouro.Id, result.Value!.LogradouroId);

        Assert
        .Equal
        (numero, result.Value.Numero);

        Assert
        .Equal
        (complemento, result.Value.Complemento);

    }
    [Theory(DisplayName = "Endereco: valida obrigatoriedade do logradouro e número")]
    [InlineData
    (null, "1", "LOGRADOURO_OBRIGATORIO")]
    [InlineData
    ("valid", "", "NUMERO_OBRIGATORIO")]
    public void Deve_Falhar_Criacao_Quando_EnderecoInvalido

    (string logradouroCase, string numero, string expected)

    {
        Logradouro? logradouro = null;
        if (logradouroCase == "valid")
            logradouro = Logradouro

            .Criar(1, "12345-678", "Rua Teste", "Bairro", "Cidade", "SP", "Brasil").Value!;

        var result = Endereco
        .Criar
        (logradouro!, numero, "");

        Assert
        .True
        (result.IsFailure);

        Assert
        .Contains
        (result.Notifications,
        n =>
        n.Mensagem == expected);

    }

    [Theory(DisplayName = "Cpf: nulo/vazio/espaços -> CPF_OBRIGATORIO")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Deve_Falhar_Criacao_Quando_CpfNuloOuVazio(string? input)
    {
        var result = Cpf.Criar(input!);
        Assert.True(result.IsFailure);
        Assert.Single(result.Notifications);
        Assert.Equal("CPF_OBRIGATORIO", result.Notifications.First().Mensagem);
    }
    [Theory(DisplayName = "Cpf: formatos válidos (com e sem pontuação)")]
    [InlineData("529.982.247-25")]
    [InlineData("52998224725")]
    public void Deve_Criar_Cpf_Quando_ValorValido(string input)
    {
        var result = Cpf.Criar(input);
        Assert.True(result.IsSuccess);
        Assert.Equal("52998224725", result.Value!.Valor);
    }
    [Theory(DisplayName = "Cpf: inválido - dígitos/verificador incorreto -> CPF_INVALIDO")]
    [InlineData("123.456.789-00")]
    [InlineData("111.111.111-11")]
    public void Deve_Falhar_Criacao_Quando_CpfInvalido(string input)
    {
        var result = Cpf.Criar(input);
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Notifications);
    }
    [Theory(DisplayName = "Cpf: sem dígitos -> CPF_DIGITOS")]
    [InlineData(" dfgdf ")]
    [InlineData("abc")]
    public void Deve_Falhar_Criacao_Quando_CpfSemDigitos(string input)
    {
        var result = Cpf.Criar(input);
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Notifications);
        Assert.Contains(result.Notifications, n => n.Mensagem == "CPF_DIGITOS");
    }
    [Theory(DisplayName = "Telefone: dígitos inválidos -> TELEFONE_DIGITOS")]
    [InlineData("1234")]
    [InlineData("(1)2345")]
    public void Deve_Falhar_Criacao_Quando_TelefoneDigitosInvalidos(string input)
    {
        var result = Telefone.Criar(input);
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Notifications);
    }
    [Theory(DisplayName = "Telefone: formatos válidos (com e sem formatação)")]
    [InlineData("(11) 91234-5678")]
    [InlineData("11912345678")]
    public void Deve_Criar_Telefone_Quando_Valido(string input)
    {
        var result = Telefone.Criar(input);
        Assert.True(result.IsSuccess);
        Assert.Equal("11912345678", result.Value!.Valor);
    }
    [Theory(DisplayName = "Telefone: obrigatório -> TELEFONE_OBRIGATORIO")]
    [InlineData(null)]
    [InlineData("")]
    public void Deve_Falhar_Criacao_Quando_TelefoneNuloOuVazio(string? input)
    {
        var result = Telefone.Criar(input!);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "TELEFONE_OBRIGATORIO");
    }
    [Theory(DisplayName = "Senha: valida requisito de uppercase")]
    [InlineData("abcdef", false)]
    [InlineData("Abcdef", true)]
    public void Deve_Validar_RequisitoUppercase_Senha(string senha, bool isSuccess)
    {
        var result = Senha.Criar(senha);
        Assert.Equal(isSuccess, result.IsSuccess);
    }
    [Theory(DisplayName = "Senha: obrigatório -> SENHA_OBRIGATORIO")]
    [InlineData(null)]
    [InlineData("")]
    public void Deve_Falhar_Criacao_Quando_SenhaNulaOuVazia(string? input)
    {
        var result = Senha.Criar(input!);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "SENHA_OBRIGATORIO");
    }
    [Theory(DisplayName = "Arquivo: criação válida")]
    [InlineData(1)]
    [InlineData(100)]
    public void Deve_Criar_Arquivo_Quando_Valido(int tamanho)
    {
        var conteudo = new byte[tamanho];

        var result = Arquivo.Criar(conteudo);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(tamanho, result.Value!.Conteudo.Length);
    }

    [Theory(DisplayName = "Email: remove espaços quando input tem espaços")]
    [InlineData(" usuario@teste.com ", "usuario@teste.com")]
    [InlineData(" teste@email.com ", "teste@email.com")]
    public void Deve_Criar_Email_E_RemoverEspacos_Quando_InputTemEspacos(
        string input, string esperado)
    {
        var result = Email.Criar(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(esperado, result.Value!.Valor);
    }

    [Theory(DisplayName = "Email: formato inválido -> EMAIL_FORMATO")]
    [InlineData("email")]
    [InlineData("email@")]
    [InlineData("@teste.com")]
    [InlineData("email@teste")]
    public void Deve_Falhar_Criacao_Quando_FormatoEmailInvalido(string input)
    {
        var result = Email.Criar(input);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Notifications,
            n => n.Mensagem == "EMAIL_FORMATO");
    }

    [Fact(DisplayName = "Arquivo: nulo -> ARQUIVO_OBRIGATORIO")]
    public void Deve_Falhar_Criacao_Quando_ArquivoNulo()
    {
        var result = Arquivo.Criar(null!);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Notifications,
            n => n.Mensagem == "ARQUIVO_OBRIGATORIO");
    }

    [Fact(DisplayName = "Arquivo: tamanho maior que 15MB -> ARQUIVO_TIPO_TAMANHO")]
    public void Deve_Falhar_Criacao_Quando_ArquivoMaiorQue15MB()
    {
        var conteudo = new byte[(15 * 1024 * 1024) + 1];

        var result = Arquivo.Criar(conteudo);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Notifications,
            n => n.Mensagem == "ARQUIVO_TIPO_TAMANHO");
    }

    [Theory(DisplayName = "Email: formatos válidos")]
    [InlineData("usuario@teste.com")]
    [InlineData("aline@exemplo.com.br")]
    public void Deve_Criar_Email_Quando_Valido(string input)
    {
        var result = Email.Criar(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(input, result.Value!.Valor);
    }

    [Theory(DisplayName = "Senha: formato inválido -> SENHA_FORMATO")]
    [InlineData("abcde")]
    [InlineData("abcdef")]
    public void Deve_Falhar_Criacao_Quando_SenhaFormatoInvalido(string input)
    {
        var result = Senha.Criar(input);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Notifications,
            n => n.Mensagem == "SENHA_FORMATO");
    }

    [Theory(DisplayName = "Cep: quantidade de dígitos inválida")]
    [InlineData("1234567")]
    [InlineData("123456789")]
    public void Deve_Falhar_Criacao_Quando_CepQuantidadeDigitosInvalida(string input)
    {
        var result = Cep.Criar(input);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Notifications,
            n => n.Mensagem == "CEP_DIGITOS");
    }

    [Theory(DisplayName = "Telefone: quantidade de dígitos inválida")]
    [InlineData("1191234567")]
    [InlineData("119123456789")]
    public void Deve_Falhar_Criacao_Quando_TelefoneQuantidadeDigitosInvalida(string input)
    {
        var result = Telefone.Criar(input);

        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Notifications,
            n => n.Mensagem == "TELEFONE_DIGITOS");
    }
}