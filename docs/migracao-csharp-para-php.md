# Migração C# → PHP (Laravel)

## Contexto

O projeto original era um **monólito ASP.NET Core 9 MVC** com:

- Razor views (`Areas/Admin`, `Areas/Public`)
- Entity Framework + SQL Server
- ASP.NET Identity (cookies)
- API parcial para mobile (`ApiAppointmentsController`, `ClientsController`)
- SignalR para notificações

## Nova arquitetura

| Antes (.NET) | Depois (Laravel) |
|--------------|------------------|
| `SistemaDbContext` | Eloquent Models + Migrations |
| `Controllers/API/*` | `app/Http/Controllers/Api/V1/*` |
| `Areas/Admin/*` | Vue 3 SPA (futuro `frontend/`) |
| `IdentityUser` | `User` + enum `UserRole` |
| Cookies | Sanctum Bearer tokens |
| `Repository/*` | Eloquent + Services |
| `ViewModels` | Form Requests + API Resources |
| SQL Server | SQLite |
| SignalR | Notificações REST (WebSocket futuro) |

## Mapeamento de entidades

| C# (`Data/Entities`) | Laravel (`app/Models`) | Notas |
|----------------------|------------------------|-------|
| `User` | `User` | `first_name`, `last_name`, `role` |
| `Customer` | `Client` | Nome alinhado à API REST |
| `Professional` | `Professional` | FK `user_id` |
| `Service` | `Service` | `duration_minutes` em vez de string |
| `Appointment` | `Appointment` | Status em lowercase |
| `Notification` | `Notification` | Simplificado na v1 |

## Mapeamento de endpoints

| .NET (legado) | Laravel (novo) |
|---------------|--------------|
| `POST /api/ApiAppointments/create` | `POST /api/v1/appointments` |
| `PUT /api/ApiAppointments/update/{id}` | `PUT /api/v1/appointments/{id}` |
| `DELETE /api/ApiAppointments/cancel/{id}` | `DELETE /api/v1/appointments/{id}` |
| `GET /api/ApiAppointments/professional/{id}` | `GET /api/v1/appointments?professional_id={id}` |
| `GET /api/Clients` | `GET /api/v1/clients` |
| `GET /Public/PublicBooking/GetAvailableSlots` | `GET /api/v1/public/availability` |
| `POST /Public/PublicAppointment/CreateAppointment` | `POST /api/v1/public/appointments` |
| `GET /Public/PublicProfessionals/Index` | `GET /api/v1/public/professionals` |
| `POST /Public/PublicAccount/Register` | `POST /api/v1/auth/register` (sempre client) |
| *(contacto público — novo)* | `POST /api/v1/public/contact` |
| *(não existia)* | `GET /api/v1/contact-messages` (admin) |

## Melhorias na migração

1. **Autenticação obrigatória** — a API .NET não tinha `[Authorize]` nos endpoints mobile
2. **DTOs/Form Requests** — não aceita entidade completa no body
3. **Cálculo de `end_time`** — backend calcula com base na duração do serviço
4. **Conflito de horários** — `AppointmentService::ensureNoScheduleConflict`
5. **Policies por role** — admin, profissional e cliente com permissões distintas

## Estratégia de migração por fases

### Fase 1 — Backend API (concluída nesta etapa)
- Laravel 12 + SQLite + Sanctum
- Módulos: auth, users, clients, professionals, services, appointments, dashboard, notifications

### Fase 2 — Frontend Vue 3
- Painel admin e painel cliente
- Consumo da API via Axios/fetch
- Rotas protegidas por role

### Fase 3 — Módulos avançados
- Produtos, stock, caixa
- PDF/Excel
- Google Calendar
- WebSockets

### Fase 4 — Funcionalidades públicas (concluída)
- Agendamento público completo (serviço → profissional → data/hora → dados → confirmar)
- Registo de cliente (`role` sempre `client`)
- Listagem pública de profissionais
- Formulário de contacto + painel admin de mensagens
- Testes PHPUnit em `backend/tests/Feature/PublicApiTest.php` — **6/6 a passar**

#### Testes `PublicApiTest` (6/6)

Executar: `cd backend && php artisan test --filter=PublicApiTest`

| Teste | Cobertura |
|-------|-----------|
| `test_public_can_list_active_services` | Serviços públicos |
| `test_client_registration_always_creates_client_role` | Registo de cliente |
| `test_public_can_list_active_professionals` | Profissionais públicos |
| `test_public_can_create_appointment_as_guest` | Agendamento guest |
| `test_public_booking_rejects_schedule_conflict` | Bloqueio de conflito de horário |
| `test_public_can_submit_contact_message` | Contacto / feedback |

> **Nota de implementação:** o teste de registo não deve assumir `user_id` fixo (ex.: `1`), porque o `setUp()` cria utilizadores de apoio (ex.: profissional) que ocupam IDs anteriores. Usar o `user.id` devolvido pela resposta da API ao validar o registo em `clients`.

### Fase 5 — Produtos, Stock e Caixa ERP (concluída)
- Produtos com barcode, factory, aliases API (`sale_price`, `current_stock`, etc.)
- Stock: endpoints `/api/v1/stock/entry|exit|adjustment|history|low`
- Caixa: `cash_registers` + `cash_transactions`, `CashService`
- Vue: `ProductsView`, `StockView`, `CashView`, `SalesView`
- Testes: `ProductStockCashTest` (10/10)

### Fase 6 — Descomissionar .NET
- Validar paridade funcional
- Migrar dados SQL Server → SQLite/MySQL se necessário

## Dados de seed vs produção

O seeder `SalonSeeder` cria dados de demonstração equivalentes ao `SeedDb` do .NET, com 3 perfis de utilizador e um agendamento de exemplo.

## Código legado

O projeto C# permanece na raiz do repositório como referência durante a migração. Não será removido até a Fase 4.
