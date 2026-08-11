# Auditoria do Banco de Dados Atual — Salon Management System

**Data:** 2026-08-11  
**Tipo:** diagnóstico somente leitura  
**Fonte:** SQLite real consultado via `sqlite3` + `php artisan db:show` / `migrate:status`  
**Âmbito:** `backend/.env`, `backend/config/database.php`, schema real, Models/Requests/Resources de Product, seeders, comparação com `legacy-csharp/` (leitura) e `docs/LEGACY_MIGRATION_AUDIT.md`  

**Nesta execução NÃO foi:** criado código, migration, seeder, dado fictício, alteração de tabelas, frontend, backend ou `legacy-csharp/`.

---

## 1. Banco utilizado

| Item | Valor real |
|------|------------|
| `DB_CONNECTION` | `sqlite` |
| `DB_DATABASE` (`.env`) | `database/database.sqlite` |
| Default em `config/database.php` | `env('DB_CONNECTION', 'sqlite')` |
| Resolução SQLite | relativo a `backend/` → `backend/database/database.sqlite` |
| Foreign keys | `DB_FOREIGN_KEYS` default `true` (config) |
| Migrations executadas | **26** (todas Ran) |
| Tabelas totais | **26** (inclui infra Laravel) |

### Caminho físico do ficheiro SQLite

```
C:\Projetos\Pessoal\salon-management-system\backend\database\database.sqlite
```

(Unix/Git Bash: `/c/Projetos/Pessoal/salon-management-system/backend/database/database.sqlite`)

**Nota:** não há password DB (SQLite local). Secrets de `.env` (APP_KEY, mail, AWS, etc.) **não** são reproduzidos neste relatório.

---

## 2. Inventário de tabelas reais

### 2.1 Tabelas funcionais (domínio)

| Tabela | Existe | Registos |
|--------|--------|---------:|
| `users` | SIM | 4 |
| `clients` | SIM | 3 |
| `professionals` | SIM | 2 |
| `professional_schedules` | SIM | 4 |
| `professional_service` | SIM | 19 |
| `services` | SIM | 63 |
| `service_categories` | SIM | 6 |
| `products` | SIM | **0** |
| `product_categories` | SIM | 1 |
| `appointments` | SIM | 2 |
| `sales` | SIM | 0 |
| `sale_items` | SIM | 0 |
| `stock_movements` | SIM | 0 |
| `cash_registers` | SIM | 0 |
| `cash_transactions` | SIM | 0 |
| `notifications` | SIM | 3 |
| `contact_messages` | SIM | 0 |

### 2.2 Tabelas de infraestrutura Laravel

| Tabela | Registos |
|--------|---------:|
| `migrations` | 26 |
| `sessions` | 15 |
| `personal_access_tokens` | 7 |
| `cache` | 0 |
| `cache_locks` | 0 |
| `jobs` | 0 |
| `job_batches` | 0 |
| `failed_jobs` | 0 |
| `password_reset_tokens` | 0 |

### 2.3 Tabelas ausentes (procuradas e não encontradas)

| Nome pesquisado | Existe? |
|-----------------|---------|
| `suppliers` / `supplier` / `providers` / `vendors` / `fornecedores` | **NÃO** |
| `payables` / `accounts_payable` / `expenses` / `bills` / `financial_payables` | **NÃO** |
| `receivables` / `accounts_receivable` | **NÃO** |

---

## 3. Foreign keys reais

Consultado com `PRAGMA foreign_key_list`:

| De | Para | ON DELETE |
|----|------|-----------|
| `clients.user_id` | `users.id` | SET NULL |
| `professionals.user_id` | `users.id` | CASCADE |
| `professional_schedules.professional_id` | `professionals.id` | CASCADE |
| `professional_schedules.created_by` | `users.id` | SET NULL |
| `professional_service.professional_id` | `professionals.id` | CASCADE |
| `professional_service.service_id` | `services.id` | CASCADE |
| `services.service_category_id` | `service_categories.id` | SET NULL |
| `products.product_category_id` | `product_categories.id` | SET NULL |
| `appointments.client_id` | `clients.id` | CASCADE |
| `appointments.service_id` | `services.id` | RESTRICT |
| `appointments.professional_id` | `professionals.id` | RESTRICT |
| `notifications.user_id` | `users.id` | CASCADE |
| `stock_movements.product_id` | `products.id` | CASCADE |
| `stock_movements.user_id` | `users.id` | SET NULL |
| `sales.client_id` | `clients.id` | SET NULL |
| `sales.user_id` | `users.id` | CASCADE |
| `sale_items.sale_id` | `sales.id` | CASCADE |
| `sale_items.product_id` | `products.id` | SET NULL |
| `sale_items.service_id` | `services.id` | SET NULL |
| `cash_registers.opened_by` | `users.id` | CASCADE |
| `cash_registers.closed_by` | `users.id` | SET NULL |
| `cash_transactions.cash_register_id` | `cash_registers.id` | CASCADE |
| `cash_transactions.user_id` | `users.id` | SET NULL |

**Índices UNIQUE relevantes**

| Tabela | Índice | Coluna |
|--------|--------|--------|
| `products` | `products_sku_unique` | `sku` |
| `products` | `products_barcode_unique` | `barcode` |
| `product_categories` | `product_categories_slug_unique` | `slug` |
| `service_categories` | (slug unique) | `slug` |
| `users` | `users_email_unique` | `email` |

---

## 4. Colunas por tabela funcional

Legenda: **NN** = NOT NULL; **PK** = primary key; **FK** = foreign key.

### `users`

| Coluna | Tipo | NN | Default | PK/FK |
|--------|------|----|---------|-------|
| id | INTEGER | SIM | — | PK |
| first_name | varchar | SIM | — | |
| last_name | varchar | SIM | — | |
| email | varchar | SIM | — | UNIQUE |
| phone | varchar | NÃO | — | |
| role | varchar | SIM | `'client'` | |
| is_active | tinyint(1) | SIM | `1` | |
| email_verified_at | datetime | NÃO | — | |
| password | varchar | SIM | — | |
| remember_token | varchar | NÃO | — | |
| created_at / updated_at | datetime | NÃO | — | |

### `clients`

| Coluna | Tipo | NN | Default | PK/FK |
|--------|------|----|---------|-------|
| id | INTEGER | SIM | — | PK |
| user_id | INTEGER | NÃO | — | FK → users |
| name | varchar | SIM | — | |
| email / phone / address | varchar | NÃO | — | |
| birth_date | date | NÃO | — | |
| notes | TEXT | NÃO | — | |
| allergy_history | varchar | NÃO | — | |
| is_active | tinyint(1) | SIM | `1` | |
| timestamps | datetime | NÃO | — | |

### `professionals`

| Coluna | Tipo | NN | Default | PK/FK |
|--------|------|----|---------|-------|
| id | INTEGER | SIM | — | PK |
| user_id | INTEGER | NÃO | — | FK → users |
| name | varchar | SIM | — | |
| specialty | varchar | SIM | — | |
| phone / email | varchar | NÃO | — | |
| commission_percentage | numeric | SIM | `0` | |
| is_active | tinyint(1) | SIM | `1` | |
| photo | varchar | NÃO | — | |
| biography | TEXT | NÃO | — | |
| instagram / facebook | varchar | NÃO | — | |
| years_experience | INTEGER | NÃO | — | |
| timestamps | datetime | NÃO | — | |

### `professional_schedules`

| Coluna | Tipo | NN | Default | PK/FK |
|--------|------|----|---------|-------|
| id | INTEGER | SIM | — | PK |
| professional_id | INTEGER | SIM | — | FK → professionals |
| day_of_week | INTEGER | SIM | — | |
| start_time / end_time | time | SIM | — | |
| created_by | INTEGER | NÃO | — | FK → users |
| timestamps | datetime | NÃO | — | |

### `professional_service`

| Coluna | Tipo | NN | Default | PK/FK |
|--------|------|----|---------|-------|
| id | INTEGER | SIM | — | PK |
| professional_id | INTEGER | SIM | — | FK → professionals |
| service_id | INTEGER | SIM | — | FK → services |
| timestamps | datetime | NÃO | — | |

### `services`

| Coluna | Tipo | NN | Default | PK/FK |
|--------|------|----|---------|-------|
| id | INTEGER | SIM | — | PK |
| name | varchar | SIM | — | |
| description | TEXT | NÃO | — | |
| price | numeric | SIM | — | |
| duration_minutes | INTEGER | SIM | `60` | |
| is_active | tinyint(1) | SIM | `1` | |
| category | varchar | NÃO | — | legado textual |
| service_category_id | INTEGER | NÃO | — | FK → service_categories |
| image | varchar | NÃO | — | |
| timestamps | datetime | NÃO | — | |

### `service_categories`

| Coluna | Tipo | NN | Default | PK/FK |
|--------|------|----|---------|-------|
| id | INTEGER | SIM | — | PK |
| name | varchar | SIM | — | |
| slug | varchar | SIM | — | UNIQUE |
| description | TEXT | NÃO | — | |
| is_active | tinyint(1) | SIM | `1` | |
| sort_order | INTEGER | SIM | `0` | |
| timestamps | datetime | NÃO | — | |

### `products` ⭐

| Coluna | Tipo | NN | Default | PK/FK / UNIQUE |
|--------|------|----|---------|-----------------|
| id | INTEGER | SIM | — | PK |
| name | varchar | SIM | — | |
| description | **varchar** | NÃO | — | ⚠️ não é TEXT |
| sku | varchar | NÃO | — | **UNIQUE** |
| category | varchar | NÃO | — | legado textual |
| price | numeric | SIM | — | |
| cost_price | numeric | NÃO | — | |
| stock_quantity | INTEGER | SIM | `0` | |
| min_stock | INTEGER | SIM | `0` | |
| image | varchar | NÃO | — | path string |
| is_active | tinyint(1) | SIM | `1` | |
| created_at / updated_at | datetime | NÃO | — | |
| barcode | varchar | NÃO | — | **UNIQUE** |
| product_category_id | INTEGER | NÃO | — | FK → product_categories ON DELETE SET NULL |

**Confirmado ausente em `products`:** `supplier_id`, `brand`, `usage_instructions`, `ingredients`.

### `product_categories`

| Coluna | Tipo | NN | Default | PK/FK |
|--------|------|----|---------|-------|
| id | INTEGER | SIM | — | PK |
| name | varchar | SIM | — | |
| slug | varchar | SIM | — | UNIQUE |
| description | TEXT | NÃO | — | |
| is_active | tinyint(1) | SIM | `1` | |
| sort_order | INTEGER | SIM | `0` | |
| timestamps | datetime | NÃO | — | |

### `appointments`

| Coluna | Tipo | NN | Default | PK/FK |
|--------|------|----|---------|-------|
| id | INTEGER | SIM | — | PK |
| client_id | INTEGER | SIM | — | FK → clients CASCADE |
| service_id | INTEGER | SIM | — | FK → services RESTRICT |
| professional_id | INTEGER | SIM | — | FK → professionals RESTRICT |
| start_time / end_time | datetime | SIM | — | |
| status | varchar | SIM | `'pending'` | |
| notes | TEXT | NÃO | — | |
| total_price | numeric | NÃO | — | |
| is_active | tinyint(1) | SIM | `1` | |
| reminder_sent | tinyint(1) | SIM | `0` | |
| timestamps | datetime | NÃO | — | |

### `sales`

| Coluna | Tipo | NN | Default | PK/FK |
|--------|------|----|---------|-------|
| id | INTEGER | SIM | — | PK |
| client_id | INTEGER | NÃO | — | FK → clients |
| user_id | INTEGER | SIM | — | FK → users |
| total_amount | numeric | SIM | — | |
| payment_method | varchar | SIM | — | |
| status | varchar | SIM | `'paid'` | |
| notes | TEXT | NÃO | — | |
| timestamps | datetime | NÃO | — | |

### `sale_items`

| Coluna | Tipo | NN | Default | PK/FK |
|--------|------|----|---------|-------|
| id | INTEGER | SIM | — | PK |
| sale_id | INTEGER | SIM | — | FK → sales |
| product_id | INTEGER | NÃO | — | FK → products |
| service_id | INTEGER | NÃO | — | FK → services |
| description | varchar | SIM | — | |
| quantity | INTEGER | SIM | `1` | |
| unit_price / total_price | numeric | SIM | — | |
| timestamps | datetime | NÃO | — | |

### `stock_movements`

| Coluna | Tipo | NN | Default | PK/FK |
|--------|------|----|---------|-------|
| id | INTEGER | SIM | — | PK |
| product_id | INTEGER | SIM | — | FK → products |
| user_id | INTEGER | NÃO | — | FK → users |
| type | varchar | SIM | — | |
| quantity | INTEGER | SIM | — | |
| previous_quantity / new_quantity | INTEGER | SIM | — | |
| reason | varchar | NÃO | — | |
| notes | TEXT | NÃO | — | |
| timestamps | datetime | NÃO | — | |

### `cash_registers` / `cash_transactions` / `notifications` / `contact_messages`

Conforme `PRAGMA table_info` acima (secção 4 dump). Sem surpresas face às migrations já aplicadas.

---

## 5. Estado de Products

### 5.1 Banco

- Tabela `products`: **existe**
- Registos: **0**
- Colunas reais: `id`, `name`, `description`, `sku`, `category`, `price`, `cost_price`, `stock_quantity`, `min_stock`, `image`, `is_active`, `created_at`, `updated_at`, `barcode`, `product_category_id`
- `supplier_id`: **NÃO existe**

### 5.2 Comparativo CAMPO × BANCO × MODEL × REQUEST × RESOURCE

| CAMPO | BANCO | MODEL `$fillable` | Store/Update Request | ProductResource |
|-------|:-----:|:-----------------:|:--------------------:|:---------------:|
| id | ✅ | — | — | ✅ |
| name | ✅ | ✅ | ✅ required (store) | ✅ |
| description | ✅ varchar | ✅ | ✅ nullable **max:255** | ✅ |
| sku | ✅ UNIQUE nullable | ✅ | ✅ nullable unique | ✅ |
| barcode | ✅ UNIQUE nullable | ✅ | ✅ nullable unique | ✅ |
| category | ✅ | ✅ | ✅ nullable | ✅ (via label) |
| product_category_id | ✅ FK | ✅ | ✅ nullable exists | ✅ |
| price | ✅ NN | ✅ | ✅ required_without sale_price | ✅ (+ alias sale_price) |
| cost_price | ✅ nullable | ✅ | ✅ nullable (+ alias purchase_price) | ✅ (+ purchase_price) |
| stock_quantity | ✅ default 0 | ✅ | ✅ sometimes (+ alias current_stock) | ✅ (+ current_stock) |
| min_stock | ✅ default 0 | ✅ | ✅ sometimes (+ alias minimum_stock) | ✅ (+ minimum_stock) |
| image | ✅ nullable | ✅ | ✅ **upload ficheiro** (image/jpeg\|png\|webp, max 2MB) | ✅ (+ image_url) |
| is_active | ✅ default 1 | ✅ | ✅ sometimes boolean | ✅ |
| supplier_id | ❌ | ❌ | ❌ | ❌ |
| brand | ❌ | ❌ | ❌ | ❌ |
| usage_instructions | ❌ | ❌ | ❌ | ❌ |
| ingredients | ❌ | ❌ | ❌ | ❌ |

### 5.3 Model Product (resumo)

- `$fillable`: name, description, sku, barcode, category, product_category_id, price, cost_price, stock_quantity, min_stock, image, is_active  
- Casts: price/cost_price decimal:2; stock_quantity/min_stock int; is_active bool  
- Relations: `productCategory()` BelongsTo; `stockMovements()` HasMany  
- `imageUrl()`: aceita path relativo storage **ou** path público legado `/images/...`

### 5.4 Imagem — path vs upload

| Via | Comportamento |
|-----|---------------|
| API `POST/PUT` produto | espera **ficheiro** multipart (`image`); grava em disk `public` sob `products/{uuid}.ext` |
| Coluna `image` | string nullable; pode guardar path relativo storage **ou** path absoluto público `/images/...` |
| Frontend `mediaUrl()` | resolve `/images/...` localmente; storage via `VITE_API_URL` + `/storage/...` |

---

## 6. Estado de Product Categories

| Item | Valor |
|------|-------|
| Tabela existe | **SIM** |
| Quantidade | **1** |
| ID | `1` |
| name | `Cabelo Produtos` |
| slug | `cabelo-produtos` |
| description | `NULL` / vazio |
| is_active | `1` |
| sort_order | `0` |

Não há categorias “Unhas”, “Produtos Wiñk”, “Acessórios”, etc. no banco atual.

---

## 7. Suppliers

**SUPPLIERS: TABELA NÃO EXISTE NO LARAVEL ATUAL.**

- Sem Model/Migration/Controller Laravel para fornecedores.
- `products.supplier_id`: **não existe**.
- Legado C#: entidade `Supplier` (`Suppliers`) com Name, Phone, Email, TaxId, Address, DeliveryTime, Notes, Products 1:N.

---

## 8. Payables

**PAYABLES: TABELA NÃO EXISTE.**

- Sem equivalentes `accounts_payable`, `expenses`, `bills`, `financial_payables`.
- Legado C#: `Payable` (`Payables`) — Amount, DueDate, Status, Type, SupplierId, ProfessionalId, SaleId, ProductId, etc.

---

## 9. Receivables

**RECEIVABLES: TABELA NÃO EXISTE.**

- Sem equivalentes encontrados.
- Legado C#: `Receivable` (`Receivables`) — Customer, Professional, Service, Sale, Product, PaymentMethod, etc.

---

## 10. Comparação com legacy-csharp

| LEGADO | LARAVEL ATUAL | ESTADO |
|--------|---------------|--------|
| `Supplier` / `Suppliers` | — | **ausente** |
| `Payable` / `Payables` | — | **ausente** |
| `Receivable` / `Receivables` | — | **ausente** |
| `Product` (+ SupplierId) | `products` (sem supplier_id) | **parcial** |
| `ProductCategory` | `product_categories` | **existe** |
| `StockMovement` (+ Supplier opcional no legado) | `stock_movements` (sem supplier) | **parcial / unificado** |

Alinhado com `docs/LEGACY_MIGRATION_AUDIT.md` (prioridade Alta para Suppliers / Payables / Receivables).

---

## 11. Seeders existentes e natureza dos dados

### Seeders no código

| Seeder | Chamado por | Natureza |
|--------|-------------|----------|
| `DatabaseSeeder` | `db:seed` | orquestra |
| `SalonSeeder` | DatabaseSeeder | **demo**: users admin/ana/maria, professional Ana, client Maria, **3 produtos demo** (Shampoo/Máscara/Esmalte), categorias Cabelo/Unhas, 1 appointment, notifications |
| `BookingServicesSeeder` | SalonSeeder | **estrutural/catálogo real de serviços** a partir de `database/data/booking_services.php` |
| `ProfessionalScheduleSeeder` | (não chamado no DatabaseSeeder) | demo de horários para Ana Costa |

### O que o banco **real** mostra agora

| Dado | Classificação | Observação |
|------|---------------|------------|
| users `admin@salon.test`, `ana.costa@salon.test`, `maria.silva@salon.test` | demo | emails `*.salon.test` |
| user `reboucasericka@gmail.com` | **real / operacional** | não vem do seeder padrão |
| clients Maria + 2× Ericka Rebouças | misto demo + real | possível duplicado Ericka |
| professional Ana Costa | demo | |
| professional Juliana Neves | **não-demo** (email `@sistema.com`) | |
| 63 services + 6 service_categories | catálogo estrutural (BookingServices) + 3 serviços antigos (Corte/Coloração/Manicure sem category FK) | |
| product_categories `Cabelo Produtos` | residual | **não** coincide com nomes do SalonSeeder (`Cabelo`/`Unhas`) |
| products | **vazio** | os 3 produtos demo do SalonSeeder **não estão** presentes |
| appointments (2) | demo/teste | |
| sales / stock / cash / contact | vazios | |

**Conclusão:** o banco já mistura demo + dados reais. Produtos demo do seeder foram removidos ou nunca aplicados nesta instância; categorias de produto também diferem do seeder.

---

## 12. Preparação para inserir PRODUTOS REAIS

### A tabela está pronta?

**SIM**, para catálogo operacional básico (nome, preço, stock, categoria, imagem, sku/barcode).

**NÃO**, se o requisito incluir fornecedor, brand, modo de aplicação/ingredientes como campos separados, ou descrições longas via API (>255 chars).

### Campos que podes fornecer (existentes)

| Campo | Obrigatório? | Notas |
|-------|--------------|-------|
| `name` | **SIM** | max 150 (request) |
| `price` (ou `sale_price`) | **SIM** | ≥ 0 |
| `description` | NÃO | **max 255** na API; coluna varchar |
| `sku` | NÃO | se preenchido, **UNIQUE** |
| `barcode` | NÃO | se preenchido, **UNIQUE** |
| `category` | NÃO | texto legado; API preenche a partir do nome da categoria se `product_category_id` for enviado |
| `product_category_id` | NÃO | se enviado no store: deve existir e `is_active=true` |
| `cost_price` | NÃO | pode ser `NULL` |
| `stock_quantity` | NÃO | default **0** — pode iniciar a zero |
| `min_stock` | NÃO | default 0 |
| `image` | NÃO | API = upload; DB aceita path string |
| `is_active` | NÃO | default true |

### Categoria precisa existir antes?

- **Não é obrigatória** (`product_category_id` nullable).
- Se quiseres organizar por categoria: criar/usar registo em `product_categories` **antes** e passar o `id`.
- Hoje só existe: `id=1` `Cabelo Produtos`.

### Validações existentes (StoreProductRequest)

- name required string max 150  
- description nullable max **255**  
- sku/barcode nullable unique max 50  
- product_category_id nullable exists (ativos no create)  
- price required_without sale_price, numeric min 0  
- cost_price nullable numeric min 0  
- stock/min_stock sometimes integer min 0  
- image nullable image mimes jpeg/jpg/png/webp max 2048 KB  

### Resposta: “É seguro inserir produtos reais agora?”

**SIM** — com condições:

1. Usar só campos existentes (sem supplier/brand/ingredientes estruturados).  
2. Manter `description` ≤ 255 chars se inserir via API; textos de catálogo longos (modo de aplicação + ingredientes Wiñk) **não cabem** no contrato atual da API.  
3. Preferir `sku` único para idempotência futura.  
4. Garantir/criar categorias desejadas em `product_categories` (hoje só “Cabelo Produtos”).  
5. Imagens: upload via API **ou** path `/images/...` se inserção direta/controlada no DB (coluna aceita; API store espera ficheiro).  
6. Não depende de Suppliers/Payables/Receivables para o CRUD de produtos.

---

## 13. Idempotência futura (sem implementar)

Confirmado pelo schema real:

| Entidade | Melhor chave natural | Evidência |
|----------|----------------------|-----------|
| `Product` | `sku` | UNIQUE index `products_sku_unique` (nullable → vários NULL possíveis; para dados reais **preencher sku**) |
| `Product` (alternativa) | `barcode` | UNIQUE `products_barcode_unique` |
| `ProductCategory` | `slug` | UNIQUE `product_categories_slug_unique` |

Não assumir `name` como unique — **não há** índice unique em `products.name`.

---

## 14. Migrations aplicadas (batch)

Todas Ran: users/cache/jobs, Sanctum, clients, professionals, services, appointments, notifications, products, stock_movements, sales, sale_items, contact_messages, barcode, cash_*, services description/category, professional photo/extend, professional_schedules, professional_service, category tables + FK backfill, services.image.

---

## 15. Resumo executivo

```
BANCO:
SQLite → C:\Projetos\Pessoal\salon-management-system\backend\database\database.sqlite

PRODUCTS:
Tabela existe: SIM
Quantidade: 0
Colunas: id, name, description, sku, category, price, cost_price,
         stock_quantity, min_stock, image, is_active, created_at,
         updated_at, barcode, product_category_id
         (sem supplier_id)

PRODUCT CATEGORIES:
Tabela existe: SIM
Quantidade: 1
Categorias existentes:
  1 | Cabelo Produtos | cabelo-produtos | active | sort 0

SUPPLIERS:
Tabela existe: NÃO

PAYABLES:
Tabela existe: NÃO

RECEIVABLES:
Tabela existe: NÃO

É seguro inserir produtos reais agora?
SIM — para catálogo básico (name+price+stock+sku+categoria+imagem),
      com limite de description=255 via API e sem fornecedor/ERP.
```

---

*Documento gerado por auditoria de diagnóstico. Não substitui decisão de produto sobre migrar Suppliers/Payables/Receivables nem alargar o schema de produtos.*
