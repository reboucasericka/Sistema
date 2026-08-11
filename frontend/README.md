# Frontend — Salon Management System

Vue 3 SPA separada do backend Laravel.

## Requisitos

- Node.js 20+
- Backend Laravel a correr em `http://127.0.0.1:8000`

## Instalação

```bash
npm install
cp .env.example .env
```

## Desenvolvimento

```bash
npm run dev
```

Abrir `http://127.0.0.1:5173`

## Build produção

```bash
npm run build
npm run preview
```

Em produção, definir `VITE_API_URL` com a URL pública da API (ex: `https://api.seudominio.com`).

## Rotas do site

### Área pública (sem login)

| Rota | Descrição |
|------|-----------|
| `/` | Home pública |
| `/services` | Catálogo de serviços |
| `/products` | Catálogo de produtos |
| `/booking` | Landing de agendamento |
| `/booking/flow` | Wizard completo (serviço → profissional → horário → dados → confirmar) |
| `/professionals` | Equipa de profissionais |
| `/professionals/:id` | Perfil do profissional |
| `/contact` | Formulário de contacto |
| `/prices` | Tabela de preços (serviços) |
| `/about` | Sobre nós |
| `/academy` | Academia Ewellin |
| `/recruitment` | Recrutamento |
| `/services/:id` | Detalhe do serviço |
| `/register` | Registo de cliente |
| `/*` inexistente | Página 404 pública |

### Área autenticada

| Rota | Perfis | Descrição |
|------|--------|-----------|
| `/login` | convidado | Login |
| `/panel` | admin, professional | Painel principal |
| `/panel/admin/*` | admin, professional | Painel administrativo |
| `/panel/admin/messages` | admin | Mensagens de contacto recebidas |
| `/panel/client/*` | client | Área do cliente |

## API pública consumida pelo frontend

| Endpoint | Uso |
|----------|-----|
| `GET /api/v1/public/services` | Catálogos e booking |
| `GET /api/v1/public/products` | Catálogo produtos |
| `GET /api/v1/public/professionals` | Página profissionais |
| `GET /api/v1/public/availability` | Horários disponíveis |
| `POST /api/v1/public/appointments` | Criar agendamento (guest ou autenticado) |
| `POST /api/v1/public/contact` | Formulário de contacto |
| `POST /api/v1/auth/register` | Registo (sempre role `client`) |

## Login de teste

| Perfil | Email | Password | Destino após login |
|--------|-------|----------|-------------------|
| Admin | admin@salon.test | password | `/panel` |
| Profissional | ana.costa@salon.test | password | `/panel` |
| Cliente | maria.silva@salon.test | password | `/panel/client/dashboard` |

## Rotas do painel

| Rota | Perfis | Descrição |
|------|--------|-----------|
| `/panel` | admin, professional | Painel principal do sistema |
| `/panel/admin/dashboard` | admin, professional | Dashboard administrativo |
| `/panel/admin/*` | conforme módulo | Módulos admin (clientes, serviços, produtos, etc.) |
| `/panel/admin/messages` | admin | Mensagens de contacto |
| `/panel/admin/products` | admin, professional | Produtos |
| `/panel/admin/stock` | admin, professional | Movimentos de stock |
| `/panel/admin/sales` | admin, professional | Vendas (POS) |
| `/panel/admin/cash` | admin, professional | Caixa |
| `/panel/client/dashboard` | client | Área do cliente |
| `/panel/client/appointments` | client | Agendamentos do cliente |

Rotas antigas `/admin/*` e `/client/*` redirecionam automaticamente para a nova estrutura.

## Autenticação

- Login via `POST /api/v1/auth/login`
- Registo via `POST /api/v1/auth/register`
- Token Bearer guardado em `localStorage`
- Interceptor Axios adiciona `Authorization` automaticamente
- Rotas protegidas por perfil (`admin`, `professional`, `client`)
- Site público usa `body.legacy-site` + Bootstrap/Slick; painel usa Tailwind isolado

| `/panel/admin/sales` | admin, professional | Vendas (POS) |
| `/panel/admin/cash` | admin, professional | Caixa |

## Testes (Etapa 5 — ERP)

`ProductStockCashTest` — 10 testes (`php artisan test --filter=ProductStockCashTest`):

- CRUD produtos (com aliases ERP e barcode)
- Entrada / saída / ajuste de stock
- Bloqueio de stock negativo
- Alerta stock mínimo (`GET /stock/low`)
- Histórico de movimentos
- Abrir / fechar caixa
- Receitas e despesas
- Relatório diário

## Testes (Etapa 4 — API pública)

Ficheiro: `backend/tests/Feature/PublicApiTest.php`

```bash
cd backend && php artisan test --filter=PublicApiTest
```

**6/6 testes a passar:**

| Teste | Área |
|-------|------|
| Serviços públicos | `GET /api/v1/public/services` |
| Registo de cliente | `POST /api/v1/auth/register` (role `client`) |
| Profissionais públicos | `GET /api/v1/public/professionals` |
| Agendamento guest | `POST /api/v1/public/appointments` |
| Conflito de horário | rejeição 422 quando slot ocupado |
| Contacto / feedback | `POST /api/v1/public/contact` |

O teste de registo usa o `user.id` da resposta JSON — não assumir `user_id` fixo, pois outros registos no `setUp()` podem ocupar IDs anteriores.
