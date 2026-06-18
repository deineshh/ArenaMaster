FROM node:20-alpine

WORKDIR /app

# OpenSSL для Prisma
RUN apk add --no-cache openssl

# Копіюємо package files
COPY package*.json ./

# Встановлюємо залежності
RUN npm install

# Копіюємо Prisma схему і генеруємо клієнт
COPY prisma ./prisma
RUN npx prisma generate

# Копіюємо весь код
COPY src ./src
COPY server.js ./
COPY prisma.config.js ./

# Зображення-seed вбудовуємо в образ окремо від volume
# /app/public буде перекрито volume, тому seed йде в /app/public_seed
COPY public /app/public_seed

# Копіюємо entrypoint
COPY docker-entrypoint.sh ./
RUN chmod +x docker-entrypoint.sh

# Створюємо директорії для зображень
RUN mkdir -p /app/public/images/products /app/public/images/users /app/public/images/chains

EXPOSE 3000

ENTRYPOINT ["./docker-entrypoint.sh"]
