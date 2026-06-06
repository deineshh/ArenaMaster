# ---- Build stage ----
FROM node:22-alpine AS builder

WORKDIR /app

COPY package*.json ./
RUN npm install

COPY . .

# VITE_API_URL порожній — всі запити відносні, nginx проксює /api/ і /uploads/
ARG VITE_API_URL=""
ENV VITE_API_URL=$VITE_API_URL

RUN npm run build

# ---- Production stage ----
FROM nginx:alpine

# Копіюємо зібраний фронтенд (React Router SPA outputs to build/client)
COPY --from=builder /app/build/client /usr/share/nginx/html

# Копіюємо nginx конфіг
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
