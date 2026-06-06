#!/bin/sh
set -e

echo "==> Ensuring image directories exist..."
mkdir -p /app/public/images/products /app/public/images/users /app/public/images/chains

echo "==> Syncing seed images to volume..."
if [ -d "/app/public_seed/images" ]; then
    echo "Found seed files, copying..."
    cp -r /app/public_seed/images/products/. /app/public/images/products/
    cp -r /app/public_seed/images/chains/. /app/public/images/chains/
    cp -r /app/public_seed/images/users/. /app/public/images/users/
    echo "Image sync completed."
else
    echo "No seed files found in /app/public_seed"
fi

echo "==> Waiting for PostgreSQL to be ready..."
MAX_RETRIES=30
RETRY=0
until npx prisma migrate deploy 2>/dev/null; do
    RETRY=$((RETRY + 1))
    if [ "$RETRY" -ge "$MAX_RETRIES" ]; then
        echo "ERROR: PostgreSQL not available after $MAX_RETRIES attempts. Exiting."
        exit 1
    fi
    echo "   Migration failed (attempt $RETRY/$MAX_RETRIES), retrying in 3s..."
    sleep 3
done
echo "==> Migrations applied!"

echo "==> Running seed..."
node prisma/seeds/index.js
echo "==> Seed completed!"

echo "==> Starting server..."
exec node server.js
