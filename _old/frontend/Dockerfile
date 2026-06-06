# ---- Build stage ----
FROM node:20-alpine AS builder

WORKDIR /app

COPY package*.json ./
RUN npm install

COPY . .

# VITE_API_URL порожній — всі запити відносні, nginx проксює /api/ і /images/
ARG VITE_API_URL=""
ENV VITE_API_URL=$VITE_API_URL

RUN npm run build

# ---- Production stage ----
FROM nginx:alpine

# Копіюємо зібраний фронтенд
COPY --from=builder /app/dist /usr/share/nginx/html

# Копіюємо nginx конфіг
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
