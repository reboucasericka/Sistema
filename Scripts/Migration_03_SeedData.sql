-- =============================================
-- Script de Migração: Dados Iniciais (Seed)
-- =============================================

-- 1. Inserir dados iniciais na tabela de Configurações
IF NOT EXISTS (SELECT 1 FROM Configuracoes)
BEGIN
    INSERT INTO Configuracoes 
    (NomeClinica, Email, TelefoneFixo, TelemovelWhatsApp, Endereco, Logo, Icone, LogoRelatorio, TipoRelatorio, Instagram, CNPJ, LicencaSanitaria)
    VALUES
    ('Clínica Ericka Rebouças', 'contato@clinicaericka.com', '(351) 1234-5678', '(351) 98765-4321',
     'Rua das Flores, 100 - Coimbra', 'logo.png', 'favicon.ico', 'logo_rel.png', 'PDF',
     'https://instagram.com/reboucasericka', '123456789', 'LS-2024-001');
END

-- 2. Inserir dados iniciais na tabela de Formas de Pagamento
IF NOT EXISTS (SELECT 1 FROM FormasPagamento)
BEGIN
    INSERT INTO FormasPagamento (Descricao) VALUES
    ('Dinheiro'),
    ('Cartão de Débito'),
    ('Cartão de Crédito'),
    ('PIX'),
    ('Transferência Bancária'),
    ('Cheque'),
    ('Vale Presente'),
    ('Parcelado');
END

-- 3. Inserir dados iniciais na tabela de Categorias de Serviços
IF NOT EXISTS (SELECT 1 FROM CategoriasServicos)
BEGIN
    INSERT INTO CategoriasServicos (Nome, Descricao) VALUES
    ('Extensão de Cílios Fio a Fio', 'Extensão de cílios técnica fio a fio'),
    ('Extensão de Cílios Volume Russo', 'Extensão de cílios técnica volume russo'),
    ('Extensão de Cílios Híbrido', 'Extensão de cílios técnica híbrida'),
    ('Manutenção de Extensão de Cílios', 'Manutenção e retoque de extensões'),
    ('Remoção de Extensão de Cílios', 'Remoção segura de extensões'),
    ('Lash Lifting (Permanente de Cílios)', 'Permanente de cílios naturais'),
    ('Coloração de Cílios', 'Coloração de cílios naturais'),
    ('Design de Sobrancelhas', 'Design e modelagem de sobrancelhas'),
    ('Depilação Facial', 'Depilação de pelos faciais'),
    ('Micropigmentação', 'Micropigmentação de sobrancelhas e lábios'),
    ('Tratamentos Faciais', 'Tratamentos diversos para o rosto');
END

-- 4. Inserir dados iniciais na tabela de Categorias de Produtos
IF NOT EXISTS (SELECT 1 FROM CategoriasProdutos)
BEGIN
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
    ('Primer para Cílios', 'Primers para preparação'),
    ('Produtos para Micropigmentação', 'Produtos específicos para micropigmentação'),
    ('Produtos para Depilação', 'Produtos para depilação facial');
END

-- 5. Inserir dados iniciais na tabela de Fornecedores
IF NOT EXISTS (SELECT 1 FROM Fornecedores)
BEGIN
    INSERT INTO Fornecedores (Nome, Telemovel, Email, Endereco, ContatoResponsavel, Website) VALUES
    ('Beauty Supplies Portugal', '(351) 912345678', 'contato@beautysupplies.pt', 'Rua da Beleza, 123 - Lisboa', 'Maria Silva', 'www.beautysupplies.pt'),
    ('Lash Pro Europe', '(351) 923456789', 'info@lashpro.eu', 'Avenida das Flores, 456 - Porto', 'João Santos', 'www.lashpro.eu'),
    ('Estética Profissional', '(351) 934567890', 'vendas@esteticaprof.pt', 'Rua dos Profissionais, 789 - Coimbra', 'Ana Costa', 'www.esteticaprof.pt'),
    ('Beauty World', '(351) 945678901', 'comercial@beautyworld.pt', 'Praça da Beleza, 321 - Braga', 'Carlos Oliveira', 'www.beautyworld.pt');
END

-- 6. Inserir dados iniciais na tabela de Serviços
IF NOT EXISTS (SELECT 1 FROM Servicos)
BEGIN
    INSERT INTO Servicos (Nome, CategoriaServicoId, Valor, DuracaoMinutos, Descricao, CodigoServico) VALUES
    ('Extensão Fio a Fio - Clássica', 1, 45.00, 120, 'Extensão de cílios fio a fio clássica', 'EXT-FIO-001'),
    ('Extensão Fio a Fio - Volume', 1, 55.00, 150, 'Extensão de cílios fio a fio com volume', 'EXT-FIO-002'),
    ('Extensão Volume Russo - 2D', 2, 65.00, 180, 'Extensão volume russo 2D', 'EXT-VOL-001'),
    ('Extensão Volume Russo - 3D', 2, 75.00, 210, 'Extensão volume russo 3D', 'EXT-VOL-002'),
    ('Extensão Híbrida', 3, 50.00, 150, 'Extensão híbrida (fio a fio + volume)', 'EXT-HIB-001'),
    ('Manutenção Extensão', 4, 25.00, 60, 'Manutenção de extensão de cílios', 'MAN-EXT-001'),
    ('Remoção Extensão', 5, 15.00, 30, 'Remoção segura de extensões', 'REM-EXT-001'),
    ('Lash Lifting', 6, 35.00, 90, 'Permanente de cílios naturais', 'LASH-LIFT-001'),
    ('Coloração Cílios', 7, 20.00, 45, 'Coloração de cílios naturais', 'COL-CIL-001'),
    ('Design Sobrancelhas', 8, 15.00, 30, 'Design e modelagem de sobrancelhas', 'DES-SOB-001'),
    ('Depilação Facial', 9, 12.00, 20, 'Depilação de pelos faciais', 'DEP-FAC-001');
END

-- 7. Inserir dados iniciais na tabela de Produtos
IF NOT EXISTS (SELECT 1 FROM Produtos)
BEGIN
    INSERT INTO Produtos (Nome, Descricao, CategoriaProdutoId, ValorCompra, ValorVenda, Estoque, FornecedorId, CodigoProduto, UnidadeMedida) VALUES
    ('Fios 0.07mm - 8mm', 'Fios para extensão 0.07mm tamanho 8mm', 1, 0.15, 0.25, 1000, 1, 'FIO-007-008', 'UN'),
    ('Fios 0.07mm - 10mm', 'Fios para extensão 0.07mm tamanho 10mm', 1, 0.15, 0.25, 1000, 1, 'FIO-007-010', 'UN'),
    ('Fios 0.07mm - 12mm', 'Fios para extensão 0.07mm tamanho 12mm', 1, 0.15, 0.25, 1000, 1, 'FIO-007-012', 'UN'),
    ('Cola Adesiva Premium', 'Cola adesiva premium para extensões', 2, 8.50, 15.00, 50, 2, 'COL-PREM-001', 'UN'),
    ('Removedor de Cola', 'Removedor de cola para extensões', 3, 12.00, 20.00, 30, 2, 'REM-COL-001', 'UN'),
    ('Pinça Curva Profissional', 'Pinça curva para aplicação de extensões', 4, 25.00, 45.00, 20, 3, 'PIN-CUR-001', 'UN'),
    ('Pinça Reta Profissional', 'Pinça reta para aplicação de extensões', 4, 25.00, 45.00, 20, 3, 'PIN-RET-001', 'UN'),
    ('Microbrush Set', 'Conjunto de microbrush para aplicação', 5, 15.00, 25.00, 50, 3, 'MIC-SET-001', 'SET'),
    ('Pads Protetores', 'Pads protetores para pálpebras', 6, 0.50, 1.00, 500, 4, 'PAD-PRO-001', 'UN'),
    ('Sérum Fortalecedor', 'Sérum fortalecedor para cílios', 7, 18.00, 30.00, 25, 4, 'SER-FOR-001', 'UN'),
    ('Máscara Cílios', 'Máscara especializada para cílios', 8, 22.00, 35.00, 20, 4, 'MAS-CIL-001', 'UN'),
    ('Shampoo Limpeza', 'Shampoo para limpeza de cílios', 9, 15.00, 25.00, 30, 4, 'SHA-LIM-001', 'UN');
END

-- 8. Inserir dados iniciais na tabela de Estéticas
IF NOT EXISTS (SELECT 1 FROM Esteticas)
BEGIN
    INSERT INTO Esteticas (Nome, Endereco, Telemovel, Email, CNPJ, ResponsavelTecnico, LicencaSanitaria, HorarioFuncionamento, CapacidadeMaxima) VALUES
    ('Clínica Ericka Rebouças - Sede', 'Rua das Flores, 100 - Coimbra', '(351) 98765-4321', 'sede@clinicaericka.com', '123456789', 'Ericka Rebouças', 'LS-2024-001', '09:00-18:00', 10),
    ('Clínica Ericka Rebouças - Filial Porto', 'Avenida da Liberdade, 200 - Porto', '(351) 91234-5678', 'porto@clinicaericka.com', '987654321', 'Maria Silva', 'LS-2024-002', '09:00-19:00', 8),
    ('Clínica Ericka Rebouças - Filial Lisboa', 'Rua Augusta, 300 - Lisboa', '(351) 92345-6789', 'lisboa@clinicaericka.com', '456789123', 'João Santos', 'LS-2024-003', '08:00-20:00', 12);
END

-- 9. Inserir dados iniciais na tabela de Planos
IF NOT EXISTS (SELECT 1 FROM Planos)
BEGIN
    INSERT INTO Planos (Nome, Descricao, Valor, QuantidadeSessoes, ValidadeDias) VALUES
    ('Plano Básico - 3 Sessões', 'Plano básico com 3 sessões de extensão', 120.00, 3, 90),
    ('Plano Intermediário - 5 Sessões', 'Plano intermediário com 5 sessões', 200.00, 5, 120),
    ('Plano Premium - 8 Sessões', 'Plano premium com 8 sessões', 300.00, 8, 180),
    ('Plano Anual - 12 Sessões', 'Plano anual com 12 sessões', 400.00, 12, 365),
    ('Plano Manutenção - 4 Sessões', 'Plano de manutenção com 4 sessões', 80.00, 4, 60);
END

-- 10. Inserir dados iniciais na tabela de Log de Ações
IF NOT EXISTS (SELECT 1 FROM LogAcoes)
BEGIN
    INSERT INTO LogAcoes (UsuarioId, TabelaAfetada, Acao, RegistroId, DadosAntigos, DadosNovos, DataHora, IpAddress, UserAgent) VALUES
    (1, 'Configuracoes', 'INSERT', 1, NULL, '{"NomeClinica":"Clínica Ericka Rebouças","Email":"contato@clinicaericka.com"}', GETDATE(), '192.168.1.1', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36');
END

PRINT 'Dados iniciais inseridos com sucesso!';

