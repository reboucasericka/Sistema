# Legado ASP.NET Core

Este diretório contém o **projeto original C# / ASP.NET Core MVC** do salon-management-system.

## Estado

- **Arquivado** — não é usado em produção
- Mantido apenas como **referência** durante a migração para Laravel + Vue
- Não recebe novas funcionalidades

## Conteúdo principal

| Item | Descrição |
|------|-----------|
| `Sistema.csproj` / `Sistema.sln` | Projeto Visual Studio |
| `Program.cs` | Entry point ASP.NET |
| `Areas/` | Admin e Public (MVC) |
| `Controllers/` | MVC + API parcial |
| `Data/` | Entity Framework, entidades, repositórios |
| `Views/` | Razor views |
| `wwwroot/` | Assets estáticos (css, js, images, uploads) |

## Imagens

As imagens originais permanecem em `wwwroot/images/`, `wwwroot/img/` e `wwwroot/uploads/`.

As imagens necessárias ao **novo frontend Vue** foram copiadas para:

```
frontend/public/images/
```

## Executar o legado (opcional)

Requer .NET 9 SDK:

```bash
cd legacy-csharp
dotnet restore
dotnet run
```

## Migração

Ver `docs/migracao-csharp-para-php.md` na raiz do repositório.
