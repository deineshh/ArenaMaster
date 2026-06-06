# ArenaMaster

Платформа для організації аматорських кіберспортивних турнірів.

## Стек

- **Backend:** ASP.NET Minimal API (.NET 9), EF Core 9, PostgreSQL 16
- **Frontend:** React 19, Mantine 7, Redux Toolkit, React Router 7, Vite

## Запуск (Docker)

```bash
cp .env.example .env
cp arena-master-client/.env.example arena-master-client/.env
docker compose up --build
```

- API: http://localhost:5000
- Scalar: http://localhost:5000/scalar/v1
- Client: http://localhost:5173

## Локальний запуск

### База даних

```bash
docker compose up db -d
```

### API

```bash
cd ArenaMaster.Api
dotnet run
```

### Client

```bash
cd arena-master-client
npm install
npm run dev
```

## Тестові облікові записи (Development seed)

| Нікнейм | Email | Пароль | Роль |
|---------|-------|--------|------|
| admin_master | admin@arenamaster.ua | Test1234! | admin |
| dmytro_arena | dmytro.arena@gmail.com | Organizer1! | organizer |
| serhiy_blade | serhiy.blade@gmail.com | Test1234! | player |
