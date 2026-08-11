# Auditoria Final do Legado — Salon Management System

**Data:** 2026-08-11  
**Âmbito:** `legacy-csharp/` (somente leitura) × stack ativa Laravel 12 + Vue 3  
**Tipo:** auditoria funcional completa — **sem implementação, sem remoção, sem alteração de código de aplicação**  
**Objetivo:** decidir se `legacy-csharp/` pode ser eliminada

---

## Veredicto executivo

### PODE APAGAR `legacy-csharp` AGORA?

# **NÃO**

A aplicação Laravel/Vue **não depende em runtime** do C# para arrancar.  
Contudo, a pasta ainda é a **única fonte de regras e módulos** não migrados (ERP financeiro, relatórios, auth avançada, comissões operacionais, integrações, etc.). Removê-la agora destruiria a referência necessária à migração.

**Recomendação:** manter `legacy-csharp/` até fechar os bloqueadores de prioridade Alta/Média ou documentar descarte oficial de cada um.

---

## 1. Inventário completo do legado

### 1.1 Stack e estrutura

| Área | Conteúdo |
|------|----------|
| Tipo | ASP.NET Core MVC monólito |
| ORM | Entity Framework (`SistemaDbContext`) |
| Auth | ASP.NET Identity (`User` : `IdentityUser`) |
| UI | Razor (`Areas/Admin`, `Areas/Public`, `Views/`) |
| API | `Controllers/API/*` + `PublicApiCalendarController` |
| Serviços | Email, PDF, Excel, Google Calendar, notificações, lembretes, backup, PDV/caixa |
| Assets | `wwwroot/` (js PDV, charts, uploads paths) |

### 1.2 Contagens (excluindo `bin/`, `obj/`, libs geradas)

| Artefacto | Quantidade aproximada |
|-----------|----------------------:|
| Controllers | **43** |
| Entidades `Data/Entities` | **36** (+ `IEntity`) |
| ViewModels `Models/**` | **38** |
| Services | **10** |
| Repository interfaces | **12** |
| Views Razor (`.cshtml`) | **176** |
| Migrations EF | presentes em `Migrations/` |

### 1.3 Módulos funcionais encontrados no legado

1. Autenticação / Identity / roles  
2. Dashboard admin  
3. Clientes (vários controllers redundantes)  
4. Profissionais + horários + serviços associados  
5. Serviços + categorias  
6. Produtos + categorias + fornecedores  
7. Stock (entrada/saída/ajuste/movimentos)  
8. Agendamentos admin + público + API  
9. Booking / reagendamento / cancelamento  
10. Caixa + movimentos + PDV  
11. Vendas (`Sale`/`SaleItem` — parcialmente usado no PDV)  
12. ERP: Payables, Receivables, PaymentMethods  
13. PriceTables + Settings  
14. Relatórios + Excel + PDF  
15. Comissões (campos + relatório via Payables)  
16. Notificações + email + lembretes  
17. Feedback / ServiceReview  
18. Planos (`Plan` / `PlanAppointment`)  
19. Histórico de procedimentos  
20. Site público (home, about, academy, recruitment, prices, products, professionals)  
21. Painel cliente / painel profissional  
22. Google Calendar + social login  
23. Uploads de imagens  
24. AccessLog / Backup / BeautyCenter / Billing legado  

---

## 2. Entidades — mapeamento e estado

| Entidade C# | Tabela | Campos importantes | Laravel | Estado |
|-------------|--------|--------------------|---------|--------|
| `User` | Users / AspNetUsers | FirstName, LastName, Active, ImageId, Identity | `User` | 🟡 PARCIAL (sem reset UI, sem social, sem email activation) |
| `Customer` | Customers | Name, Email, Phone, Address, BirthDate, Notes, AllergyHistory, ImageId, UserId | `Client` | 🟡 PARCIAL (sem imagem cliente) |
| `Professional` | Professionals | Specialty, Commission%, DefaultCommission, ImageId, UserId | `Professional` | 🟡 PARCIAL (commission global sim; painel próprio não) |
| `ProfessionalSchedule` | ProfessionalSchedules | DayOfWeek, Start/End, ProfessionalId | `ProfessionalSchedule` | ✅ MIGRADO |
| `ProfessionalService` | ProfessionalServices | ProfessionalId, ServiceId, **Commission** | `professional_service` pivot | 🟡 PARCIAL (**sem coluna commission**) |
| `Service` | Service | Price, Duration, Commission, ReturnDays, ImageId, CategoryId | `Service` | 🟡 PARCIAL (sem commission/return_days no model) |
| `Category` | Category | Name, Description | `ServiceCategory` | ✅ MIGRADO (melhorado: slug, is_active, sort_order) |
| `Product` | Products | Prices, Stock, MinStock, Image, SupplierId | `Product` | 🟡 PARCIAL (sem SupplierId) |
| `ProductCategory` | ProductCategories | Name | `ProductCategory` | ✅ MIGRADO (melhorado) |
| `Supplier` | Suppliers | Name, TaxId, Phone, Email… | — | ❌ NÃO MIGRADO |
| `Appointment` | Appointments | Start/End, Status, Notes, TotalPrice, ReminderSent, GoogleEventId | `Appointment` | 🟡 PARCIAL (sem GoogleEventId; reminder_sent só campo) |
| `Sale` | Sales | Customer, Professional, PaymentMethod, Discount, Totals | `Sale` | 🟡 PARCIAL (enum payment; discount limitado) |
| `SaleItem` | SaleItems | ProductId, Qty, Prices | `SaleItem` | 🔵 SUBSTITUÍDO+ (também `service_id`) |
| `StockMovement` | StockMovements | Product, Qty, Type, User, Supplier | `StockMovement` | ✅ MIGRADO (unificado) |
| `StockEntry` / `StockExit` | StockEntries / StockExits | Entrada/saída dedicadas | — | 🔵 SUBSTITUÍDO por `StockMovement` |
| `CashRegister` | CashRegisters | Open/Close users, Initial/Final | `CashRegister` | ✅ MIGRADO |
| `CashMovement` | CashMovements | Type, Amount, Description | `CashTransaction` | ✅ MIGRADO (renomeado) |
| `Payable` | Payables | Amount, DueDate, Type, Supplier, Commission… | — | ❌ NÃO MIGRADO |
| `Receivable` | Receivables | Amount, Customer, Service, Sale… | — | ❌ NÃO MIGRADO |
| `PaymentMethod` | PaymentMethods | Name, IsActive | `Enums\PaymentMethod` | 🔵 SUBSTITUÍDO (enum fixo, sem CRUD) |
| `PriceTable` | PriceTables | Category, ServiceName, Price | — / `PublicPricesView` | 🔵 SUBSTITUÍDO (preços via `services`) |
| `Setting` | Settings | Clinic, phones, logos, commissions defaults, hours | — | ❌ NÃO MIGRADO |
| `Notification` | Notifications | Message, Type, IsRead | `Notification` | 🟡 PARCIAL (REST; sem SignalR) |
| `Reminder` | Reminder | AppointmentId, SendDate, Method | — | ❌ NÃO MIGRADO (só `reminder_sent`) |
| `Feedback` | Feedbacks | Rating, Comment, Appointment | — | ❌ NÃO MIGRADO |
| `ServiceReview` | ServiceReviews | Rating, Comments | — | ❌ NÃO MIGRADO |
| `Plan` / `PlanAppointment` | Plans / PlanAppointments | Sessões, validade | — | ❌ NÃO MIGRADO |
| `ProcedureHistory` | ProcedureHistories | Material, observações técnicas, preço | — | ❌ NÃO MIGRADO |
| `Billing` / `BillingDetails` | Billings / BillingDetails | Billing legado | — | ❓ DECISÃO (parece obsoleto vs Sale) |
| `BeautyCenter` | BeautyCenters | Dados multi-centro | — | ❓ DECISÃO |
| `AccessLog` | AccessLog | Auditoria de ações | — | ❓ DECISÃO |
| `Profile` | Profiles | Perfis Identity paralelos | — | ⚪ DESCARTÁVEL (roles enum) |
| `Schedule` (`Shedule.cs`) | Schedule | Horário legado paralelo | — | ⚪ DESCARTÁVEL (duplicado de ProfessionalSchedule) |
| `Payable` commission type | via Payables | Comissões a pagar | — | ❌ NÃO MIGRADO |

---

## 3. Controllers — inventário e classificação

### 3.1 Admin (24)

| Controller | Funcionalidade | Laravel / Vue | Estado |
|------------|----------------|---------------|--------|
| `AdminController` | Dashboard + charts | `DashboardController` / `DashboardView` | 🟡 PARCIAL |
| `AdminAppointmentsController` | CRUD + slots + status + Google | `AppointmentController` + `AppointmentService` / `AppointmentsView` | 🟡 PARCIAL |
| `AdminProfessionalsController` | CRUD + foto + toggle + user | `ProfessionalController` / `ProfessionalsView` | 🟡 PARCIAL |
| `AdminProfessionalSchedulesController` | CRUD horários | `ProfessionalScheduleController` / `ProfessionalSchedulesView` | ✅ MIGRADO |
| `AdminServiceProfessionalsController` | Vínculo profissional↔serviço | `ProfessionalServiceController` | 🟡 PARCIAL (sem commission) |
| `AdminServicesController` | CRUD serviços + imagem | `ServiceController` / `ServicesView` | 🟡 PARCIAL |
| `AdminServiceCategoriesController` | CRUD categorias serviço | `ServiceCategoryController` / `CategoriesView` | ✅ MIGRADO |
| `AdminProductsController` | CRUD produtos + imagem + supplier | `ProductController` / `ProductsView` | 🟡 PARCIAL |
| `AdminProductCategoriesController` | CRUD categorias produto | `ProductCategoryController` / `CategoriesView` | ✅ MIGRADO |
| `AdminStockController` | Entry/Output/Adjust/Low | `StockController` / `StockView` | ✅ MIGRADO |
| `AdminCashRegisterController` | Caixa + PDV | `CashController` + `SaleController` / `CashView` + `SalesView` | 🟡 PARCIAL |
| `AdminCashMovementsController` | Movimentos caixa | `CashController` | ✅ MIGRADO |
| `AdminCustomerController` | Clientes avançado | `ClientController` / `ClientsView` | 🟡 PARCIAL |
| `AdminClientsController` | CRUD clientes simples | idem | 🔵 SUBSTITUÍDO |
| `AdminClientManagementController` | Gestão + report cliente | parcial em Clients + Dashboard | 🟡 PARCIAL |
| `AdminClientController` | Perfil cliente + appointments | área client Vue | 🟡 PARCIAL |
| `AdminNotificationsController` | CRUD/read notifications | `NotificationController` | 🟡 PARCIAL |
| `AdminSuppliersController` | CRUD fornecedores | — | ❌ NÃO MIGRADO |
| `AdminPayablesController` | Contas a pagar | — | ❌ NÃO MIGRADO |
| `AdminReceivablesController` | Contas a receber | — | ❌ NÃO MIGRADO |
| `AdminPaymentMethodController` | Métodos pagamento CRUD | enum `PaymentMethod` | 🔵 SUBSTITUÍDO |
| `AdminPriceTablesController` | Tabela preços | `PublicPricesView` (services) | 🔵 SUBSTITUÍDO |
| `AdminSettingsController` | Settings clínica | — | ❌ NÃO MIGRADO |
| `AdminReportsController` | Reports + Excel/PDF | Dashboard KPIs apenas | ❌ NÃO MIGRADO |

### 3.2 Public (13)

| Controller | Estado |
|------------|--------|
| `PublicServicesController` | ✅ MIGRADO (`PublicServicesView`, API public) |
| `PublicProductsController` | ✅ MIGRADO |
| `PublicProfessionalsController` | ✅ MIGRADO |
| `PublicProfessionalProfileController` | 🟡 PARCIAL (detalhe público sim; painel pro não) |
| `PublicBookingController` | ✅ MIGRADO / 🔵 melhorado (`BookingFlowView`) |
| `PublicAppointmentController` | ✅ MIGRADO (API availability + public appointments) |
| `PublicAccountController` | 🟡 PARCIAL (register sim; activation email não) |
| `PublicProfileController` | 🟡 PARCIAL (área client básica) |
| `PublicClientPanelController` | 🟡 PARCIAL (sem PDF export / Google link) |
| `PublicPriceController` | 🔵 SUBSTITUÍDO |
| `PublicInstitutionalController` | ✅ MIGRADO (About/Academy) |
| `PublicRecrutamentoController` | ✅ MIGRADO |
| `PublicFeedbackController` | ❌ NÃO MIGRADO |

### 3.3 Root / API (6)

| Controller | Estado |
|------------|--------|
| `AccountController` | 🟡 PARCIAL (login/logout/register; sem forgot/reset/social/settings UI) |
| `HomeController` | 🔵 SUBSTITUÍDO por Vue public |
| `ApiAppointmentsController` | ✅ MIGRADO (`/api/v1/appointments`) |
| `ClientsController` (API) | ✅ MIGRADO |
| `ProdutosController` (API) | ✅ MIGRADO |
| `PublicApiCalendarController` | ❌ NÃO MIGRADO |

---

## 4. Views Razor — funcionalidades vs Vue

| Capacidade nas Views | Vue atual | Estado |
|----------------------|-----------|--------|
| Formulários CRUD admin | Sim (modais/forms) | ✅ / 🟡 |
| Filtros / pesquisa | Sim em várias views | ✅ |
| Tabs Categorias Serviços/Produtos | `CategoriesView` | ✅ |
| Dashboard KPIs | `DashboardView` | 🟡 (sem charts avançados Excel/PDF) |
| PDV / barcode venda | `SalesView` simplificado | 🟡 |
| Relatórios export | — | ❌ |
| Calendário cliente avançado | — | ❌ |
| Upload imagens | UI parcial (Professionals/Services/Products); uploads ainda problemáticos em UX | 🟡 |
| Feedback/rating | — | ❌ |
| Settings clínica | — | ❌ |
| Forgot/Reset password | — | ❌ |
| Social login buttons | — | ❌ |
| Tawk chat widget | — | ❌ |

---

## 5. Autenticação e utilizadores

| Capacidade legado | Laravel/Vue | Estado |
|-------------------|-------------|--------|
| Login email/password | Sanctum `POST /auth/login` + `LoginView` | ✅ |
| Logout | `POST /auth/logout` | ✅ |
| Register cliente | `POST /auth/register` (sempre client) | ✅ |
| Roles Admin / Professional / Client | enum `UserRole` | ✅ (legado usava IdentityRole + strings) |
| Criação utilizador admin | `UserController` API | 🟡 (sem UI Vue dedicada) |
| Ativar/desativar user | `is_active` | 🟡 |
| Forgot password | tabela `password_reset_tokens` existe | ❌ sem endpoints/UI |
| Reset password | Identity no legado | ❌ |
| Change password (conta) | — | ❌ |
| Email confirmation / activation | `PublicAccount` + EmailService | ❌ |
| Profile self-service | Account Profile/Settings | ❌ |
| Social login Google/Facebook | AccountController ExternalLogin | ❌ |
| DiagnoseAdmin endpoint | — | ⚪ DESCARTÁVEL (risco) |

**Conclusão Auth:** login funciona ≠ Auth migrado. Auth está **parcial**.

---

## 6–13. Módulos de negócio (síntese)

### Clientes
- ✅ CRUD, notes, allergy_history, is_active, link user  
- 🟡 histórico appointments via lista; sem ProcedureHistory  
- ❌ imagem cliente, report financeiro cliente, documentos  

### Profissionais
- ✅ CRUD, specialty, photo, social, biography, experience, commission_percentage, schedules, services sync, public list/detail  
- 🟡 uploads (contrato existe; UX/pendências conhecidas)  
- ❌ commission por serviço, férias/ausências, painel próprio completo, DefaultCommission paralelo  

### Serviços
- ✅ CRUD, categories, price, duration, description, image, public, booking, professional association  
- ❌ Service.Commission, ReturnDays, commission no vínculo  

### Produtos
- ✅ CRUD, categories, price/cost, stock, min_stock, barcode/sku, image, sales link, low stock  
- ❌ Supplier FK, batch/expiry (legado StockEntry)  

### Agendamentos (profundidade)
- ✅ create admin + public, edit, cancel, status, professional, service, duration→end_time, price, notes, conflict detection, ProfessionalSchedule slots, client link, dashboard counts  
- 🟡 reminder_sent campo sem job; filtros calendário avançados limitados  
- ❌ Google Calendar sync/webhook, recorrência (também ausente no legado), export Excel/PDF flags  

### Stock
- ✅ stock atual, entry, exit, adjust, history, min stock, alerts low, user responsável  
- ❌ supplier on movement, batch/expiry  

### Vendas
- ✅ Sale + items product/service, client, payment_method enum, cancel com reposição stock, histórico  
- 🟡 professional na venda, descontos avançados, recibos PDF  
- ❌ geração automática de Receivable/Payable/comissão  

### Caixa
- ✅ open, close, income, expense, current, daily report  
- 🟡 histórico multi-dia / charts  
- ❌ SMS/WhatsApp no fecho (legado também stub)  

---

## 14. ERP / Financeiro

| Módulo legado | Controllers / Entities | Laravel/Vue | Estado |
|---------------|------------------------|-------------|--------|
| Suppliers | AdminSuppliers + Supplier | — | ❌ |
| Payables | AdminPayables + Payable | — | ❌ |
| Receivables | AdminReceivables + Receivable | — | ❌ |
| PaymentMethods | AdminPaymentMethod | Enum fixo | 🔵 |
| PriceTables | AdminPriceTables | Preços via Services | 🔵 |
| Settings | AdminSettings + Setting | — | ❌ |
| Billing legado | Billing/BillingDetails | Sales | ❓ |

---

## 15. Comissões

| Capacidade | Legado | Laravel | Estado |
|------------|--------|---------|--------|
| % global profissional | `CommissionPercentage`, `DefaultCommission` | `commission_percentage` | 🟡 |
| Comissão por serviço (pivot) | `ProfessionalService.Commission` | **ausente** na pivot | ❌ |
| Comissão no Service | `Service.Commission` | ausente | ❌ |
| Cálculo automático na venda/agendamento | **não encontrado** (campos + relatório manual via Payables) | — | ❌ |
| Pagamento comissão | Payables type `Commission` | — | ❌ |
| Relatório comissões | AdminReports Commissions | — | ❌ |
| Settings CommissionType / defaults | Setting | — | ❌ |

**Conclusão:** o legado tinha **modelo de dados** de comissão, mas o **cálculo automático estava incompleto**. Laravel migrou só a % global.

---

## 16. Relatórios

| Relatório / export | Legado | Laravel/Vue | Estado |
|--------------------|--------|-------------|--------|
| Dashboard admin | AdminController | DashboardView | 🟡 |
| Cash flow chart | AdminReports | Cash report diário | 🟡 |
| Sales report | AdminReports | Sales list | 🟡 |
| Commissions report | AdminReports | — | ❌ |
| Financial summary | AdminReports | — | ❌ |
| Monthly trial balance / Net profit views | Views existem | — | ❌ |
| Export Excel (EPPlus/ClosedXML) | ExcelExportService | — | ❌ |
| Export PDF (iText / stub HTML) | PdfExportService | — | ❌ |
| Client panel PDF history | PublicClientPanel | — | ❌ |

---

## 17. Uploads e ficheiros

**Legado suportava:**
- Upload local `wwwroot/uploads/{products|professionals|services}/{guid}.ext`
- Modo Blob/Azure (`StorageHelper`, `BlobHelper`) — configuração inconsistente (`Storage:Mode`)
- ImageId + ImageFullPath em Product, Professional, Service, Customer
- Logos em Settings (campos; upload no Settings controller fraco)

**Laravel atual:**
- Storage para profissionais/serviços/produtos (endpoints multipart + `MediaUploadTest`)
- Sem módulo Settings logos
- Sem upload de documentos genéricos / cliente

**Estado:** 🟡 PARCIAL (e UX de upload ainda reportada como pendente operacionalmente)

---

## 18. Notificações e comunicação

| Item | Legado | Atual | Estado |
|------|--------|-------|--------|
| In-app notifications | Notification + SignalR/Tawk hooks | Notification REST | 🟡 |
| Contact form | parcial / Home Contact | ContactMessage + admin messages | 🔵 NOVO/melhor |
| Email SMTP activation/reset/booking | EmailService, AppointmentNotificationService | — | ❌ |
| Appointment reminders job | AppointmentReminderService (24h/2h) | só flag `reminder_sent` | ❌ |
| SMS | não real | — | ⚪ |
| WhatsApp send | stub / campo settings | — | ⚪ |
| Feedback ratings | PublicFeedback | — | ❌ |
| Tawk live chat | _TawkPartial | — | ❌ |

---

## 19. Integrações

| Integração | Existe no legado? | No novo? | Classificação |
|------------|-------------------|----------|---------------|
| Google Calendar sync/webhook | **SIM** (`GoogleCalendarSyncService`, PublicApiCalendar) | NÃO | EXISTE NO LEGADO → ❌ pendente |
| Google/Facebook social login | **SIM** | NÃO | EXISTE NO LEGADO → ❌ |
| SMTP email | **SIM** | NÃO (app) | EXISTE NO LEGADO → ❌ |
| Tawk | **SIM** (parcial/inconsistente) | NÃO | EXISTE NO LEGADO → ❓ |
| Google Maps embed | layout público | pode ser estático Vue | ❓ |
| Pagamentos externos (Stripe etc.) | **NÃO** encontrado | NÃO | só planeado se listado noutro doc |
| WebSockets | SignalR | REST (futuro WS nos docs) | 🔵 SUBSTITUÍDO |
| WhatsApp/SMS APIs | stubs | NÃO | legado incompleto |

---

## 20. Configurações

| Setting legado | Atual | Estado |
|----------------|-------|--------|
| ClinicName, Email, Phones, Address | — | ❌ |
| Logo / Icon / ReportLogo | — | ❌ |
| Instagram / WhatsApp | hardcode UI pública parcial | 🟡 |
| CommissionType / default commissions | — | ❌ |
| BusinessHours / DefaultServiceDuration | schedules + service duration | 🔵 parcial via outros módulos |
| ImagesFolder | storage Laravel | 🔵 |

---

## 21. Rotas / endpoints

- Legado: MVC areas + API parcial (appointments, clients, products, Google calendar).  
- Laravel: **92 routes** (`php artisan route:list`, 2026-08-11), API REST `/api/v1/*` cobrindo módulos migrados.  
- Não há equivalência 1:1 (esperado). Gaps: suppliers, payables, receivables, settings, reports export, password reset, Google calendar, feedback, users UI.

---

## 22. Banco de dados — tabela comparativa

| Legado | Laravel | Estado | Observação |
|--------|---------|--------|------------|
| Users / AspNet* | `users` + Sanctum tokens | 🟡 | password_reset_tokens criado, não usado na API |
| Customers | `clients` | 🟡 | sem ImageId |
| Professionals | `professionals` | 🟡 | commission global; sem DefaultCommission separado |
| ProfessionalSchedules | `professional_schedules` | ✅ | nova implementação robusta |
| ProfessionalServices | `professional_service` | 🟡 | **comissão por serviço ausente** |
| Service / Category | `services` / `service_categories` | ✅/🟡 | categorias melhoradas |
| Products / ProductCategories | `products` / `product_categories` | ✅/🟡 | sem suppliers |
| Suppliers | — | ❌ | |
| Appointments | `appointments` | 🟡 | sem GoogleEventId |
| Sales / SaleItems | `sales` / `sale_items` | 🔵+ | sale_items com service_id |
| StockMovements (+Entry/Exit) | `stock_movements` | 🔵 | unificado |
| CashRegisters / CashMovements | `cash_registers` / `cash_transactions` | ✅ | |
| Payables / Receivables | — | ❌ | |
| PaymentMethods | enum | 🔵 | |
| PriceTables | — | 🔵 | via services |
| Settings | — | ❌ | |
| Notifications | `notifications` | 🟡 | |
| Reminder | — | ❌ | |
| Feedback / ServiceReviews | — | ❌ | |
| Plans / PlanAppointments | — | ❌ | |
| ProcedureHistories | — | ❌ | |
| Billings / BillingDetails | — | ❓ | |
| BeautyCenters | — | ❓ | |
| AccessLog | — | ❓ | |
| Profiles / Schedule duplicado | — | ⚪ | |
| Contact (novo) | `contact_messages` | 🔵 NOVO | |

---

## 23. Regras do legado ainda não reproduzidas

| # | Regra | Onde no C# | Impacto | Equivalente atual | Prioridade |
|---|-------|------------|---------|-------------------|------------|
| 1 | Gestão de fornecedores e vínculo a produtos/stock | AdminSuppliers, Supplier, Product.SupplierId | Sem rastreio de abastecimento | — | Alta |
| 2 | Contas a pagar (despesas, comissões, fornecedor) | AdminPayables, Payable | Sem financeiro operacional | — | Alta |
| 3 | Contas a receber | AdminReceivables, Receivable | Sem aging/cliente financeiro | — | Alta |
| 4 | Comissão por serviço no pivot | ProfessionalService.Commission | Comissão operacional incompleta | só % global | Alta |
| 5 | Relatórios + Excel/PDF | AdminReports, Excel/PdfExportService | Sem exports gerenciais | Dashboard KPIs | Alta |
| 6 | Settings da clínica | AdminSettings, Setting | Branding/contactos não administráveis | hardcodes/UI | Média |
| 7 | Password reset / forgot | AccountController + EmailService | UX auth incompleta | tabela vazia | Média |
| 8 | Email confirmation / activation | PublicAccountController | Contas sem verificação | register direto | Média |
| 9 | Lembretes automáticos 24h/2h | AppointmentReminderService | No-shows | reminder_sent flag | Média |
| 10 | Google Calendar sync | GoogleCalendarSyncService, PublicApiCalendar | Sem sync agenda | — | Média |
| 11 | Feedback/reviews pós-serviço | PublicFeedback, ServiceReview | Sem satisfação | ContactMessage | Média |
| 12 | Planos de sessões | Plan, PlanAppointment | Pacotes não suportados | — | Média |
| 13 | ProcedureHistory técnico | ProcedureHistory | Prontuário técnico ausente | notes cliente | Baixa |
| 14 | AccessLog auditoria | AccessLog | Sem trilha de ações | — | Baixa |
| 15 | Social login | AccountController ExternalLogin | — | — | Baixa |
| 16 | PaymentMethod CRUD dinâmico | AdminPaymentMethod | Métodos fixos no enum | Enum | Baixa |
| 17 | PriceTable paralela a serviços | AdminPriceTables | redundante se services forem fonte | PublicPrices | Baixa |
| 18 | PDV barcode-centric FinalizeSale sem Sale entity | AdminCashRegister FinalizeSale | Legado inconsistente | SaleService real | 🔵 já melhorado |
| 19 | Emails booking confirm/cancel/reschedule | Views/Emails + services | Sem emails transacionais | — | Média |

---

## 24. Funcionalidades novas / substituições melhoradas no Laravel/Vue

| Item | Tipo | Nota |
|------|------|------|
| API REST versionada `/api/v1` + Sanctum | NOVO | Auth obrigatória (legado API fraca) |
| Form Requests + Policies + Services | SUBSTITUIÇÃO MELHORADA | vs Controllers gordos |
| `end_time` calculado + conflito rigoroso | SUBSTITUIÇÃO MELHORADA | |
| Availability baseada em ProfessionalSchedule | SUBSTITUIÇÃO MELHORADA | legado público usava defaults 9–18 em partes |
| Categorias com slug/is_active/sort_order | SUBSTITUIÇÃO MELHORADA | |
| SaleItem com `service_id` | NOVO | legado SaleItem só Product |
| StockMovement unificado | SUBSTITUIÇÃO MELHORADA | vs Entry/Exit + Movement |
| ContactMessage admin | NOVO | |
| Vue SPA + role routing | NOVO | |
| SQLite + testes Feature extensivos | NOVO | |
| Public booking flow multi-step | SUBSTITUIÇÃO MELHORADA | |

---

## 25. Matriz final por módulo

| Módulo | Legado | Laravel/Vue | Estado | Pode remover legado deste módulo? |
|--------|--------|-------------|--------|-----------------------------------|
| Auth básica | Account login/logout/register | Sanctum + Vue | ✅ | Sim *parcial* (manter ref reset/social) |
| Auth avançada | reset, activation, social, profile | — | ❌ | **Não** |
| Users admin | Identity helpers | User API | 🟡 | Não |
| Dashboard | AdminController | DashboardView | 🟡 | Não (reports) |
| Clientes | vários AdminClient* | ClientsView + client panel | 🟡 | Não |
| Profissionais | AdminProfessionals | ProfessionalsView | 🟡 | Não |
| Horários | AdminProfessionalSchedules | ProfessionalSchedulesView | ✅ | Sim |
| Serviços profissionais | AdminServiceProfessionals | sync services | 🟡 | Não (commission) |
| Serviços | AdminServices | ServicesView | 🟡 | Quase |
| Categorias serviço/produto | Admin*Categories | CategoriesView | ✅ | Sim |
| Produtos | AdminProducts | ProductsView | 🟡 | Não (supplier) |
| Stock | AdminStock | StockView | ✅ | Sim |
| Agendamentos | Admin + Public + API | Appointments + Booking | 🟡 | Não (Google/reminders) |
| Site público institucional | Public* | Vue public | ✅ | Sim |
| Vendas | Sale + PDV | SalesView | 🟡 | Não |
| Caixa | CashRegister/Movements | CashView | ✅/🟡 | Quase |
| Fornecedores | AdminSuppliers | — | ❌ | **Não** |
| Payables | AdminPayables | — | ❌ | **Não** |
| Receivables | AdminReceivables | — | ❌ | **Não** |
| Payment methods | CRUD | Enum | 🔵 | Sim se descarte aceite |
| Price tables | CRUD | Services prices | 🔵 | Sim se descarte aceite |
| Settings | AdminSettings | — | ❌ | **Não** |
| Relatórios/export | AdminReports | KPIs | ❌ | **Não** |
| Comissões | campos + Payables | % global | 🟡 | **Não** |
| Notificações | SignalR/REST mix | REST | 🟡 | Quase |
| Contacto | parcial | ContactMessages | 🔵 | Sim |
| Feedback | PublicFeedback | — | ❌ | **Não** |
| Planos | Plan* | — | ❌ | **Não** |
| Google Calendar | Sync service | — | ❌ | **Não** |
| Uploads | StorageHelper | Storage Laravel | 🟡 | Não |
| AccessLog / Billing / BeautyCenter / Schedule dup | vários | — | ❓/⚪ | Aguarda decisão |

Legenda: ✅ MIGRADO · 🟡 PARCIAL · 🔵 SUBSTITUÍDO · ⚪ DESCARTADO · ❌ NÃO MIGRADO · ❓ DECISÃO NECESSÁRIA

---

## 26. Percentagem estimada (por funcionalidade, não por ficheiros)

Base: ~40 capacidades funcionais relevantes inventariadas no legado.

| Classe | Estimativa |
|--------|------------|
| ✅ Migrado | **~45%** |
| 🟡 Parcial | **~22%** |
| 🔵 Substituído / ⚪ descartável (já coberto) | **~8%** |
| ❌ Não migrado | **~25%** |

**Migrado + Substituído utilizável ≈ 53%**  
**Ainda depende do legado como referência ≈ 47%** (parcial + não migrado + decisões)

---

## 27. Bloqueadores para remoção do legado

1. **Suppliers** — Prioridade **Alta** — Ação: Migrar **ou** Descartar oficialmente  
2. **Payables** — Prioridade **Alta** — Migrar / Descartar  
3. **Receivables** — Prioridade **Alta** — Migrar / Descartar  
4. **Comissão por serviço + relatório/pagamento** — Prioridade **Alta** — Migrar  
5. **Relatórios Excel/PDF** — Prioridade **Alta** — Migrar (subset) / Substituir  
6. **Settings clínicas** — Prioridade **Média** — Migrar  
7. **Password reset + change password** — Prioridade **Média** — Migrar  
8. **Email transacional + activation** — Prioridade **Média** — Migrar / Descartar  
9. **Appointment reminders** — Prioridade **Média** — Migrar  
10. **Google Calendar** — Prioridade **Média** — Migrar / Descartar oficialmente  
11. **Feedback/Reviews** — Prioridade **Média** — Migrar / Descartar  
12. **Planos de sessões** — Prioridade **Média** — Migrar / Descartar  
13. **ProcedureHistory** — Prioridade **Baixa** — Migrar / Descartar  
14. **Social login / Tawk / AccessLog / BeautyCenter / Billing** — Prioridade **Baixa** — Decisão  

---

## 28. Candidatos a descarte (AGUARDA DECISÃO)

| Candidato | O que fazia | Porquê parece obsoleto | O que já existe | Impacto se descartar |
|-----------|-------------|------------------------|-----------------|----------------------|
| `PriceTable` | Lista de preços paralela | Duplica `Service.price` | PublicPrices / Services | Baixo se prices = services |
| `PaymentMethod` entity | CRUD métodos | Enum cobre cash/card/transfer/other | Enum Laravel | Médio se precisarem métodos dinâmicos |
| `Schedule` (`Shedule.cs`) | Horário paralelo | Duplicado de ProfessionalSchedule | ProfessionalSchedule | Baixo |
| `Profile` entity | Perfis paralelos a roles | Roles enum | UserRole | Baixo |
| `Billing` / `BillingDetails` | Faturação antiga | Sale cobre PDV moderno | Sale/SaleItem | Médio — confirmar se havia dados reais |
| `BeautyCenter` | Multi-unidade | App single-salon | — | Baixo se single tenant |
| `AccessLog` | Auditoria UI | Sem consumidor no novo | logs Laravel | Baixo/médio compliance |
| Social login | OAuth Google/Facebook | Pode não ser necessário | Login email | Baixo |
| Tawk widget | Chat | Pode usar link WhatsApp | Contact form | Baixo |
| `DiagnoseAdmin` | Debug endpoint | Risco segurança | — | Descartar recomendado |
| PDV FinalizeSale sem Sale | Venda só caixa | Já corrigido no Laravel | SaleService | Descartar lógica legada |

**Estado de todos:** **AGUARDA DECISÃO** — não descartados oficialmente nesta auditoria.

---

## 29. Auditoria de dependências

### Runtime (Laravel/Vue)
- **Nenhuma** importação de `legacy-csharp`, `.cs`, Razor ou EF no `backend/` ou `frontend/` de aplicação.
- `composer.json` / `package.json`: sem dependência do legado.

### Build / tooling
| Local | Tipo | Nota |
|-------|------|------|
| `.vscode/tasks.json` (raiz) | build tooling | aponta para `${workspaceFolder}/Sistema.csproj` (**path quebrado**/legado) |
| `legacy-csharp/.vscode/tasks.json` | build legado | local ao legado |
| `postman/*.json` | documentação | descrição ainda menciona ASP.NET |

### Documentação
| Ficheiro | Tipo |
|----------|------|
| `README.md` | referência explícita a `legacy-csharp/` |
| `docs/arquitetura.md` | referência |
| `docs/migracao-csharp-para-php.md` | guia migração |
| `legacy-csharp/README.md` | self-doc |

**Conclusão:** zero dependência de **runtime**. Existem referências **documentais** e **tasks VS Code** obsoletas. A app Laravel/Vue corre sem a pasta C#, mas a **migração** ainda depende dela como especificação.

---

## 30. Veredicto final

### PODE APAGAR `legacy-csharp` AGORA?

# **NÃO**

### Motivos (bloqueadores)
1. Módulos ERP não migrados (Suppliers, Payables, Receivables)  
2. Comissões operacionais incompletas  
3. Relatórios/exportações ausentes  
4. Auth avançada (reset, activation, profile) ausente  
5. Settings, Feedback, Planos, Google Calendar, reminders ausentes  
6. Decisões de descarte ainda não formalizadas  
7. Pasta ainda citada como referência oficial de migração  

### Quando poderia ser SIM
- Bloqueadores Alta migrados **ou** descartados por escrito  
- Bloqueadores Média tratados ou descartados  
- Tasks/docs atualizados  
- Testes e build verdes (já estão)  
- Checklist de zero dependência runtime/build confirmada  

**Mesmo com SIM futuro: não apagar automaticamente sem aprovação explícita.**

---

## 31. Validação do sistema novo (sem alterar código)

Executado em **2026-08-11**:

### Backend
```text
cd backend
php artisan test
→ Tests: 77 passed (360 assertions)

php artisan route:list
→ 92 routes
```

### Frontend
```text
cd frontend
npm run build
→ ✓ built (exit 0)
```

---

## 32. Histórico desta auditoria

| Campo | Valor |
|-------|-------|
| Pasta analisada | `legacy-csharp/` |
| Controllers | 43 |
| Entidades | 36 |
| Views Razor | 176 |
| Models Laravel | 16 |
| Resultado apagar | **NÃO** |
| Documento | `docs/LEGACY_MIGRATION_AUDIT.md` |

---

*Documento gerado para servir de base a decisões de migração, descarte e descomissionamento. Não substitui aprovação de produto/negócio.*
