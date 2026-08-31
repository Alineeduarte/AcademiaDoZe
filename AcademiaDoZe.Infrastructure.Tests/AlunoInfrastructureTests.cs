//Aline Duarte Sutil

using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Repositories;

namespace AcademiaDoZe.Infrastructure.Tests;

public class AlunoInfrastructureTests : TestBase
{
    private readonly AlunoRepository _repository;
    private readonly LogradouroRepository _logradouroRepository;

    public AlunoInfrastructureTests()
    {
        _repository = new AlunoRepository(ConnectionString, DatabaseType);
        _logradouroRepository = new LogradouroRepository(ConnectionString, DatabaseType);
    }

    // Senha muda automaticamente conforme o SGBD selecionado no TestBase
    private string ObterSenhaTeste()
    {
        return DatabaseType switch
        {
            Infrastructure.Data.DatabaseType.Sqlite => "AbcSQLite123",
            Infrastructure.Data.DatabaseType.SqlServer => "AbcSQLServer123",
            Infrastructure.Data.DatabaseType.MySql => "AbcMySQL123",
            _ => "AbcTeste123"
        };
    }

    private async Task<Logradouro> CriarEInserirLogradouroAsync()
    {
        var result = Logradouro.Criar(
            0,
            GerarCep(),
            "Rua Teste Aluno",
            "Centro",
            "Anita Garibaldi",
            "SC",
            "Brasil"
        );

        Assert.True(result.IsSuccess);

        return await _logradouroRepository.Adicionar(result.Value!);
    }

    public async Task<Aluno> CriarEInserirAlunoAsync()
    {
        var logradouro = await CriarEInserirLogradouroAsync();

        var result = Aluno.Criar(
            0,
            "Aline",
            GerarCpf(),
            DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
            GerarTelefone(),
            GerarEmail(),
            logradouro,
            "100",
            "Duarte Sutil",
            ObterSenhaTeste(),
            Arquivo.Criar(new byte[] { 1, 2, 3 }).Value!
        );

        Assert.True(result.IsSuccess);

        return await _repository.Adicionar(result.Value!);
    }

    [Fact]
    public async Task Deve_Adicionar_Aluno()
    {
        var logradouro = await CriarEInserirLogradouroAsync();

        var result = Aluno.Criar(
            0,
            "Aline",
            GerarCpf(),
            DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
            GerarTelefone(),
            GerarEmail(),
            logradouro,
            "101",
            "Duarte Sutil",
            ObterSenhaTeste(),
            Arquivo.Criar(new byte[] { 1, 2, 3 }).Value!
        );

        Assert.True(result.IsSuccess);

        var aluno = await _repository.Adicionar(result.Value!);

        Assert.NotNull(aluno);
        Assert.True(aluno.Id > 0);
        Assert.Equal("Aline", aluno.Nome);
        Assert.Equal("Duarte Sutil", aluno.Endereco.Complemento);
        Assert.Equal(ObterSenhaTeste(), aluno.Senha.Valor);
    }

    [Fact]
    public async Task Deve_Obter_Aluno_Por_Id()
    {
        var aluno = await CriarEInserirAlunoAsync();

        var encontrado = await _repository.ObterPorId(aluno.Id);

        Assert.NotNull(encontrado);
        Assert.Equal(aluno.Id, encontrado.Id);
        Assert.Equal(aluno.Cpf.Valor, encontrado.Cpf.Valor);
        Assert.Equal("Aline", encontrado.Nome);
        Assert.Equal("Duarte Sutil", encontrado.Endereco.Complemento);
        Assert.Equal(ObterSenhaTeste(), encontrado.Senha.Valor);
    }

    [Fact]
    public async Task Deve_Obter_Todos_Alunos()
    {
        await CriarEInserirAlunoAsync();
        await CriarEInserirAlunoAsync();

        var alunos = await _repository.ObterTodos();

        Assert.NotNull(alunos);
        Assert.NotEmpty(alunos);
    }

    [Fact]
    public async Task Deve_Obter_Aluno_Por_Cpf()
    {
        var aluno = await CriarEInserirAlunoAsync();

        var encontrado = await _repository.ObterPorCpf(aluno.Cpf);

        Assert.NotNull(encontrado);
        Assert.Equal(aluno.Cpf.Valor, encontrado.Cpf.Valor);
        Assert.Equal("Aline", encontrado.Nome);
    }

    [Fact]
    public async Task Deve_Obter_Aluno_Por_Email()
    {
        var aluno = await CriarEInserirAlunoAsync();

        var encontrado = await _repository.ObterPorEmail(aluno.Email);

        Assert.NotNull(encontrado);
        Assert.Equal(aluno.Email.Valor, encontrado.Email.Valor);
        Assert.Equal("Aline", encontrado.Nome);
    }

    [Fact]
    public async Task Deve_Verificar_Se_Cpf_Ja_Existe()
    {
        var aluno = await CriarEInserirAlunoAsync();

        var existe = await _repository.CpfJaExiste(aluno.Cpf);

        Assert.True(existe);
    }

    [Fact]
    public async Task Deve_Verificar_Se_Email_Ja_Existe()
    {
        var aluno = await CriarEInserirAlunoAsync();

        var existe = await _repository.EmailJaExiste(aluno.Email);

        Assert.True(existe);
    }

    [Fact]
    public async Task Deve_Obter_Aluno_Por_Nome()
    {
        var aluno = await CriarEInserirAlunoAsync();

        var encontrados = await _repository.ObterPorNome("Aline");

        Assert.NotNull(encontrados);
        Assert.Contains(encontrados, a => a.Id == aluno.Id);
    }

    [Fact]
    public async Task Deve_Atualizar_Aluno()
    {
        var aluno = await CriarEInserirAlunoAsync();

        var logradouro = await CriarEInserirLogradouroAsync();

        var result = Aluno.Criar(
            aluno.Id,
            "Aline",
            aluno.Cpf.Valor,
            aluno.DataNascimento,
            aluno.Telefone.Valor,
            aluno.Email.Valor,
            logradouro,
            "999",
            "Duarte Sutil",
            ObterSenhaTeste(),
            aluno.Foto
        );

        Assert.True(result.IsSuccess);

        await _repository.Atualizar(result.Value!);

        var encontrado = await _repository.ObterPorId(aluno.Id);

        Assert.NotNull(encontrado);
        Assert.Equal("Aline", encontrado.Nome);
        Assert.Equal("Duarte Sutil", encontrado.Endereco.Complemento);
        Assert.Equal("999", encontrado.Endereco.Numero);
        Assert.Equal(ObterSenhaTeste(), encontrado.Senha.Valor);
    }

    [Fact]
    public async Task Deve_Trocar_Senha_Do_Aluno()
    {
        var aluno = await CriarEInserirAlunoAsync();

        // Continua contendo a sigla do SGBD
        var senhaResult = Senha.Criar(ObterSenhaTeste());

        Assert.True(senhaResult.IsSuccess);

        var alterou = await _repository.TrocarSenha(
            aluno.Id,
            senhaResult.Value!
        );

        Assert.True(alterou);

        var encontrado = await _repository.ObterPorId(aluno.Id);

        Assert.NotNull(encontrado);
        Assert.Equal(ObterSenhaTeste(), encontrado.Senha.Valor);
    }

    [Fact]
    public async Task Deve_Remover_Aluno()
    {
        var aluno = await CriarEInserirAlunoAsync();

        var removeu = await _repository.Remover(aluno.Id);

        Assert.True(removeu);

        var encontrado = await _repository.ObterPorId(aluno.Id);

        Assert.Null(encontrado);
    }

    [Fact]
    public async Task Cpf_Inexistente_Deve_Retornar_False()
    {
        var cpfResult = Cpf.Criar(GerarCpf());

        Assert.True(cpfResult.IsSuccess);

        var existe = await _repository.CpfJaExiste(cpfResult.Value!);

        Assert.False(existe);
    }

    [Fact]
    public async Task Email_Inexistente_Deve_Retornar_False()
    {
        var emailResult = Email.Criar(GerarEmail());

        Assert.True(emailResult.IsSuccess);

        var existe = await _repository.EmailJaExiste(emailResult.Value!);

        Assert.False(existe);
    }

    [Fact]
    public async Task Obter_Id_Inexistente_Deve_Retornar_Null()
    {
        var encontrado = await _repository.ObterPorId(999999999);

        Assert.Null(encontrado);
    }

    [Fact]
    public async Task Remover_Id_Inexistente_Deve_Retornar_False()
    {
        var removeu = await _repository.Remover(999999999);

        Assert.False(removeu);
    }

    [Fact]
    public async Task Trocar_Senha_Id_Inexistente_Deve_Retornar_False()
    {
        var senhaResult = Senha.Criar(ObterSenhaTeste());

        Assert.True(senhaResult.IsSuccess);

        var alterou = await _repository.TrocarSenha(
            999999999,
            senhaResult.Value!
        );

        Assert.False(alterou);
    }
}