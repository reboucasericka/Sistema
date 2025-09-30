-- =============================================
-- Script de Migração: Criação de Índices
-- =============================================

-- Índices para tabela de Usuários
CREATE NONCLUSTERED INDEX IX_Usuarios_Email ON Usuarios(Email);
CREATE NONCLUSTERED INDEX IX_Usuarios_AspNetUserId ON Usuarios(AspNetUserId);
CREATE NONCLUSTERED INDEX IX_Usuarios_PerfilId ON Usuarios(PerfilId);
CREATE NONCLUSTERED INDEX IX_Usuarios_Ativo ON Usuarios(Ativo);

-- Índices para tabela de Clientes
CREATE NONCLUSTERED INDEX IX_Clientes_Email ON Clientes(Email);
CREATE NONCLUSTERED INDEX IX_Clientes_Telemovel ON Clientes(Telemovel);
CREATE NONCLUSTERED INDEX IX_Clientes_Ativo ON Clientes(Ativo);
CREATE NONCLUSTERED INDEX IX_Clientes_DataCadastro ON Clientes(DataCadastro);
CREATE NONCLUSTERED INDEX IX_Clientes_DataUltimaVisita ON Clientes(DataUltimaVisita);

-- Índices para tabela de Produtos
CREATE NONCLUSTERED INDEX IX_Produtos_CodigoProduto ON Produtos(CodigoProduto);
CREATE NONCLUSTERED INDEX IX_Produtos_CategoriaProdutoId ON Produtos(CategoriaProdutoId);
CREATE NONCLUSTERED INDEX IX_Produtos_FornecedorId ON Produtos(FornecedorId);
CREATE NONCLUSTERED INDEX IX_Produtos_Ativo ON Produtos(Ativo);
CREATE NONCLUSTERED INDEX IX_Produtos_Estoque ON Produtos(Estoque);

-- Índices para tabela de Serviços
CREATE NONCLUSTERED INDEX IX_Servicos_CategoriaServicoId ON Servicos(CategoriaServicoId);
CREATE NONCLUSTERED INDEX IX_Servicos_Ativo ON Servicos(Ativo);
CREATE NONCLUSTERED INDEX IX_Servicos_CodigoServico ON Servicos(CodigoServico);

-- Índices para tabela de Agendamentos
CREATE NONCLUSTERED INDEX IX_Agendamentos_Data ON Agendamentos(Data);
CREATE NONCLUSTERED INDEX IX_Agendamentos_ClienteId ON Agendamentos(ClienteId);
CREATE NONCLUSTERED INDEX IX_Agendamentos_ProfissionalId ON Agendamentos(ProfissionalId);
CREATE NONCLUSTERED INDEX IX_Agendamentos_ServicoId ON Agendamentos(ServicoId);
CREATE NONCLUSTERED INDEX IX_Agendamentos_Status ON Agendamentos(Status);
CREATE NONCLUSTERED INDEX IX_Agendamentos_Data_Status ON Agendamentos(Data, Status);
CREATE NONCLUSTERED INDEX IX_Agendamentos_ClienteId_Data ON Agendamentos(ClienteId, Data);

-- Índices para tabela de Horários
CREATE NONCLUSTERED INDEX IX_Horarios_ProfissionalId ON Horarios(ProfissionalId);
CREATE NONCLUSTERED INDEX IX_Horarios_DiaSemana ON Horarios(DiaSemana);
CREATE NONCLUSTERED INDEX IX_Horarios_DataEspecifica ON Horarios(DataEspecifica);
CREATE NONCLUSTERED INDEX IX_Horarios_Status ON Horarios(Status);

-- Índices para tabela de Estoque Movimentações
CREATE NONCLUSTERED INDEX IX_EstoqueMovimentacoes_ProdutoId ON EstoqueMovimentacoes(ProdutoId);
CREATE NONCLUSTERED INDEX IX_EstoqueMovimentacoes_DataMovimentacao ON EstoqueMovimentacoes(DataMovimentacao);
CREATE NONCLUSTERED INDEX IX_EstoqueMovimentacoes_TipoMovimentacao ON EstoqueMovimentacoes(TipoMovimentacao);
CREATE NONCLUSTERED INDEX IX_EstoqueMovimentacoes_UsuarioId ON EstoqueMovimentacoes(UsuarioId);

-- Índices para tabela de Estoque Entradas
CREATE NONCLUSTERED INDEX IX_EstoqueEntradas_ProdutoId ON EstoqueEntradas(ProdutoId);
CREATE NONCLUSTERED INDEX IX_EstoqueEntradas_Data ON EstoqueEntradas(Data);
CREATE NONCLUSTERED INDEX IX_EstoqueEntradas_FornecedorId ON EstoqueEntradas(FornecedorId);

-- Índices para tabela de Estoque Saídas
CREATE NONCLUSTERED INDEX IX_EstoqueSaidas_ProdutoId ON EstoqueSaidas(ProdutoId);
CREATE NONCLUSTERED INDEX IX_EstoqueSaidas_Data ON EstoqueSaidas(Data);
CREATE NONCLUSTERED INDEX IX_EstoqueSaidas_AgendamentoId ON EstoqueSaidas(AgendamentoId);

-- Índices para tabela de Faturação
CREATE NONCLUSTERED INDEX IX_Faturacao_Data ON Faturacao(Data);
CREATE NONCLUSTERED INDEX IX_Faturacao_ClienteId ON Faturacao(ClienteId);
CREATE NONCLUSTERED INDEX IX_Faturacao_Status ON Faturacao(Status);
CREATE NONCLUSTERED INDEX IX_Faturacao_NumeroFatura ON Faturacao(NumeroFatura);

-- Índices para tabela de Faturação Detalhes
CREATE NONCLUSTERED INDEX IX_FaturacaoDetalhes_FaturacaoId ON FaturacaoDetalhes(FaturacaoId);
CREATE NONCLUSTERED INDEX IX_FaturacaoDetalhes_TipoItem ON FaturacaoDetalhes(TipoItem);
CREATE NONCLUSTERED INDEX IX_FaturacaoDetalhes_ProdutoId ON FaturacaoDetalhes(ProdutoId);
CREATE NONCLUSTERED INDEX IX_FaturacaoDetalhes_ServicoId ON FaturacaoDetalhes(ServicoId);

-- Índices para tabela de Pagar
CREATE NONCLUSTERED INDEX IX_Pagar_DataVenc ON Pagar(DataVenc);
CREATE NONCLUSTERED INDEX IX_Pagar_Pago ON Pagar(Pago);
CREATE NONCLUSTERED INDEX IX_Pagar_UsuarioLanc ON Pagar(UsuarioLanc);
CREATE NONCLUSTERED INDEX IX_Pagar_DataLanc ON Pagar(DataLanc);

-- Índices para tabela de Receber
CREATE NONCLUSTERED INDEX IX_Receber_DataVenc ON Receber(DataVenc);
CREATE NONCLUSTERED INDEX IX_Receber_Pago ON Receber(Pago);
CREATE NONCLUSTERED INDEX IX_Receber_UsuarioLanc ON Receber(UsuarioLanc);
CREATE NONCLUSTERED INDEX IX_Receber_DataLanc ON Receber(DataLanc);

-- Índices para tabela de Caixa
CREATE NONCLUSTERED INDEX IX_Caixa_Data ON Caixa(Data);
CREATE NONCLUSTERED INDEX IX_Caixa_Status ON Caixa(Status);
CREATE NONCLUSTERED INDEX IX_Caixa_UsuarioAbertura ON Caixa(UsuarioAbertura);

-- Índices para tabela de Lembretes
CREATE NONCLUSTERED INDEX IX_Lembretes_AgendamentoId ON Lembretes(AgendamentoId);
CREATE NONCLUSTERED INDEX IX_Lembretes_DataEnvio ON Lembretes(DataEnvio);
CREATE NONCLUSTERED INDEX IX_Lembretes_Status ON Lembretes(Status);

-- Índices para tabela de Avaliações
CREATE NONCLUSTERED INDEX IX_AvaliacoesAtendimento_ClienteId ON AvaliacoesAtendimento(ClienteId);
CREATE NONCLUSTERED INDEX IX_AvaliacoesAtendimento_ServicoId ON AvaliacoesAtendimento(ServicoId);
CREATE NONCLUSTERED INDEX IX_AvaliacoesAtendimento_DataAvaliacao ON AvaliacoesAtendimento(DataAvaliacao);

-- Índices para tabela de Histórico de Procedimentos
CREATE NONCLUSTERED INDEX IX_HistoricoProcedimentos_ClienteId ON HistoricoProcedimentos(ClienteId);
CREATE NONCLUSTERED INDEX IX_HistoricoProcedimentos_ProfissionalId ON HistoricoProcedimentos(ProfissionalId);
CREATE NONCLUSTERED INDEX IX_HistoricoProcedimentos_DataProcedimento ON HistoricoProcedimentos(DataProcedimento);

-- Índices para tabela de Profissionais
CREATE NONCLUSTERED INDEX IX_Profissionais_UsuarioId ON Profissionais(UsuarioId);
CREATE NONCLUSTERED INDEX IX_Profissionais_Ativo ON Profissionais(Ativo);

-- Índices para tabela de Profissional Serviços
CREATE NONCLUSTERED INDEX IX_ProfissionalServicos_ProfissionalId ON ProfissionalServicos(ProfissionalId);
CREATE NONCLUSTERED INDEX IX_ProfissionalServicos_ServicoId ON ProfissionalServicos(ServicoId);

-- Índices para tabela de Planos
CREATE NONCLUSTERED INDEX IX_Planos_ClienteId ON Planos(ClienteId);
CREATE NONCLUSTERED INDEX IX_Planos_Ativo ON Planos(Ativo);

-- Índices para tabela de Plano Agendamentos
CREATE NONCLUSTERED INDEX IX_PlanoAgendamentos_PlanoId ON PlanoAgendamentos(PlanoId);
CREATE NONCLUSTERED INDEX IX_PlanoAgendamentos_AgendamentoId ON PlanoAgendamentos(AgendamentoId);

-- Índices para tabela de Log de Ações
CREATE NONCLUSTERED INDEX IX_LogAcoes_UsuarioId ON LogAcoes(UsuarioId);
CREATE NONCLUSTERED INDEX IX_LogAcoes_TabelaAfetada ON LogAcoes(TabelaAfetada);
CREATE NONCLUSTERED INDEX IX_LogAcoes_DataHora ON LogAcoes(DataHora);

-- Índices para tabela de Backup
CREATE NONCLUSTERED INDEX IX_BackupDados_Tabela ON BackupDados(Tabela);
CREATE NONCLUSTERED INDEX IX_BackupDados_DataBackup ON BackupDados(DataBackup);

PRINT 'Índices criados com sucesso!';

