# Quick Start - CI/CD Deployment

This is a quick reference for deploying FocusedBytes to your Hetzner server.

## Overview

- **Server**: Hetzner 4GB instance
- **Reverse Proxy**: Caddy (automatic HTTPS)
- **Containers**: Docker Compose
- **CI/CD**: GitHub Actions with Environment Secrets

## Port Mapping

| Environment | Frontend | Backend | Database |
|-------------|----------|---------|----------|
| Development | 3010     | 5010    | 5432     |
| Production  | 3020     | 5020    | 5433     |

## Quick Setup Steps

### 1. Configure DNS

Point these domains to your Hetzner server IP:
- `development.focusedbytes.com` → Your Hetzner IP
- `focusedbytes.com` → Your Hetzner IP (or keep existing)

### 2. Add to Caddy Configuration

SSH to server and edit `/etc/caddy/Caddyfile`:

```bash
sudo nano /etc/caddy/Caddyfile
```

Add this block:

```caddy
development.focusedbytes.com {
    handle /api/* {
        reverse_proxy localhost:5010
    }
    handle /health {
        reverse_proxy localhost:5010
    }
    handle /swagger* {
        reverse_proxy localhost:5010
    }
    handle {
        reverse_proxy localhost:3010
    }
}
```

Reload Caddy:

```bash
sudo caddy validate --config /etc/caddy/Caddyfile
sudo systemctl reload caddy
```

### 3. Create GitHub Environments

**GitHub Repo → Settings → Environments**

Create `development` and `production` environments.

### 4. Add Environment Secrets

**For development environment**, add these 9 secrets:

| Secret | Value |
|--------|-------|
| `SSH_PRIVATE_KEY` | Your SSH private key |
| `SERVER_HOST` | Your Hetzner IP address |
| `SERVER_PORT` | `22` |
| `SERVER_USER` | `root` |
| `DEPLOY_PATH` | `/var/www/focusedbytes-dev` |
| `POSTGRES_USER` | `fb_dev_user` |
| `POSTGRES_PASSWORD` | Generate: `openssl rand -base64 32` |
| `PUBLIC_API_URL` | `https://development.focusedbytes.com/api` |
| `ORIGIN` | `https://development.focusedbytes.com` |

**For production environment**, same secret names but different values:

| Secret | Production Value |
|--------|------------------|
| `DEPLOY_PATH` | `/var/www/focusedbytes` |
| `POSTGRES_USER` | `fb_prod_user` |
| `POSTGRES_PASSWORD` | Different strong password |
| `PUBLIC_API_URL` | `https://focusedbytes.com/api` |
| `ORIGIN` | `https://focusedbytes.com` |

### 5. Generate SSH Key for GitHub Actions

On your local machine:

```bash
ssh-keygen -t ed25519 -C "github-actions" -f ~/.ssh/github_actions

# Copy to server
ssh-copy-id -i ~/.ssh/github_actions.pub root@YOUR_HETZNER_IP

# Display private key (copy to GitHub secret)
cat ~/.ssh/github_actions
```

### 6. Deploy!

#### Deploy to Development:

```bash
git checkout -b development
git push -u origin development
```

GitHub Actions will automatically deploy to `https://development.focusedbytes.com`

#### Deploy to Production:

```bash
git checkout main
git merge development
git push origin main
```

GitHub Actions will deploy to `https://focusedbytes.com` (may need approval if configured).

## Monitoring

### Check Deployment Status

- GitHub: **Actions** tab in your repository
- Watch live deployment logs

### Check Running Containers on Server

```bash
ssh root@YOUR_HETZNER_IP
cd /var/www/focusedbytes-dev  # or /var/www/focusedbytes for prod
docker compose ps
docker compose logs -f
```

### View Caddy Logs

```bash
sudo journalctl -u caddy -f
```

### Access Services

- Development: https://development.focusedbytes.com
- Production: https://focusedbytes.com
- API Swagger (dev only): https://development.focusedbytes.com/swagger
- Health Check: https://development.focusedbytes.com/health

## Troubleshooting

### Deployment Failed

1. Check GitHub Actions logs
2. SSH to server and check Docker logs:
   ```bash
   docker compose logs -f
   ```

### SSL Certificate Issues

```bash
# Check Caddy status
sudo systemctl status caddy

# View Caddy logs
sudo journalctl -u caddy --no-pager -n 50
```

### Containers Won't Start

```bash
# Check what's using the ports
sudo lsof -i :3000
sudo lsof -i :5000

# Restart containers
docker compose restart
```

## File Structure

```
/var/www/
├── focusedbytes-dev/                       # Development deployment
│   ├── docker-compose.development.yml      # Dev config (ports 3010, 5010)
│   └── .env
└── focusedbytes/                           # Production deployment
    ├── docker-compose.production.yml       # Prod config (ports 3020, 5020)
    └── .env
```

## Next Steps

1. ✅ Set up development deployment first
2. ✅ Test everything works
3. ✅ Set up production environment with protection rules
4. ✅ Configure production Caddy block
5. ✅ Deploy to production

## Full Documentation

See [DEPLOYMENT.md](DEPLOYMENT.md) for complete details.

---

**Questions?** Contact: fb@focusedbytes.com
