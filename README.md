# Salon Management System

Sistema de gestão de salão de beleza com **backend e frontend separados**.

## Estrutura do repositório

```
salon-management-system/
├── backend/           # API REST Laravel 12 + SQLite + Sanctum
├── frontend/          # SPA Vue 3 + Vite + Tailwind CSS
├── docs/              # Documentação (arquitetura, Postman, migração)
├── postman/           # Collections Postman para testar a API
└── legacy-csharp/     # Projeto ASP.NET Core original (apenas referência)
```

| Pasta | Descrição |
|-------|-----------|
| `backend/` | API Laravel — autenticação, clientes, serviços, agendamentos, produtos, stock, vendas |
| `frontend/` | Interface web Vue — painel admin (produtos, stock, caixa) e área do cliente |
| `legacy-csharp/` | Código C# antigo — **não usar em produção**, mantido para consulta na migração |
| `frontend/public/images/` | Imagens estáticas usadas pelo Vue (`/images/...`) |

## Requisitos

- PHP 8.3+ e Composer
- Node.js 20+ e npm
- SQLite (incluído no backend)

## Início rápido

### 1. Backend

```bash
cd backend
composer install
cp .env.example .env
php artisan key:generate
touch database/database.sqlite
php artisan migrate:fresh --seed
php artisan serve
```

API: `http://127.0.0.1:8000`

### 2. Frontend

```bash
cd frontend
npm install
npm run dev
```

App: `http://127.0.0.1:5173`

O Vite faz proxy de `/api` para o backend em desenvolvimento.

## Credenciais de teste

| Perfil | Email | Password |
|--------|-------|----------|
| Admin | admin@salon.test | password |
| Profissional | ana.costa@salon.test | password |
| Cliente | maria.silva@salon.test | password |

## Imagens no frontend

Todas as imagens usadas pela SPA ficam em `frontend/public/images/` e são referenciadas como:

```html
<img src="/images/logo/logo.png" alt="Logo">
```

Subpastas:

- `logo/` — logótipos da marca
- `services/` — imagens ilustrativas de serviços
- `banners/` — banners e sliders
- `placeholders/` — avatares e imagens padrão
- `uploads/` — amostras copiadas do sistema legado

Constantes centralizadas em `frontend/src/lib/images.ts`.

## Etapa 5 — Produtos, Stock e Caixa

Módulos ERP no backend e painel admin:

| Módulo | Rotas API | Páginas admin |
|--------|-----------|---------------|
| Produtos | `GET/POST/PUT/DELETE /api/v1/products` | `/panel/admin/products` |
| Stock | `POST /api/v1/stock/entry\|exit\|adjustment`, `GET history\|low` | `/panel/admin/stock` |
| Caixa | `POST /api/v1/cash/open\|close\|income\|expense`, `GET current\|report` | `/panel/admin/cash` |
| Vendas (POS) | `GET/POST /api/v1/sales` | `/panel/admin/sales` |

Testes: `cd backend && php artisan test --filter=ProductStockCashTest` (10 testes)

> Colunas BD: `price`/`cost_price`/`stock_quantity`/`min_stock` expostas na API também como `sale_price`, `purchase_price`, `current_stock`, `minimum_stock`.

### Navegação do frontend

**Área pública** (sem autenticação): `/`, `/services`, `/products`, `/booking`

**Área autenticada:**

| Destino | Rota |
|---------|------|
| Login | `/login` |
| Painel principal | `/panel` |
| Painel administrativo | `/panel/admin/dashboard` e `/panel/admin/*` |
| Área do cliente | `/panel/client/dashboard` e `/panel/client/*` |

Testes backend: `cd backend && php artisan test`

### Testes da API pública (Etapa 4)

`PublicApiTest` — 6/6 a passar (`php artisan test --filter=PublicApiTest`):

- serviços públicos
- registo de cliente
- profissionais públicos
- agendamento guest
- bloqueio de conflito de horário
- contacto / feedback

O teste de registo valida `clients.user_id` com o ID devolvido pela API, não com um valor fixo.

## Documentação

- [Arquitetura](docs/arquitetura.md)
- [Endpoints Postman](docs/endpoints-postman.md)
- [Migração C# → PHP](docs/migracao-csharp-para-php.md)

## Legado C#

O projeto ASP.NET Core MVC original foi movido para `legacy-csharp/`. Não faz parte da stack ativa. Consulte `legacy-csharp/README.md` para detalhes.

## Stack

| Camada | Tecnologia |
|--------|------------|
| API | Laravel 12, Sanctum, SQLite |
| Frontend | Vue 3, Vite, Tailwind CSS v4, Pinia, Vue Router |
| Testes API | Postman (sem Swagger) |
