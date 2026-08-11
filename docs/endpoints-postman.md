# Endpoints Postman — Salon Management System API

Base URL: `{{base_url}}` = `http://127.0.0.1:8000`

Headers comuns (rotas autenticadas):

```
Accept: application/json
Content-Type: application/json
Authorization: Bearer {{auth_token}}
```

---

## Environment Postman sugerido

| Variável | Valor inicial |
|----------|---------------|
| `base_url` | `http://127.0.0.1:8000` |
| `auth_token` | *(preenchido após login)* |
| `appointment_id` | `1` |
| `client_id` | `1` |
| `professional_id` | `1` |
| `service_id` | `1` |
| `notification_id` | `1` |
| `product_id` | `1` |
| `sale_id` | `1` |

### Script de teste — Login (salvar token)

Na aba **Tests** do request `POST Login`:

```javascript
if (pm.response.code === 200) {
    const json = pm.response.json();
    if (json.token) {
        pm.environment.set('auth_token', json.token);
    }
}
```

---

## 0. Catálogo público (sem token)

### GET Serviços públicos

`GET {{base_url}}/api/v1/public/services?per_page=12`

### GET Produtos públicos

`GET {{base_url}}/api/v1/public/products?per_page=12`

### GET Produtos públicos

`GET {{base_url}}/api/v1/public/products?per_page=12`

### GET Profissionais públicos

`GET {{base_url}}/api/v1/public/professionals?per_page=12`

### GET Disponibilidade de horários

`GET {{base_url}}/api/v1/public/availability?professional_id=1&service_id=1&date=2026-06-25`

Resposta: `{ "data": { "slots": ["09:00", "09:30", ...] } }`

### POST Agendamento público

`POST {{base_url}}/api/v1/public/appointments`

```json
{
  "service_id": 1,
  "professional_id": 1,
  "date": "2026-06-25",
  "time": "10:00",
  "client_name": "Maria Silva",
  "client_email": "maria@example.com",
  "client_phone": "910000000",
  "notes": "Primeira visita"
}
```

> Se enviar `Authorization: Bearer {{auth_token}}` com role `client`, os campos `client_*` são opcionais.

### POST Contacto público

`POST {{base_url}}/api/v1/public/contact`

```json
{
  "name": "Pedro",
  "email": "pedro@example.com",
  "phone": "910000001",
  "subject": "Dúvida",
  "message": "Gostaria de saber mais sobre os serviços."
}
```

> Usado pelo site público Vue. Não requer `Authorization` (exceto endpoints autenticados abaixo).

---

## 1. Autenticação

### POST Register

`POST {{base_url}}/api/v1/auth/register`

```json
{
  "first_name": "João",
  "last_name": "Santos",
  "email": "joao.santos@example.com",
  "phone": "+351912000000",
  "password": "password123",
  "password_confirmation": "password123",
  "device_name": "postman"
}
```

> O role é sempre `client`; o campo `role` no body é ignorado.

### POST Login

`POST {{base_url}}/api/v1/auth/login`

```json
{
  "email": "admin@salon.test",
  "password": "password",
  "device_name": "postman"
}
```

**Resposta esperada (200):**

```json
{
  "message": "Login efetuado com sucesso.",
  "token": "1|...",
  "token_type": "Bearer",
  "user": { "id": 1, "role": "admin" }
}
```

### GET Me

`GET {{base_url}}/api/v1/auth/me`

### POST Logout

`POST {{base_url}}/api/v1/auth/logout`

---

## 2. Dashboard

### GET Dashboard

`GET {{base_url}}/api/v1/dashboard`

**Admin** — totais globais. **Profissional** — inclui bloco `professional`. **Cliente** — agendamentos futuros.

---

## 3. Utilizadores (admin)

### GET Listar

`GET {{base_url}}/api/v1/users?role=professional&per_page=15`

### POST Criar

`POST {{base_url}}/api/v1/users`

```json
{
  "first_name": "Carla",
  "last_name": "Mendes",
  "email": "carla@salon.test",
  "phone": "+351912111222",
  "password": "password123",
  "role": "professional",
  "is_active": true
}
```

### GET / PUT / DELETE

- `GET {{base_url}}/api/v1/users/2`
- `PUT {{base_url}}/api/v1/users/2`
- `DELETE {{base_url}}/api/v1/users/2`

---

## 4. Clientes

### GET Listar

`GET {{base_url}}/api/v1/clients?search=maria`

### POST Criar

`POST {{base_url}}/api/v1/clients`

```json
{
  "name": "Sofia Pereira",
  "email": "sofia@example.com",
  "phone": "+351912333444",
  "address": "Rua das Flores, 10",
  "birth_date": "1990-05-15",
  "notes": "Cliente nova",
  "is_active": true
}
```

### GET / PUT / DELETE

- `GET {{base_url}}/api/v1/clients/{{client_id}}`
- `PUT {{base_url}}/api/v1/clients/{{client_id}}`
- `DELETE {{base_url}}/api/v1/clients/{{client_id}}`

---

## 5. Profissionais

### POST Criar

`POST {{base_url}}/api/v1/professionals`

```json
{
  "user_id": 2,
  "name": "Ana Costa",
  "specialty": "Cabeleireiro",
  "phone": "+351900000002",
  "email": "ana.costa@salon.test",
  "commission_percentage": 30,
  "is_active": true
}
```

### GET Listar

`GET {{base_url}}/api/v1/professionals?is_active=true`

---

## 6. Serviços

### POST Criar

`POST {{base_url}}/api/v1/services`

```json
{
  "name": "Escova",
  "description": "Escova modeladora",
  "price": 25.00,
  "duration_minutes": 30,
  "is_active": true
}
```

### GET Listar

`GET {{base_url}}/api/v1/services?is_active=true`

---

## 7. Agendamentos

### POST Criar

`POST {{base_url}}/api/v1/appointments`

```json
{
  "client_id": 1,
  "service_id": 1,
  "professional_id": 1,
  "start_time": "2026-07-01T14:00:00",
  "notes": "Primeira visita",
  "status": "pending"
}
```

> O `end_time` e `total_price` são calculados automaticamente pelo backend.

### GET Listar (filtros)

`GET {{base_url}}/api/v1/appointments?professional_id=1&date=2026-07-01&status=pending`

### PUT Atualizar

`PUT {{base_url}}/api/v1/appointments/{{appointment_id}}`

```json
{
  "status": "confirmed",
  "notes": "Confirmado por telefone"
}
```

### DELETE Cancelar

`DELETE {{base_url}}/api/v1/appointments/{{appointment_id}}`

> Marca como `canceled` e `is_active: false` (soft cancel).

---

## 9. Produtos (admin / profissional)

> Clientes **não** têm acesso a estas rotas.

### GET Listar

`GET {{base_url}}/api/v1/products?search=shampoo&low_stock=true&per_page=15`

### POST Criar (admin)

`POST {{base_url}}/api/v1/products`

```json
{
  "name": "Shampoo Reparação",
  "description": "500ml",
  "sku": "SHP-001",
  "barcode": "1234567890123",
  "category": "Cabelo",
  "sale_price": 24.90,
  "purchase_price": 12.00,
  "current_stock": 20,
  "minimum_stock": 5,
  "is_active": true
}
```

### GET Detalhe

`GET {{base_url}}/api/v1/products/{{product_id}}`

### PUT Atualizar (admin)

`PUT {{base_url}}/api/v1/products/{{product_id}}`

### DELETE Desativar (admin)

`DELETE {{base_url}}/api/v1/products/{{product_id}}`

---

## 10. Stock (admin para movimentos; listagem admin/profissional)

### GET Movimentos

`GET {{base_url}}/api/v1/stock-movements?product_id=1&type=in`

### POST Entrada (admin)

`POST {{base_url}}/api/v1/products/{{product_id}}/stock/in`

```json
{
  "quantity": 10,
  "reason": "compra",
  "notes": "Fornecedor X"
}
```

### POST Saída (admin)

`POST {{base_url}}/api/v1/products/{{product_id}}/stock/out`

```json
{
  "quantity": 2,
  "reason": "perda",
  "notes": "Produto danificado"
}
```

### POST Ajuste manual (admin)

`POST {{base_url}}/api/v1/products/{{product_id}}/stock/adjust`

```json
{
  "new_quantity": 15,
  "reason": "inventário",
  "notes": "Contagem física"
}
```

> Stock negativo não é permitido. Vendas pagas com produtos geram saída automática.

### Endpoints ERP (Etapa 5)

`POST {{base_url}}/api/v1/stock/entry`

```json
{ "product_id": 1, "quantity": 10, "notes": "Compra fornecedor" }
```

`POST {{base_url}}/api/v1/stock/exit`

```json
{ "product_id": 1, "quantity": 2, "notes": "Uso interno" }
```

`POST {{base_url}}/api/v1/stock/adjustment`

```json
{ "product_id": 1, "new_quantity": 15, "notes": "Inventário" }
```

`GET {{base_url}}/api/v1/stock/history?product_id=1`

`GET {{base_url}}/api/v1/stock/low`

---

## 11. Caixa (admin / profissional)

`POST {{base_url}}/api/v1/cash/open` — `{ "opening_amount": 100 }`

`POST {{base_url}}/api/v1/cash/close` — `{ "closing_amount": 130 }`

`POST {{base_url}}/api/v1/cash/income` — `{ "amount": 50, "description": "Venda balcão" }`

`POST {{base_url}}/api/v1/cash/expense` — `{ "amount": 20, "description": "Material" }`

`GET {{base_url}}/api/v1/cash/current` — saldo do caixa aberto

`GET {{base_url}}/api/v1/cash/report?date=2026-06-23` — resumo diário

> Apenas um caixa pode estar aberto de cada vez.

---

## 12. Vendas / POS (admin / profissional)

### GET Listar

`GET {{base_url}}/api/v1/sales?status=paid&date=2026-06-23`

### POST Criar venda

`POST {{base_url}}/api/v1/sales`

```json
{
  "client_id": 1,
  "payment_method": "card",
  "status": "paid",
  "notes": "Venda balcão",
  "items": [
    { "product_id": 1, "quantity": 2 },
    { "service_id": 1, "quantity": 1 }
  ]
}
```

> `unit_price` é opcional — usa preço do produto/serviço. Venda `paid` com produto baixa stock.

### GET Detalhe

`GET {{base_url}}/api/v1/sales/{{sale_id}}`

### POST Cancelar

`POST {{base_url}}/api/v1/sales/{{sale_id}}/cancel`

> Repõe stock dos produtos da venda cancelada.

---

## 13. Notificações

### GET Listar

`GET {{base_url}}/api/v1/notifications?unread_only=true`

### PATCH Marcar como lida

`PATCH {{base_url}}/api/v1/notifications/{{notification_id}}/read`

### DELETE Remover

`DELETE {{base_url}}/api/v1/notifications/{{notification_id}}`

---

## Credenciais de teste (seeder)

| Perfil | Email | Password |
|--------|-------|----------|
| Admin | admin@salon.test | password |
| Profissional | ana.costa@salon.test | password |
| Cliente | maria.silva@salon.test | password |

---

## Importar collection

Ficheiro local: `postman/Salon-Laravel-API.postman_collection.json`

No Postman: **Import** → selecionar o ficheiro → escolher environment com `base_url` e `auth_token`.

## Ordem recomendada de testes

1. `POST Login` (admin)
2. `GET Dashboard` (inclui produtos, stock baixo, vendas do dia)
3. `GET Products` / `POST Products`
4. `POST /api/v1/stock/entry` ou `POST Stock In` (legado)
5. `POST /api/v1/cash/open` → `income` / `expense` → `close`
6. `POST Sales`
7. `GET Appointments`
8. `GET Notifications`
9. `POST Logout`
