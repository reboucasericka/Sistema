-- =============================================
-- Script de Migração: Nova Estrutura do Banco
-- =============================================

-- 1. Criar tabela de Perfis
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Perfis' AND xtype='U')
BEGIN
    CREATE TABLE Perfis (
        PerfilId INT IDENTITY(1,1) PRIMARY KEY,
        Nome NVARCHAR(50) NOT NULL UNIQUE,
        AspNetRoleId NVARCHAR(450) NULL,
        DataCriacao DATETIME2 DEFAULT GETDATE(),
        Ativo BIT DEFAULT 1
    );
    
    -- Inserir perfis padrão
    INSERT INTO Perfis (Nome) VALUES 
    ('Administrador'),
    ('Recepcionista'),
    ('Profissional'),
    ('Gerente');
END

-- 2. Criar tabela de Usuários melhorada
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Usuarios' AND xtype='U')
BEGIN
    CREATE TABLE Usuarios (
        UsuarioId INT IDENTITY(1,1) PRIMARY KEY,
        AspNetUserId NVARCHAR(450) NOT NULL UNIQUE,
        Nome NVARCHAR(100) NOT NULL,
        Email NVARCHAR(100) NOT NULL UNIQUE,
        Nif NVARCHAR(20) NULL,
        PerfilId INT NOT NULL FOREIGN KEY REFERENCES Perfis(PerfilId),
        DataCadastro DATETIME2 NOT NULL DEFAULT GETDATE(),
        Ativo BIT NOT NULL DEFAULT 1,
        Telemovel NVARCHAR(20) NULL,
        Endereco NVARCHAR(200) NULL,
        Foto NVARCHAR(200) NULL,
        DataUltimoAcesso DATETIME2 NULL,
        TentativasLogin INT DEFAULT 0,
        Bloqueado BIT DEFAULT 0
    );
END

-- 3. Criar tabela de Clientes melhorada
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Clientes' AND xtype='U')
BEGIN
    CREATE TABLE Clientes (
        ClienteId INT IDENTITY(1,1) PRIMARY KEY,
        Nome NVARCHAR(100) NOT NULL,
        Email NVARCHAR(100) NULL,
        Telemovel NVARCHAR(20) NULL,
        Endereco NVARCHAR(200) NULL,
        DataNascimento DATE NULL,
        DataCadastro DATETIME2 NOT NULL DEFAULT GETDATE(),
        Ativo BIT NOT NULL DEFAULT 1,
        Observacoes NVARCHAR(MAX) NULL,
        HistoricoAlergias NVARCHAR(255) NULL,
        Genero CHAR(1) NULL CHECK (Genero IN ('M','F','O')),
        Profissao NVARCHAR(100) NULL,
        ComoConheceu NVARCHAR(100) NULL,
        IndicacaoClienteId INT NULL,
        DataUltimaVisita DATETIME2 NULL,
        TotalVisitas INT DEFAULT 0,
        ValorTotalGasto DECIMAL(10,2) DEFAULT 0,
        
        CONSTRAINT FK_Clientes_Indicacao FOREIGN KEY (IndicacaoClienteId) REFERENCES Clientes(ClienteId)
    );
END

-- 4. Criar tabela de Categorias de Serviços
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CategoriasServicos' AND xtype='U')
BEGIN
    CREATE TABLE CategoriasServicos (
        CategoriaServicoId INT IDENTITY(1,1) PRIMARY KEY,
        Nome NVARCHAR(100) NOT NULL UNIQUE,
        Descricao NVARCHAR(255) NULL,
        Ativo BIT DEFAULT 1,
        DataCriacao DATETIME2 DEFAULT GETDATE()
    );
    
    -- Inserir categorias padrão
    INSERT INTO CategoriasServicos (Nome, Descricao) VALUES
    ('Extensão de Cílios Fio a Fio', 'Extensão de cílios técnica fio a fio'),
    ('Extensão de Cílios Volume Russo', 'Extensão de cílios técnica volume russo'),
    ('Extensão de Cílios Híbrido', 'Extensão de cílios técnica híbrida'),
    ('Manutenção de Extensão de Cílios', 'Manutenção e retoque de extensões'),
    ('Remoção de Extensão de Cílios', 'Remoção segura de extensões'),
    ('Lash Lifting (Permanente de Cílios)', 'Permanente de cílios naturais'),
    ('Coloração de Cílios', 'Coloração de cílios naturais'),
    ('Design de Sobrancelhas', 'Design e modelagem de sobrancelhas'),
    ('Depilação Facial', 'Depilação de pelos faciais');
END

-- 5. Criar tabela de Serviços melhorada
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Servicos' AND xtype='U')
BEGIN
    CREATE TABLE Servicos (
        ServicoId INT IDENTITY(1,1) PRIMARY KEY,
        Nome NVARCHAR(150) NOT NULL,
        CategoriaServicoId INT NOT NULL FOREIGN KEY REFERENCES CategoriasServicos(CategoriaServicoId),
        Valor DECIMAL(10,2) NOT NULL,
        Foto NVARCHAR(200) NULL,
        DiasRetorno INT NULL,
        Ativo BIT NOT NULL DEFAULT 1,
        Comissao DECIMAL(10,2) NULL,
        DuracaoMinutos INT NOT NULL DEFAULT 60,
        Descricao NVARCHAR(MAX) NULL,
        MaterialNecessario NVARCHAR(MAX) NULL,
        InstrucoesPosTratamento NVARCHAR(MAX) NULL,
        ValorCusto DECIMAL(10,2) NULL,
        MargemLucro DECIMAL(10,2) NULL,
        CodigoServico NVARCHAR(20) NULL UNIQUE,
        DataCriacao DATETIME2 DEFAULT GETDATE()
    );
END

-- 6. Criar tabela de Categorias de Produtos
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CategoriasProdutos' AND xtype='U')
BEGIN
    CREATE TABLE CategoriasProdutos (
        CategoriaProdutoId INT IDENTITY(1,1) PRIMARY KEY,
        Nome NVARCHAR(100) NOT NULL UNIQUE,
        Descricao NVARCHAR(255) NULL,
        Ativo BIT DEFAULT 1,
        DataCriacao DATETIME2 DEFAULT GETDATE()
    );
    
    -- Inserir categorias padrão
    INSERT INTO CategoriasProdutos (Nome, Descricao) VALUES
    ('Fios para Extensão de Cílios', 'Fios de diferentes tamanhos e curvaturas'),
    ('Cola (Adesivo) para Cílios', 'Adesivos e colas para extensões'),
    ('Removedor de Cola para Cílios', 'Produtos para remoção segura'),
    ('Pinças Profissionais', 'Pinças para aplicação de extensões'),
    ('Microbrush e Cotonetes', 'Acessórios para aplicação'),
    ('Pads para Olhos (Protetores de Pálpebras)', 'Protetores para pálpebras'),
    ('Sérum Fortalecedor de Cílios', 'Sérum para fortalecimento'),
    ('Máscara de Cílios Especializada', 'Máscaras específicas'),
    ('Shampoo de Limpeza para Cílios', 'Shampoos para limpeza'),
    ('Primer para Cílios', 'Primers para preparação');
END

-- 7. Criar tabela de Fornecedores
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Fornecedores' AND xtype='U')
BEGIN
    CREATE TABLE Fornecedores (
        FornecedorId INT IDENTITY(1,1) PRIMARY KEY,
        Nome NVARCHAR(100) NOT NULL,
        Telemovel NVARCHAR(20) NULL,
        Email NVARCHAR(100) NULL,
        Nif NVARCHAR(20) NULL,
        Endereco NVARCHAR(200) NULL,
        PrazoEntrega INT NULL,
        Observacoes NVARCHAR(MAX) NULL,
        DataCadastro DATETIME2 NOT NULL DEFAULT GETDATE(),
        Ativo BIT DEFAULT 1,
        ContatoResponsavel NVARCHAR(100) NULL,
        TelefoneFixo NVARCHAR(20) NULL,
        Website NVARCHAR(200) NULL
    );
END

-- 8. Criar tabela de Produtos melhorada
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Produtos' AND xtype='U')
BEGIN
    CREATE TABLE Produtos (
        ProdutoId INT IDENTITY(1,1) PRIMARY KEY,
        Nome NVARCHAR(150) NOT NULL,
        Descricao NVARCHAR(255) NULL,
        CategoriaProdutoId INT NOT NULL FOREIGN KEY REFERENCES CategoriasProdutos(CategoriaProdutoId),
        ValorCompra DECIMAL(10,2) NOT NULL,
        ValorVenda DECIMAL(10,2) NOT NULL,
        Estoque INT NOT NULL DEFAULT 0,
        Foto NVARCHAR(200) NULL,
        NivelEstoqueMinimo INT NOT NULL DEFAULT 0,
        FornecedorId INT NULL FOREIGN KEY REFERENCES Fornecedores(FornecedorId),
        CodigoProduto NVARCHAR(50) NULL UNIQUE,
        UnidadeMedida NVARCHAR(20) DEFAULT 'UN',
        Peso DECIMAL(10,3) NULL,
        Dimensoes NVARCHAR(50) NULL,
        DataValidade DATE NULL,
        Lote NVARCHAR(50) NULL,
        LocalizacaoEstoque NVARCHAR(100) NULL,
        MargemLucro DECIMAL(10,2) NULL,
        Ativo BIT DEFAULT 1,
        DataCriacao DATETIME2 DEFAULT GETDATE()
    );
END

-- 9. Criar tabela de Profissionais
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Profissionais' AND xtype='U')
BEGIN
    CREATE TABLE Profissionais (
        ProfissionalId INT IDENTITY(1,1) PRIMARY KEY,
        UsuarioId INT NOT NULL FOREIGN KEY REFERENCES Usuarios(UsuarioId),
        Especialidade NVARCHAR(100) NOT NULL,
        ComissaoPadrao DECIMAL(10,2) NOT NULL DEFAULT 0,
        Ativo BIT NOT NULL DEFAULT 1,
        DataContratacao DATE NULL,
        SalarioBase DECIMAL(10,2) NULL,
        Observacoes NVARCHAR(MAX) NULL
    );
END

-- 10. Criar tabela de ProfissionalServicos
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ProfissionalServicos' AND xtype='U')
BEGIN
    CREATE TABLE ProfissionalServicos (
        ProfissionalServicoId INT IDENTITY(1,1) PRIMARY KEY,
        ProfissionalId INT NOT NULL FOREIGN KEY REFERENCES Profissionais(ProfissionalId),
        ServicoId INT NOT NULL FOREIGN KEY REFERENCES Servicos(ServicoId),
        Comissao DECIMAL(10,2) NULL,
        Ativo BIT DEFAULT 1,
        DataCriacao DATETIME2 DEFAULT GETDATE(),
        
        CONSTRAINT UK_ProfissionalServico UNIQUE (ProfissionalId, ServicoId)
    );
END

-- 11. Criar tabela de Horários
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Horarios' AND xtype='U')
BEGIN
    CREATE TABLE Horarios (
        HorarioId INT IDENTITY(1,1) PRIMARY KEY,
        ProfissionalId INT NOT NULL FOREIGN KEY REFERENCES Profissionais(ProfissionalId),
        Horario TIME NOT NULL,
        DiaSemana INT NULL CHECK (DiaSemana BETWEEN 1 AND 7),
        DataEspecifica DATE NULL,
        Status NVARCHAR(20) DEFAULT 'disponível' CHECK (Status IN ('disponível', 'ocupado', 'bloqueado')),
        ExportadoExcel BIT DEFAULT 0,
        ExportadoPdf BIT DEFAULT 0,
        Observacoes NVARCHAR(255) NULL
    );
END

-- 12. Criar tabela de Agendamentos melhorada
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Agendamentos' AND xtype='U')
BEGIN
    CREATE TABLE Agendamentos (
        AgendamentoId INT IDENTITY(1,1) PRIMARY KEY,
        ClienteId INT NOT NULL FOREIGN KEY REFERENCES Clientes(ClienteId),
        ServicoId INT NOT NULL FOREIGN KEY REFERENCES Servicos(ServicoId),
        ProfissionalId INT NOT NULL FOREIGN KEY REFERENCES Profissionais(ProfissionalId),
        Data DATE NOT NULL,
        Horario TIME NOT NULL,
        Status NVARCHAR(20) DEFAULT 'agendado' CHECK (Status IN ('agendado', 'confirmado', 'cancelado', 'concluído', 'ausente')),
        Observacoes NVARCHAR(MAX) NULL,
        LembreteEnviado BIT DEFAULT 0,
        ExportadoExcel BIT DEFAULT 0,
        ExportadoPdf BIT DEFAULT 0,
        DataHoraInicio DATETIME2 NULL,
        DataHoraFim DATETIME2 NULL,
        DuracaoReal INT NULL,
        ValorServico DECIMAL(10,2) NULL,
        Desconto DECIMAL(10,2) DEFAULT 0,
        ValorTotal DECIMAL(10,2) NULL,
        FormaPagamentoId INT NULL,
        Pago BIT DEFAULT 0,
        DataPagamento DATETIME2 NULL,
        ObservacoesTecnicas NVARCHAR(MAX) NULL,
        MaterialUsado NVARCHAR(MAX) NULL,
        SatisfacaoCliente INT NULL CHECK (SatisfacaoCliente BETWEEN 1 AND 5),
        ComentariosCliente NVARCHAR(MAX) NULL,
        Reagendado BIT DEFAULT 0,
        AgendamentoOriginalId INT NULL,
        DataCriacao DATETIME2 DEFAULT GETDATE(),
        UsuarioCriacao INT NOT NULL FOREIGN KEY REFERENCES Usuarios(UsuarioId),
        
        CONSTRAINT FK_Agendamentos_Original FOREIGN KEY (AgendamentoOriginalId) REFERENCES Agendamentos(AgendamentoId)
    );
END

-- 13. Criar tabela de Lembretes
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Lembretes' AND xtype='U')
BEGIN
    CREATE TABLE Lembretes (
        LembreteId INT IDENTITY(1,1) PRIMARY KEY,
        AgendamentoId INT NOT NULL FOREIGN KEY REFERENCES Agendamentos(AgendamentoId),
        DataEnvio DATETIME NOT NULL,
        MeioEnvio NVARCHAR(20) NOT NULL CHECK (MeioEnvio IN ('email', 'sms', 'whatsapp')),
        Status NVARCHAR(20) DEFAULT 'pendente' CHECK (Status IN ('pendente', 'enviado', 'falhou')),
        ExportadoExcel BIT DEFAULT 0,
        ExportadoPdf BIT DEFAULT 0,
        Mensagem NVARCHAR(MAX) NULL,
        DataEnvioReal DATETIME2 NULL
    );
END

-- 14. Criar tabela de Configurações
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Configuracoes' AND xtype='U')
BEGIN
    CREATE TABLE Configuracoes (
        ConfiguracaoId INT IDENTITY(1,1) PRIMARY KEY,
        NomeClinica NVARCHAR(100) NOT NULL,
        Email NVARCHAR(100) NOT NULL,
        TelefoneFixo NVARCHAR(20) NULL,
        TelemovelWhatsApp NVARCHAR(20) NULL,
        Endereco NVARCHAR(200) NULL,
        Logo NVARCHAR(200) NULL,
        Icone NVARCHAR(200) NULL,
        LogoRelatorio NVARCHAR(200) NULL,
        TipoRelatorio NVARCHAR(20) NULL,
        Instagram NVARCHAR(200) NULL,
        TipoComissao NVARCHAR(25) NOT NULL DEFAULT '%',
        ComissaoPadraoExtensao DECIMAL(10,2) DEFAULT 20.00,
        ComissaoPadraoDesign DECIMAL(10,2) DEFAULT 15.00,
        PastaImagens NVARCHAR(200) DEFAULT 'uploads/',
        HorarioFuncionamento NVARCHAR(50) DEFAULT '09:00-18:00',
        DuracaoPadraoServico INT DEFAULT 60,
        CNPJ NVARCHAR(20) NULL,
        LicencaSanitaria NVARCHAR(50) NULL,
        DataCriacao DATETIME2 DEFAULT GETDATE()
    );
END

-- 15. Criar tabela de Estoque Movimentações
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='EstoqueMovimentacoes' AND xtype='U')
BEGIN
    CREATE TABLE EstoqueMovimentacoes (
        MovimentacaoId INT IDENTITY(1,1) PRIMARY KEY,
        ProdutoId INT NOT NULL FOREIGN KEY REFERENCES Produtos(ProdutoId),
        Quantidade INT NOT NULL,
        TipoMovimentacao NVARCHAR(10) NOT NULL CHECK (TipoMovimentacao IN ('entrada','saida','ajuste','perda')),
        Motivo NVARCHAR(100) NOT NULL,
        UsuarioId INT NOT NULL FOREIGN KEY REFERENCES Usuarios(UsuarioId),
        FornecedorId INT NULL FOREIGN KEY REFERENCES Fornecedores(FornecedorId),
        DataMovimentacao DATETIME2 NOT NULL DEFAULT GETDATE(),
        DataValidade DATE NULL,
        Lote NVARCHAR(50) NULL,
        ValorUnitario DECIMAL(10,2) NULL,
        ValorTotal DECIMAL(10,2) NULL,
        Observacoes NVARCHAR(MAX) NULL
    );
END

-- 16. Criar tabela de Formas de Pagamento
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='FormasPagamento' AND xtype='U')
BEGIN
    CREATE TABLE FormasPagamento (
        FormaPagamentoId INT IDENTITY(1,1) PRIMARY KEY,
        Descricao NVARCHAR(50) NOT NULL UNIQUE,
        Ativo BIT DEFAULT 1,
        DataCriacao DATETIME2 DEFAULT GETDATE()
    );
    
    -- Inserir formas de pagamento padrão
    INSERT INTO FormasPagamento (Descricao) VALUES
    ('Dinheiro'),
    ('Cartão de Débito'),
    ('Cartão de Crédito'),
    ('PIX'),
    ('Transferência Bancária'),
    ('Cheque');
END

-- 17. Criar tabela de Caixa
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Caixa' AND xtype='U')
BEGIN
    CREATE TABLE Caixa (
        CaixaId INT IDENTITY(1,1) PRIMARY KEY,
        Data DATE NOT NULL,
        ValorInicial DECIMAL(10,2) NOT NULL,
        ValorFinal DECIMAL(10,2) NOT NULL,
        UsuarioAbertura INT NOT NULL FOREIGN KEY REFERENCES Usuarios(UsuarioId),
        UsuarioFechamento INT NULL FOREIGN KEY REFERENCES Usuarios(UsuarioId),
        Status NVARCHAR(20) DEFAULT 'aberto' CHECK (Status IN ('aberto', 'fechado')),
        Observacoes NVARCHAR(MAX) NULL,
        ExportadoExcel BIT DEFAULT 0,
        ExportadoPdf BIT DEFAULT 0,
        DataAbertura DATETIME2 DEFAULT GETDATE(),
        DataFechamento DATETIME2 NULL
    );
END

-- 18. Criar tabela de Faturação
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Faturacao' AND xtype='U')
BEGIN
    CREATE TABLE Faturacao (
        FaturacaoId INT IDENTITY(1,1) PRIMARY KEY,
        NumeroFatura NVARCHAR(20) NOT NULL UNIQUE,
        Data DATE NOT NULL,
        ValorTotal DECIMAL(10,2) NOT NULL,
        Desconto DECIMAL(10,2) DEFAULT 0,
        ValorFinal DECIMAL(10,2) NOT NULL,
        QuantidadeServicos INT DEFAULT 0,
        QuantidadeProdutos INT DEFAULT 0,
        UsuarioId INT NOT NULL FOREIGN KEY REFERENCES Usuarios(UsuarioId),
        ClienteId INT NULL FOREIGN KEY REFERENCES Clientes(ClienteId),
        Status NVARCHAR(20) DEFAULT 'pendente' CHECK (Status IN ('pendente', 'pago', 'cancelado')),
        FormaPagamentoId INT NULL FOREIGN KEY REFERENCES FormasPagamento(FormaPagamentoId),
        DataPagamento DATETIME2 NULL,
        Observacoes NVARCHAR(MAX) NULL,
        ExportadoExcel BIT DEFAULT 0,
        ExportadoPdf BIT DEFAULT 0,
        DataCriacao DATETIME2 DEFAULT GETDATE()
    );
END

-- 19. Criar tabela de Faturação Detalhes
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='FaturacaoDetalhes' AND xtype='U')
BEGIN
    CREATE TABLE FaturacaoDetalhes (
        FaturacaoDetalheId INT IDENTITY(1,1) PRIMARY KEY,
        FaturacaoId INT NOT NULL FOREIGN KEY REFERENCES Faturacao(FaturacaoId),
        TipoItem VARCHAR(20) NOT NULL CHECK (TipoItem IN ('Produto','Servico')),
        ProdutoId INT NULL FOREIGN KEY REFERENCES Produtos(ProdutoId),
        ServicoId INT NULL FOREIGN KEY REFERENCES Servicos(ServicoId),
        Quantidade INT NOT NULL,
        ValorUnitario DECIMAL(10,2) NOT NULL,
        ValorTotal DECIMAL(10,2) NOT NULL,
        Desconto DECIMAL(10,2) DEFAULT 0,
        Observacoes NVARCHAR(255) NULL
    );
END

-- 20. Criar tabela de Planos
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Planos' AND xtype='U')
BEGIN
    CREATE TABLE Planos (
        PlanoId INT IDENTITY(1,1) PRIMARY KEY,
        Nome NVARCHAR(100) NOT NULL,
        Descricao NVARCHAR(MAX) NULL,
        Valor DECIMAL(10,2) NOT NULL,
        QuantidadeSessoes INT NOT NULL,
        ValidadeDias INT NOT NULL,
        Ativo BIT DEFAULT 1,
        ClienteId INT NULL FOREIGN KEY REFERENCES Clientes(ClienteId),
        ExportadoExcel BIT DEFAULT 0,
        ExportadoPdf BIT DEFAULT 0,
        DataCriacao DATETIME2 DEFAULT GETDATE()
    );
END

-- 21. Criar tabela de Plano Agendamentos
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PlanoAgendamentos' AND xtype='U')
BEGIN
    CREATE TABLE PlanoAgendamentos (
        PlanoAgendamentoId INT IDENTITY(1,1) PRIMARY KEY,
        PlanoId INT NOT NULL FOREIGN KEY REFERENCES Planos(PlanoId),
        AgendamentoId INT NOT NULL FOREIGN KEY REFERENCES Agendamentos(AgendamentoId),
        SessaoUsada BIT DEFAULT 0,
        DataUso DATETIME2 NULL,
        Observacoes NVARCHAR(255) NULL
    );
END

-- 22. Criar tabela de Estéticas (Multiunidade)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Esteticas' AND xtype='U')
BEGIN
    CREATE TABLE Esteticas (
        EsteticaId INT IDENTITY(1,1) PRIMARY KEY,
        Nome NVARCHAR(100) NOT NULL,
        Endereco NVARCHAR(200) NOT NULL,
        Telemovel NVARCHAR(20) NULL,
        Email NVARCHAR(100) NULL,
        Ativo BIT DEFAULT 1,
        CNPJ NVARCHAR(20) NULL,
        ResponsavelTecnico NVARCHAR(100) NULL,
        LicencaSanitaria NVARCHAR(50) NULL,
        DataAbertura DATE NULL,
        HorarioFuncionamento NVARCHAR(100) NULL,
        CapacidadeMaxima INT NULL,
        ExportadoExcel BIT DEFAULT 0,
        ExportadoPdf BIT DEFAULT 0,
        DataCriacao DATETIME2 DEFAULT GETDATE()
    );
END

-- 23. Criar tabela de Avaliações de Atendimento
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AvaliacoesAtendimento' AND xtype='U')
BEGIN
    CREATE TABLE AvaliacoesAtendimento (
        AvaliacaoId INT IDENTITY(1,1) PRIMARY KEY,
        ClienteId INT NOT NULL FOREIGN KEY REFERENCES Clientes(ClienteId),
        ServicoId INT NOT NULL FOREIGN KEY REFERENCES Servicos(ServicoId),
        AgendamentoId INT NULL FOREIGN KEY REFERENCES Agendamentos(AgendamentoId),
        DataAvaliacao DATETIME2 NOT NULL DEFAULT GETDATE(),
        Satisfacao INT NOT NULL CHECK (Satisfacao BETWEEN 1 AND 5),
        Comentarios NVARCHAR(MAX) NULL,
        Recomendaria BIT NULL,
        ProfissionalId INT NULL FOREIGN KEY REFERENCES Profissionais(ProfissionalId)
    );
END

-- 24. Criar tabela de Histórico de Procedimentos
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='HistoricoProcedimentos' AND xtype='U')
BEGIN
    CREATE TABLE HistoricoProcedimentos (
        ProcedimentoId INT IDENTITY(1,1) PRIMARY KEY,
        ClienteId INT NOT NULL FOREIGN KEY REFERENCES Clientes(ClienteId),
        ServicoId INT NOT NULL FOREIGN KEY REFERENCES Servicos(ServicoId),
        ProfissionalId INT NOT NULL FOREIGN KEY REFERENCES Profissionais(ProfissionalId),
        AgendamentoId INT NULL FOREIGN KEY REFERENCES Agendamentos(AgendamentoId),
        DataProcedimento DATETIME2 NOT NULL DEFAULT GETDATE(),
        MaterialUsado NVARCHAR(255) NULL,
        ObservacoesTecnicas NVARCHAR(MAX) NULL,
        Preco DECIMAL(10,2) NOT NULL,
        DuracaoMinutos INT NULL,
        SatisfacaoCliente INT NULL CHECK (SatisfacaoCliente BETWEEN 1 AND 5),
        ObservacoesCliente NVARCHAR(MAX) NULL
    );
END

-- 25. Criar tabela de Pagar
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Pagar' AND xtype='U')
BEGIN
    CREATE TABLE Pagar (
        PagarId INT IDENTITY(1,1) PRIMARY KEY,
        Descricao NVARCHAR(100) NULL,
        Tipo NVARCHAR(50) NULL,
        Valor DECIMAL(10,2) NOT NULL,
        DataLanc DATE NOT NULL,
        DataVenc DATE NOT NULL,
        DataPgto DATE NULL,
        UsuarioLanc INT NOT NULL FOREIGN KEY REFERENCES Usuarios(UsuarioId),
        UsuarioBaixa INT NULL FOREIGN KEY REFERENCES Usuarios(UsuarioId),
        Foto NVARCHAR(200) NULL,
        PessoaId INT NULL,
        Pago BIT NOT NULL DEFAULT 0,
        ProdutoId INT NULL FOREIGN KEY REFERENCES Produtos(ProdutoId),
        Quantidade INT NULL,
        FormaPagamentoId INT NULL FOREIGN KEY REFERENCES FormasPagamento(FormaPagamentoId),
        Parcela INT DEFAULT 1,
        TotalParcelas INT DEFAULT 1,
        ExportadoExcel BIT DEFAULT 0,
        ExportadoPdf BIT DEFAULT 0,
        Observacoes NVARCHAR(MAX) NULL
    );
END

-- 26. Criar tabela de Receber
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Receber' AND xtype='U')
BEGIN
    CREATE TABLE Receber (
        ReceberId INT IDENTITY(1,1) PRIMARY KEY,
        Descricao NVARCHAR(100) NULL,
        Tipo NVARCHAR(50) NULL,
        Valor DECIMAL(10,2) NOT NULL,
        DataLanc DATE NOT NULL,
        DataVenc DATE NOT NULL,
        DataPgto DATE NULL,
        UsuarioLanc INT NOT NULL FOREIGN KEY REFERENCES Usuarios(UsuarioId),
        UsuarioBaixa INT NULL FOREIGN KEY REFERENCES Usuarios(UsuarioId),
        Foto NVARCHAR(200) NULL,
        PessoaId INT NULL,
        Pago BIT NOT NULL DEFAULT 0,
        ProdutoId INT NULL FOREIGN KEY REFERENCES Produtos(ProdutoId),
        Quantidade INT NULL,
        FormaPagamentoId INT NULL FOREIGN KEY REFERENCES FormasPagamento(FormaPagamentoId),
        Parcela INT DEFAULT 1,
        TotalParcelas INT DEFAULT 1,
        ExportadoExcel BIT DEFAULT 0,
        ExportadoPdf BIT DEFAULT 0,
        Observacoes NVARCHAR(MAX) NULL
    );
END

-- 27. Criar tabela de Estoque Entradas
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='EstoqueEntradas' AND xtype='U')
BEGIN
    CREATE TABLE EstoqueEntradas (
        EntradaId INT IDENTITY(1,1) PRIMARY KEY,
        ProdutoId INT NOT NULL FOREIGN KEY REFERENCES Produtos(ProdutoId),
        Quantidade INT NOT NULL,
        Motivo NVARCHAR(100) NOT NULL,
        UsuarioId INT NOT NULL FOREIGN KEY REFERENCES Usuarios(UsuarioId),
        FornecedorId INT NULL FOREIGN KEY REFERENCES Fornecedores(FornecedorId),
        Data DATE NOT NULL,
        TipoMovimentacao NVARCHAR(20) DEFAULT 'entrada',
        Lote NVARCHAR(50) NULL,
        DataValidade DATE NULL,
        ValorUnitario DECIMAL(10,2) NULL,
        ExportadoExcel BIT DEFAULT 0,
        ExportadoPdf BIT DEFAULT 0,
        Observacoes NVARCHAR(MAX) NULL
    );
END

-- 28. Criar tabela de Estoque Saídas
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='EstoqueSaidas' AND xtype='U')
BEGIN
    CREATE TABLE EstoqueSaidas (
        SaidaId INT IDENTITY(1,1) PRIMARY KEY,
        ProdutoId INT NOT NULL FOREIGN KEY REFERENCES Produtos(ProdutoId),
        Quantidade INT NOT NULL,
        Motivo NVARCHAR(100) NOT NULL,
        UsuarioId INT NOT NULL FOREIGN KEY REFERENCES Usuarios(UsuarioId),
        AgendamentoId INT NULL FOREIGN KEY REFERENCES Agendamentos(AgendamentoId),
        Data DATE NOT NULL,
        TipoMovimentacao NVARCHAR(20) DEFAULT 'saida',
        ValorUnitario DECIMAL(10,2) NULL,
        ExportadoExcel BIT DEFAULT 0,
        ExportadoPdf BIT DEFAULT 0,
        Observacoes NVARCHAR(MAX) NULL
    );
END

-- 29. Criar tabela de Log de Ações (Auditoria)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='LogAcoes' AND xtype='U')
BEGIN
    CREATE TABLE LogAcoes (
        LogId INT IDENTITY(1,1) PRIMARY KEY,
        UsuarioId INT NOT NULL FOREIGN KEY REFERENCES Usuarios(UsuarioId),
        TabelaAfetada NVARCHAR(50) NOT NULL,
        Acao NVARCHAR(20) NOT NULL CHECK (Acao IN ('INSERT', 'UPDATE', 'DELETE')),
        RegistroId INT NOT NULL,
        DadosAntigos NVARCHAR(MAX) NULL,
        DadosNovos NVARCHAR(MAX) NULL,
        DataHora DATETIME2 DEFAULT GETDATE(),
        IpAddress NVARCHAR(50) NULL,
        UserAgent NVARCHAR(500) NULL
    );
END

-- 30. Criar tabela de Backup de Dados
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='BackupDados' AND xtype='U')
BEGIN
    CREATE TABLE BackupDados (
        BackupId INT IDENTITY(1,1) PRIMARY KEY,
        Tabela NVARCHAR(50) NOT NULL,
        Dados NVARCHAR(MAX) NOT NULL,
        DataBackup DATETIME2 DEFAULT GETDATE(),
        UsuarioId INT NOT NULL FOREIGN KEY REFERENCES Usuarios(UsuarioId),
        TipoBackup NVARCHAR(20) DEFAULT 'manual' CHECK (TipoBackup IN ('manual', 'automatico', 'sistema'))
    );
END

PRINT 'Estrutura do banco de dados criada com sucesso!';

