# Deployment Guide - FocusedBytes

This guide explains how to set up CI/CD for automatic deployment to development.focusedbytes.com.

## Architecture

- **GitHub Actions** - CI/CD pipeline
- **Docker Compose** - Container orchestration
- **VPS** - Deployment target (with SSH access)

## Prerequisites

1. VPS server with:
   - Ubuntu 20.04+ or similar Linux distribution
   - Docker and Docker Compose installed
   - SSH access configured
   - Domain pointing to server (development.focusedbytes.com)

2. GitHub repository with:
   - `development` branch
   - GitHub Secrets configured (see below)

## VPS Server Setup

### 1. Install Docker and Docker Compose

```bash
# Update system
sudo apt update && sudo apt upgrade -y

# Install Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# Add user to docker group
sudo usermod -aG docker $USER

# Install Docker Compose
sudo apt install docker-compose-plugin -y

# Verify installation
docker --version
docker compose version
```

### 2. Create Deployment User (Optional but Recommended)

```bash
# Create deployment user
sudo adduser deploy
sudo usermod -aG docker deploy

# Switch to deploy user
su - deploy
```

### 3. Set Up SSH Key for GitHub Actions

```bash
# Generate SSH key (on your local machine)
ssh-keygen -t ed25519 -C "github-actions-deploy" -f ~/.ssh/github_actions_deploy

# Copy public key to server
ssh-copy-id -i ~/.ssh/github_actions_deploy.pub deploy@YOUR_SERVER_IP

# Test connection
ssh -i ~/.ssh/github_actions_deploy deploy@YOUR_SERVER_IP
```

### 4. Configure Caddy Reverse Proxy

Caddy automatically handles SSL certificates via Let's Encrypt!

Edit your Caddy configuration:

```bash
sudo nano /etc/caddy/Caddyfile
```

Add this configuration for `development.focusedbytes.com`:

```caddy
development.focusedbytes.com {
    # Backend API
    handle /api/* {
        reverse_proxy localhost:5000
    }

    # Health check endpoint
    handle /health {
        reverse_proxy localhost:5000
    }

    # Swagger/API docs (optional, only for development)
    handle /swagger* {
        reverse_proxy localhost:5000
    }

    # Frontend (SvelteKit) - handles all other routes
    handle {
        reverse_proxy localhost:3000
    }
}
```

Reload Caddy:

```bash
sudo systemctl reload caddy
```

**That's it!** Caddy will automatically:
- ✅ Obtain SSL certificate from Let's Encrypt
- ✅ Handle HTTPS redirects
- ✅ Renew certificates automatically

### 5. Update Your Environment Secrets

Since you're using Caddy, your environment secrets should be:

- `PUBLIC_API_URL`: `https://development.focusedbytes.com/api`
- `ORIGIN`: `https://development.focusedbytes.com`

No ports needed in the URLs!

## Your Caddy Configuration

Based on your existing Hetzner server setup, here's what to add to `/etc/caddy/Caddyfile`:

### For Development (development.focusedbytes.com)

```caddy
development.focusedbytes.com {
    # Backend API routes
    handle /api/* {
        reverse_proxy localhost:5010
    }

    # Health check endpoint
    handle /health {
        reverse_proxy localhost:5010
    }

    # Swagger/API docs (development only)
    handle /swagger* {
        reverse_proxy localhost:5010
    }

    # Frontend (SvelteKit) - all other routes
    handle {
        reverse_proxy localhost:3010
    }

    # Logging (optional)
    log {
        output file /var/log/caddy/development.focusedbytes.com.log
    }
}
```

### For Production (focusedbytes.com)

Update your existing `focusedbytes.com` block:

```caddy
focusedbytes.com {
    # Backend API routes
    handle /api/* {
        reverse_proxy localhost:5020
    }

    # Health check endpoint
    handle /health {
        reverse_proxy localhost:5020
    }

    # Frontend (SvelteKit) - all other routes
    # Note: Removes static file serving and studio auth
    handle {
        reverse_proxy localhost:3020
    }

    # Logging
    log {
        output file /var/log/caddy/focusedbytes.com.log
    }
}

# Keep your existing analytics subdomain
analytics.focusedbytes.com {
    reverse_proxy localhost:3001
}
```

### Port Mapping Summary

| Environment | Service | Docker Port | Caddy Proxies From |
|-------------|---------|-------------|-------------------|
| **Development** | Frontend | 3010 | https://development.focusedbytes.com |
| **Development** | Backend | 5010 | https://development.focusedbytes.com/api |
| **Production** | Frontend | 3020 | https://focusedbytes.com |
| **Production** | Backend | 5020 | https://focusedbytes.com/api |
| **Analytics** | Umami (existing) | 3001 | https://analytics.focusedbytes.com |

### Apply Caddy Changes

After editing `/etc/caddy/Caddyfile`:

```bash
# Validate configuration
sudo caddy validate --config /etc/caddy/Caddyfile

# Reload Caddy (zero downtime)
sudo systemctl reload caddy

# Check status
sudo systemctl status caddy

# View logs
sudo journalctl -u caddy -f
```

## GitHub Environments Configuration

### Step 1: Create Environments

Go to your GitHub repository: **Settings → Environments**

Create two environments:
1. Click **New environment**
2. Name it `development` → Click **Configure environment**
3. Repeat for `production`

### Step 2: Configure Production Protection (Recommended)

For the `production` environment:
- ✅ **Required reviewers**: Add yourself or team members
- ✅ **Wait timer**: 5 minutes (optional - gives time to cancel)
- ✅ **Deployment branches**: Select `Selected branches` → Add `main`

For `development` environment: No protection needed.

### Step 3: Add Environment Secrets

**For Development Environment:**

Go to **Settings → Environments → development → Add secret**

Add the following secrets:

| Secret Name | Description | Example |
|-------------|-------------|---------|
| `SSH_PRIVATE_KEY` | Private SSH key for deployment | Contents of `~/.ssh/github_actions_deploy` |
| `SERVER_HOST` | Development server IP or domain | Your Hetzner server IP |
| `SERVER_PORT` | SSH port | `22` (default) |
| `SERVER_USER` | SSH user on VPS | `root` |
| `DEPLOY_PATH` | Deployment directory | `/var/www/focusedbytes-dev` |
| `POSTGRES_USER` | PostgreSQL username | `fb_dev_user` |
| `POSTGRES_PASSWORD` | PostgreSQL password | `dev_password_here` |
| `PUBLIC_API_URL` | Backend API URL (with Caddy) | `https://development.focusedbytes.com/api` |
| `ORIGIN` | Frontend origin URL (with Caddy) | `https://development.focusedbytes.com` |

**For Production Environment:**

Go to **Settings → Environments → production → Add secret**

Add the same secret names with **production values**:

| Secret Name | Production Value Example |
|-------------|--------------------------|
| `SSH_PRIVATE_KEY` | Same or different SSH key for prod server |
| `SERVER_HOST` | Production server IP (or same Hetzner if shared) |
| `SERVER_PORT` | `22` (default) |
| `SERVER_USER` | `root` (or create dedicated `deploy` user) |
| `DEPLOY_PATH` | `/var/www/focusedbytes` |
| `POSTGRES_USER` | `fb_prod_user` |
| `POSTGRES_PASSWORD` | **STRONG different password!** |
| `PUBLIC_API_URL` | `https://focusedbytes.com/api` |
| `ORIGIN` | `https://focusedbytes.com` |

### How to Add SSH_PRIVATE_KEY Secret

1. On your local machine, display the private key:
   ```bash
   cat ~/.ssh/github_actions_deploy
   ```

2. Copy the **entire output** including:
   ```
   -----BEGIN OPENSSH PRIVATE KEY-----
   ...
   -----END OPENSSH PRIVATE KEY-----
   ```

3. Paste it into GitHub Secrets as `SSH_PRIVATE_KEY`

## Deployment Workflow

### Automatic Deployment to Development

When you push to the `development` branch:

```bash
git checkout development
git add .
git commit -m "feat: add new feature"
git push origin development
```

GitHub Actions will automatically:
1. ✅ Checkout code
2. ✅ Use **development** environment secrets
3. ✅ Set up SSH connection to dev server
4. ✅ Connect to VPS
5. ✅ Pull latest code
6. ✅ Build Docker images
7. ✅ Run database migrations
8. ✅ Start containers
9. ✅ Verify deployment

### Automatic Deployment to Production

When you push to the `main` branch:

```bash
git checkout main
git merge development  # Merge tested changes
git push origin main
```

GitHub Actions will:
1. ✅ Use **production** environment secrets
2. ✅ **Wait for approval** (if configured)
3. ✅ Deploy to production server
4. ✅ Run migrations
5. ✅ Verify deployment

**Note**: If you enabled "Required reviewers" for production, you must approve the deployment in GitHub Actions UI.

### Manual Deployment (on VPS)

If you need to deploy manually:

```bash
ssh deploy@YOUR_SERVER_IP
cd /var/www/focusedbytes
git pull origin development
docker compose down
docker compose up -d --build
docker compose exec backend dotnet ef database update --context EventStoreDbContext
docker compose exec backend dotnet ef database update --context ReadModelDbContext
```

## Monitoring

### Check Container Status

```bash
ssh deploy@YOUR_SERVER_IP
cd /var/www/focusedbytes
docker compose ps
```

### View Logs

```bash
# All services
docker compose logs -f

# Specific service
docker compose logs -f backend
docker compose logs -f frontend
docker compose logs -f postgres
```

### Health Checks

```bash
# Backend health
curl http://development.focusedbytes.com:5000/health

# Frontend
curl http://development.focusedbytes.com:3000
```

## Troubleshooting

### Deployment Failed

1. Check GitHub Actions logs in your repository
2. SSH into server and check Docker logs:
   ```bash
   docker compose logs -f
   ```

### Database Connection Issues

```bash
# Check PostgreSQL container
docker compose exec postgres psql -U postgres -d focusedbytes

# Reset database (WARNING: deletes all data)
docker compose down -v
docker compose up -d
```

### Containers Not Starting

```bash
# Check container status
docker compose ps

# Rebuild without cache
docker compose down
docker compose build --no-cache
docker compose up -d
```

### Port Already in Use

```bash
# Find process using port
sudo lsof -i :5000
sudo lsof -i :3000

# Kill process if needed
sudo kill -9 <PID>
```

## Security Recommendations

1. **Use Strong Passwords**
   - Generate secure PostgreSQL password: `openssl rand -base64 32`

2. **Restrict SSH Access**
   ```bash
   # Edit SSH config
   sudo nano /etc/ssh/sshd_config

   # Add these lines:
   PermitRootLogin no
   PasswordAuthentication no
   AllowUsers deploy

   # Restart SSH
   sudo systemctl restart sshd
   ```

3. **Enable UFW Firewall**
   ```bash
   sudo ufw allow OpenSSH
   sudo ufw allow 'Nginx Full'
   sudo ufw enable
   ```

4. **Keep System Updated**
   ```bash
   sudo apt update && sudo apt upgrade -y
   ```

## Production Deployment

For production deployment (production.focusedbytes.com):

1. Create `production` branch
2. Copy `.github/workflows/deploy-development.yml` to `deploy-production.yml`
3. Update branch trigger to `production`
4. Add production-specific secrets
5. Use separate database and stricter security

## Useful Commands

```bash
# Restart all services
docker compose restart

# View resource usage
docker stats

# Clean up unused images
docker system prune -a

# Backup database
docker compose exec postgres pg_dump -U postgres focusedbytes > backup.sql

# Restore database
docker compose exec -T postgres psql -U postgres focusedbytes < backup.sql
```

## Support

For issues or questions:
- Check logs: `docker compose logs -f`
- Review GitHub Actions workflow runs
- Contact: fb@focusedbytes.com

---

**Last Updated**: 2025-11-15
