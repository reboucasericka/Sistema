# Auditoria de limpeza da raiz — C# duplicado vs `legacy-csharp/`

**Data:** 2026-08-11  
**Objetivo:** Determinar se o ASP.NET/C# na raiz do repositório é duplicata de `legacy-csharp/` e classificar remoções seguras.  
**Método:** Comparação por estrutura, nomes, tamanho e **SHA-256** do conteúdo (não só nomes de pastas).

---

## 1. Contexto e estado Git

### Estrutura observada

| Local | Papel |
|-------|--------|
| Raiz (`Areas/`, `Controllers/`, `Sistema.csproj`, …) | Projeto ASP.NET legado (ainda trackado no Git) |
| `legacy-csharp/` | Cópia de referência do legado (**untracked** no momento da auditoria) |
| `backend/` | Laravel 12 (novo) |
| `frontend/` | Vue 3 (novo) |

### `git status` (antes da limpeza)

Itens relevantes untracked: `backend/`, `frontend/`, `legacy-csharp/`, `docs/`, `postman/`, `README.md`, `.vscode/settings.json`.

Candidatos C# da raiz: **trackados e sem modificações locais** (sem dirty nos paths candidatos).

### Aviso de segurança

`legacy-csharp/` **ainda não está commitado**. A remoção dos ficheiros C# da raiz só é segura no working tree enquanto `legacy-csharp/` existir localmente. **Recomenda-se commitar `legacy-csharp/` antes ou no mesmo commit da limpeza**, para o histórico remoto/HEAD não ficar só com a eliminação sem a cópia preservada.

**Não** se executou `git clean` nem `git reset --hard`.  
**Não** se altera `legacy-csharp/`, `backend/` nem `frontend/` (exceto validação de testes/build).

---

## 2. Resultado da comparação SHA-256

### Pastas C# da raiz vs `legacy-csharp/`

| Pasta raiz | Ficheiros raiz | Ficheiros legacy | Idênticos (hash) | Só raiz | Só legacy | Conteúdo diferente | Igual? |
|------------|----------------|------------------|------------------|---------|-----------|--------------------|--------|
| `Areas/` | 181 | 181 | 181 | 0 | 0 | 0 | **Sim** |
| `Attributes/` | 1 | 1 | 1 | 0 | 0 | 0 | **Sim** |
| `Controllers/` | 6 | 6 | 6 | 0 | 0 | 0 | **Sim** |
| `Data/` | 65 | 65 | 65 | 0 | 0 | 0 | **Sim** |
| `Helpers/` | 16 | 16 | 16 | 0 | 0 | 0 | **Sim** |
| `Migrations/` | 4 | 4 | 4 | 0 | 0 | 0 | **Sim** |
| `Models/` | 38 | 38 | 38 | 0 | 0 | 0 | **Sim** |
| `Properties/` | 5 | 5 | 5 | 0 | 0 | 0 | **Sim** |
| `Services/` | 10 | 10 | 10 | 0 | 0 | 0 | **Sim** |
| `Views/` | 32 | 32 | 32 | 0 | 0 | 0 | **Sim** |
| `wwwroot/` | 232 | 242 | 231 | 0 | 10 | 1 | **Não** |
| `.config/` | 1 | 1 | 1 | 0 | 0 | 0 | **Sim** |

### Ficheiros C# da raiz

| Ficheiro | Existe em legacy | Tamanho | Hash igual? |
|----------|------------------|---------|-------------|
| `Program.cs` | Sim | 10883 | **Sim** |
| `Sistema.csproj` | Sim | 4144 | **Sim** |
| `Sistema.sln` | Sim | 1119 | **Sim** |
| `appsettings.json` | Sim | 387 | **Sim** |

### Confirmação de natureza ASP.NET

- `Models/`, `Controllers/`, `Services/`: extensões `.cs`, namespaces `Sistema.*`, ASP.NET / EF.
- `Migrations/`: Entity Framework (`Microsoft.EntityFrameworkCore.Migrations`, `Sistema.Migrations`). **Não** confundir com `backend/database/migrations` (Laravel).
- `.config/dotnet-tools.json`: ferramenta `dotnet-ef` 9.0.9 — tooling ASP.NET.

---

## 3. `wwwroot/` — auditoria especial

### Diferenças vs `legacy-csharp/wwwroot/`

**Só em legacy (10 ficheiros — raiz não os tem):**

- `images/professionals/1.jpg`
- `images/professionals/2.jpg`
- `images/professionals/3.jpg`
- `images/professionals/4.jpg`
- `images/professionals/a.png`
- `images/professionals/i1.png`
- `images/professionals/i2.png`
- `images/professionals/i3.png`
- `images/professionals/i4.png`
- `images/professionals/img1.png`

**Conteúdo diferente (mesmo path):**

| Ficheiro | Raiz | Legacy | Frontend `public/` |
|----------|------|--------|---------------------|
| `favicon.ico` | 5430 bytes, hash `26dc5ff4bfb9…` | 15406 bytes, hash `ca1aec70744d…` | **Igual ao legacy** |

### Assets exclusivos da raiz

- Por **path**: nenhum ficheiro existe só na raiz.
- Por **conteúdo (hash)**: o `favicon.ico` da raiz **não** existe em `legacy-csharp/` nem em `frontend/public/`.

### Decisão `wwwroot/`

**REVISÃO MANUAL / DIFERENTE — MANTER** nesta limpeza.

Motivo: perder-se-ia a variante exclusiva do `favicon.ico` da raiz. Os restantes 231 ficheiros estão preservados em `legacy-csharp/wwwroot/` com hash idêntico; muitos assets de imagem/CSS do ASP.NET **não** estão em `frontend/public/`, mas estão no legacy.

---

## 4. Dependências (Laravel / Vue / tooling)

Pesquisa em `backend/`, `frontend/`, `docs/`, `postman/`, `.vscode/`, `README.md`, `.gitignore`, `.gitattributes`, com cuidado para **não** confundir:

- `backend/app/Models` ≠ `/Models`
- `backend/database/migrations` ≠ `/Migrations`
- `backend/app/Http/Controllers` ≠ `/Controllers`
- `backend/app/Services` ≠ `/Services`

### Dependências de runtime Laravel/Vue aos paths da raiz

**Nenhuma.** `backend/` e `frontend/` não referenciam `Sistema.csproj`, `Sistema.sln`, `./Areas/`, `./Controllers/`, `./Models/`, `./wwwroot/`, etc.

### Referências encontradas (não são dependência de runtime do novo sistema)

| Local | Tipo | Nota |
|-------|------|------|
| `.vscode/tasks.json` | Build tooling | `dotnet build/publish/watch` → `${workspaceFolder}/Sistema.csproj` |
| `.vscode/launch.json` | Debug ASP.NET | `Sistema.dll`, `ASPNETCORE_ENVIRONMENT`, map `/Views` |
| `.vscode/settings.json` | IDE | Postman dotenv notification — **útil, manter** |
| `docs/LEGACY_MIGRATION_AUDIT.md` | Documentação | Descreve legado; menciona tasks quebradas |
| `docs/migracao-csharp-para-php.md` | Documentação | Referências históricas a `Areas/` |
| `.gitignore` / `.gitattributes` | Meta | Comentários Visual Studio / wwwroot — genéricos |

**Conclusão:** o novo sistema **não** depende dos caminhos C# da raiz. Apenas `.vscode` precisa de correção antes de remover `Sistema.csproj`.

---

## 5. Tabela de classificação

| Item raiz | Existe em legacy-csharp | Igual? | Necessário Laravel/Vue? | Decisão |
|-----------|-------------------------|--------|-------------------------|---------|
| `Areas/` | Sim | Sim | Não | **DUPLICADO — SEGURO REMOVER** |
| `Attributes/` | Sim | Sim | Não | **DUPLICADO — SEGURO REMOVER** |
| `Controllers/` | Sim | Sim | Não | **DUPLICADO — SEGURO REMOVER** |
| `Data/` | Sim | Sim | Não | **DUPLICADO — SEGURO REMOVER** |
| `Helpers/` | Sim | Sim | Não | **DUPLICADO — SEGURO REMOVER** |
| `Migrations/` | Sim | Sim (EF) | Não | **DUPLICADO — SEGURO REMOVER** |
| `Models/` | Sim | Sim | Não | **DUPLICADO — SEGURO REMOVER** |
| `Properties/` | Sim | Sim | Não | **DUPLICADO — SEGURO REMOVER** |
| `Services/` | Sim | Sim | Não | **DUPLICADO — SEGURO REMOVER** |
| `Views/` | Sim | Sim | Não | **DUPLICADO — SEGURO REMOVER** |
| `wwwroot/` | Sim | Sim* (2b) | Não | **DUPLICADO — REMOVIDO (etapa 2b)** |
| `Program.cs` | Sim | Sim | Não | **DUPLICADO — SEGURO REMOVER** |
| `Sistema.csproj` | Sim | Sim | Não* | **DUPLICADO — SEGURO REMOVER** |
| `Sistema.sln` | Sim | Sim | Não | **DUPLICADO — SEGURO REMOVER** |
| `appsettings.json` | Sim | Sim | Não | **DUPLICADO — SEGURO REMOVER** |
| `.config/` | Sim | Sim | Não | **DUPLICADO — SEGURO REMOVER** |
| `.vscode/` | Parcial | Diferente | Parcial | **Corrigir** (remover tasks ASP.NET; manter settings) |
| `.gitignore` | — | — | Sim | **NÃO LEGADO — MANTER** |
| `.gitattributes` | — | — | Sim | **NÃO LEGADO — MANTER** |
| `backend/` | — | — | Sim | **USADO PELO NOVO SISTEMA — MANTER** |
| `frontend/` | — | — | Sim | **USADO PELO NOVO SISTEMA — MANTER** |
| `legacy-csharp/` | — | — | Referência | **MANTER (proibido apagar)** |
| `docs/` | — | — | Sim | **NÃO LEGADO — MANTER** |
| `postman/` | — | — | Sim | **NÃO LEGADO — MANTER** |
| `README.md` | — | — | Sim | **NÃO LEGADO — MANTER** |

\*Após corrigir `.vscode/tasks.json` / `launch.json`.

---

## 6. Lista exata a remover (etapa 2)

Somente itens **DUPLICADO — SEGURO REMOVER**:

### Diretórios

1. `Areas/`
2. `Attributes/`
3. `Controllers/`
4. `Data/`
5. `Helpers/`
6. `Migrations/`
7. `Models/`
8. `Properties/`
9. `Services/`
10. `Views/`
11. `.config/`

### Ficheiros

1. `Program.cs`
2. `Sistema.csproj`
3. `Sistema.sln`
4. `appsettings.json`

### Explicitamente NÃO remover

- `legacy-csharp/` (inteira)
- `backend/`
- `frontend/`
- `wwwroot/` (revisão manual — favicon exclusivo)
- `.vscode/` (pasta; apenas limpar configs ASP.NET)
- `.git/`, `.gitignore`, `.gitattributes`
- `docs/`, `postman/`, `README.md`

---

## 7. Alterações previstas em `.vscode`

| Ficheiro | Ação |
|----------|------|
| `tasks.json` | Remover tasks `dotnet build/publish/watch` sobre `Sistema.csproj`; deixar `tasks: []` |
| `launch.json` | Remover configs `.NET Core` / ASP.NET; deixar `configurations: []` |
| `settings.json` | **Manter** (Postman / IDE) |

Após limpeza: nenhuma task da raiz deve executar `dotnet build`, `dotnet run` ou referenciar `Sistema.csproj`.

Cópia ASP.NET destas configs permanece em `legacy-csharp/.vscode/`.

---

## 8. Itens para revisão manual (pendentes)

1. **Commit de `legacy-csharp/`** — garantir que a referência entra no Git (ainda untracked na altura da limpeza).
2. **`.gitignore` da raiz** — ainda no estilo Visual Studio; pode ser modernizado depois (fora deste scope).

> `wwwroot/` da raiz: resolvido na **etapa 2b** (secção 11).

---

## 9. Veredicto

O C# da raiz **é, na prática, duplicado** de `legacy-csharp/` para todo o código e configs ASP.NET comparados, **com a exceção documentada de `wwwroot/favicon.ico`** (e com legacy a ter 10 imagens a mais em `professionals/`).

Remoção controlada da etapa 2 aplica-se **apenas** à lista da secção 6, após correção do `.vscode`.

---

## 10. Resultado da limpeza (etapa 2 executada)

### Removido da raiz

**Diretórios:** `Areas/`, `Attributes/`, `Controllers/`, `Data/`, `Helpers/`, `Migrations/`, `Models/`, `Properties/`, `Services/`, `Views/`, `.config/`

**Ficheiros:** `Program.cs`, `Sistema.csproj`, `Sistema.sln`, `appsettings.json`

### Mantido (após etapa 2; atualizado na etapa 2b)

- `legacy-csharp/` — intacta (inclui `wwwroot/` completo)
- `backend/`, `frontend/`, `docs/`, `postman/`, `README.md`
- `.gitignore`, `.gitattributes`, `.vscode/` (tasks/launch limpos)

### `.vscode`

- `tasks.json` → `tasks: []` (sem `dotnet` / `Sistema.csproj`)
- `launch.json` → `configurations: []`
- `settings.json` preservado

### Validação

| Comando | Resultado |
|---------|-----------|
| `cd backend && php artisan test` | **77 passed** (360 assertions) |
| `cd frontend && npm run build` | **OK** (vite build sucesso) |

### Estrutura final da raiz (após etapa 2b)

```
salon-management-system/
├── .git/
├── .gitattributes
├── .gitignore
├── .vscode/
├── backend/
├── docs/
├── frontend/
├── legacy-csharp/
├── postman/
└── README.md
```

### Nota operacional (etapa 2)

Após a remoção do C#, `git status` chegou a listar deletes espúrios sob `wwwroot/` sem esses paths terem sido alvo do `rm`. Foi feito `git restore wwwroot/` na altura.

---

## 11. Etapa 2b — remoção de `wwwroot/` da raiz (2026-08-11)

### Reanálise

| Critério | Resultado |
|----------|-----------|
| Ficheiros na raiz `wwwroot/` | 231 (favicon já ausente no disco) |
| Paths em falta em `legacy-csharp/wwwroot/` | **0** |
| Diferenças de hash vs legacy | **0** |
| Conteúdo exclusivo vs legacy + `frontend/public/` | **0** |
| Dependências Laravel/Vue à `wwwroot/` da raiz | **Nenhuma** |

### Destino do favicon que era “exclusivo”

O hash `26dc5ff4bfb9…` (5430 bytes), antes só na raiz, está preservado em:

- `frontend/public/favicon.ico` (sistema novo)

A variante do legado continua em:

- `legacy-csharp/wwwroot/favicon.ico` (15406 bytes, hash `ca1aec70744d…`)

### Decisão

**DUPLICADO — SEGURO REMOVER** → `wwwroot/` removida da raiz.

### Confirmação pós-remoção

- `legacy-csharp/wwwroot/` intacta
- `frontend/public/favicon.ico` presente (5430 bytes)
- Estrutura da raiz alinhada com a referência desejada (sem `wwwroot/` na raiz)
