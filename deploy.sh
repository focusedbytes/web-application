#!/bin/bash

set -e  # Exit on error

# Configuration
DEPLOY_TARGET_NAME="${DEPLOY_TARGET_NAME:-focusedbytes.com}"
DEPLOY_PATH="${DEPLOY_PATH:-/var/www/focusedbytes}"
REPOSITORY_BRANCH="${REPOSITORY_BRANCH:-development}"
DOCKER_COMPOSE_FILE="${DOCKER_COMPOSE_FILE:-docker-compose.yml}"
REPO_URL="${REPO_URL:-https://github.com/your-username/your-repo.git}"

echo "🚀 Starting deployment to ${DEPLOY_TARGET_NAME}..."
echo "📋 Branch: ${REPOSITORY_BRANCH}"
echo "📁 Path: ${DEPLOY_PATH}"
echo "🐳 Compose file: ${DOCKER_COMPOSE_FILE}"
echo "📦 Repository: ${REPO_URL}"

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Create deployment directory if it doesn't exist
if [ ! -d "$DEPLOY_PATH" ]; then
    echo -e "${YELLOW}Creating deployment directory: $DEPLOY_PATH${NC}"
    sudo mkdir -p "$DEPLOY_PATH"
    sudo chown -R $USER:$USER "$DEPLOY_PATH"
fi

cd "$DEPLOY_PATH"

# Clone or pull latest code
if [ ! -d ".git" ]; then
    echo -e "${YELLOW}Cloning repository...${NC}"
    git clone "$REPO_URL" .
    git checkout "$REPOSITORY_BRANCH"
else
    echo -e "${YELLOW}Pulling latest changes...${NC}"
    git fetch origin
    git reset --hard origin/"$REPOSITORY_BRANCH"
fi

# Create .env file if it doesn't exist
if [ ! -f ".env" ]; then
    echo -e "${YELLOW}Creating .env file...${NC}"
    cat > .env <<EOF
POSTGRES_USER=${POSTGRES_USER:-postgres}
POSTGRES_PASSWORD=${POSTGRES_PASSWORD:-postgres}
PUBLIC_API_URL=${PUBLIC_API_URL:-http://localhost:5000}
ORIGIN=${ORIGIN:-http://localhost:3000}
EOF
fi

# Stop existing containers
echo -e "${YELLOW}Stopping existing containers...${NC}"
docker compose -f "$DOCKER_COMPOSE_FILE" down || true

# Remove old images to force rebuild
echo -e "${YELLOW}Removing old images...${NC}"
docker compose -f "$DOCKER_COMPOSE_FILE" rm -f || true

# Build and start containers
echo -e "${YELLOW}Building and starting containers...${NC}"
docker compose -f "$DOCKER_COMPOSE_FILE" up -d --build

# Wait for services to be ready
echo -e "${YELLOW}Waiting for services to be ready...${NC}"
echo -e "${YELLOW}Note: Migrations run automatically on backend startup${NC}"
sleep 15

# Check container status
echo -e "${YELLOW}Container status:${NC}"
docker compose -f "$DOCKER_COMPOSE_FILE" ps

# Check logs for any errors
echo -e "${YELLOW}Recent logs:${NC}"
docker compose -f "$DOCKER_COMPOSE_FILE" logs --tail=50

echo -e "${GREEN}✅ Deployment completed successfully!${NC}"
echo -e "${GREEN}Frontend: ${ORIGIN}${NC}"
echo -e "${GREEN}Backend API: ${PUBLIC_API_URL}${NC}"
