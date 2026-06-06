# Deploy Troubleshooting Guide

Типові помилки при підготовці проєкту до деплою на сервер через Docker.

---

## 1. `bcrypt` — нативна компіляція падає в Alpine

**Симптом:**
```
npm error node-pre-gyp ERR! not ok
npm error gyp ERR! find Python — You need to install the latest version of Python
```

**Причина:** `bcrypt` компілює нативний C++ модуль. В `node:alpine` немає Python і build-tools.

**Рішення:** Замінити `bcrypt` на `bcryptjs` (pure JS, ідентичне API):
```bash
npm uninstall bcrypt
npm install bcryptjs
```
Замінити імпорти у всіх файлах:
```js
// було
import bcrypt from 'bcrypt';
// стало
import bcrypt from 'bcryptjs';
```

---

## 2. Міграції не застосовуються — таблиці не існують

**Симптом:**
```
No migration found in prisma/migrations
The table `public.some_table` does not exist in the current database.
```

**Причина:** `.gitignore` містить `prisma/migrations/**/migration.sql` — SQL файли не потрапляють в репо, на сервері папка migrations порожня.

**Рішення:** Прибрати з `backend/.gitignore`:
```
# видалити цей рядок:
prisma/migrations/**/migration.sql
```
Закомітити всі `.sql` файли міграцій.

---

## 3. Зображення не завантажуються з Unsplash — `EAI_AGAIN`

**Симптом:**
```
✗ Помилка завантаження "...": getaddrinfo EAI_AGAIN api.unsplash.com
```

**Причина:** Сервер обмежує вихідний інтернет з Docker контейнерів. DNS не резолвиться.

**Рішення:** Вбудувати зображення в git і копіювати їх в volume при старті контейнера.

1. Прибрати з `backend/.gitignore`:
```
# видалити:
public/uploads/
!public/uploads/**/.gitkeep
```

2. В `backend/Dockerfile` додати копіювання в образ:
```dockerfile
COPY public /app/public_seed
```

3. В `docker-entrypoint.sh` синхронізувати в volume при старті:
```sh
cp -rf /app/public_seed/uploads/avatars/. /app/public/uploads/avatars/
cp -rf /app/public_seed/uploads/doctors/. /app/public/uploads/doctors/
cp -rf /app/public_seed/uploads/services/. /app/public/uploads/services/
```

4. В `unsplashService.js` — пропускати завантаження якщо файл вже існує:
```js
if (fs.existsSync(filepath)) {
  return `/uploads/${subDir}/${filename}.jpg`;
}
```

---

## 4. Фронтенд робить запити на `localhost:3000`

**Симптом:**
```
POST http://localhost:3000/api/v1/auth/login net::ERR_CONNECTION_REFUSED
```

**Причина:** `VITE_API_BASE_URL` передається порожнім рядком `""`, що є falsy — спрацьовує fallback `|| 'http://localhost:3000/api/v1'`.

**Рішення:** Передати явний відносний шлях в `docker-compose.yml`:
```yaml
args:
  VITE_API_BASE_URL: "/api/v1"
```
Nginx проксює `/api/` → `backend:3000/api/`.

---

## 5. Зображення завантажуються з `localhost:3000/uploads/...`

**Симптом:**
```
GET http://localhost:3000/uploads/doctors/doctor-ivanov.jpg net::ERR_CONNECTION_REFUSED
```

**Причина:** В `formatters.js` є fallback `|| 'http://localhost:3000'` для `VITE_API_ORIGIN`.

**Рішення:** Замінити `||` на `??` щоб порожній рядок не тригерив fallback:
```js
// було
const apiHost = import.meta.env.VITE_API_ORIGIN || 'http://localhost:3000';
// стало
const apiHost = import.meta.env.VITE_API_ORIGIN ?? '';
```
Тоді URL зображень стають відносними (`/uploads/...`) і nginx їх проксює.

---

## 6. `COPY public` падає при build — `/public: not found`

**Симптом:**
```
failed to calculate checksum of ref: "/public": not found
```

**Причина:** `public/uploads/` в `.gitignore` — на сервері після `git clone` папка відсутня або містить лише `.gitkeep`.

**Рішення:** Або прибрати з `.gitignore` (див. п.3), або прибрати `COPY public` з Dockerfile якщо зображення не критичні (seed завантажить їх з Unsplash якщо є інтернет).

---

## Загальний чеклист перед деплоєм

- [ ] Немає нативних npm пакетів (bcrypt → bcryptjs)
- [ ] SQL міграції закомічені в git
- [ ] `public/uploads/` закомічений або seed не залежить від інтернету
- [ ] `VITE_API_BASE_URL` = `/api/v1` (не порожній, не localhost)
- [ ] `VITE_API_ORIGIN` = `""` і в коді використовується `??` замість `||`
- [ ] nginx проксює `/api/`, `/uploads/`, `/health`
- [ ] Секрети винесені в `.env.production` в корені (не в git)
