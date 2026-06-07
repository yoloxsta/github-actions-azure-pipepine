# ACR Multi-Environment Build and Push Pipeline

GitHub Actions workflow for building and pushing Docker images to Azure Container Registry using OIDC authentication with separate pipelines for dev, uat, and prod environments.

## Pipeline Architecture

```
main.yml (Router)
├── dev branch push   → dev.yml  (Docker Buildx)
├── uat branch push   → uat.yml  (Docker Buildx)
└── prod branch push  → prod.yml (ACR Tasks)
```

## Prerequisites

### 1. Create Federated Identity Credential

Run this command on Azure CLI:

```bash
az identity federated-credential create \
  --name github-actions \
  --resource-group <YOUR_RESOURCE_GROUP> \
  --identity-name <YOUR_MANAGED_IDENTITY_NAME> \
  --issuer https://token.actions.githubusercontent.com \
  --subject repo:<YOUR_GITHUB_USER>/<YOUR_REPO> \
  --audiences api://AzureADTokenExchange
```

### 2. Add GitHub Secrets

Go to: Repository → Settings → Secrets and variables → Actions

Add these secrets:

| Secret Name | Description |
|-------------|-------------|
| `AZURE_CLIENT_ID` | Managed Identity Client ID |
| `AZURE_SUBSCRIPTION_ID` | Azure Subscription ID |
| `AZURE_TENANT_ID` | Azure Tenant ID |

### 3. Configure GitHub Environments (Optional)

Go to: Repository → Settings → Environments

Create environments:
- `dev`
- `uat`
- `prod`

You can add protection rules (required reviewers, deployment branches) for each environment.

## Workflow Files

| File | Trigger | Build Method | Image Tag |
|------|---------|--------------|-----------|
| `main.yml` | Router | - | - |
| `dev.yml` | push to `dev` branch | Docker Buildx | `<IMAGE_NAME>:dev-latest` |
| `uat.yml` | push to `uat` branch | Docker Buildx | `<IMAGE_NAME>:uat-latest` |
| `prod.yml` | push to `prod` branch | ACR Tasks (`az acr build`) | `<IMAGE_NAME>:prod-latest` |

## Build Methods Comparison

### Docker Buildx (dev.yml, uat.yml)

```yaml
- name: Build and push Docker image
  uses: docker/build-push-action@v6
  with:
    context: .
    push: true
    tags: ${{ env.ACR_LOGIN_SERVER }}/${{ env.IMAGE_NAME }}:${{ env.IMAGE_TAG }}
    cache-from: type=gha
    cache-to: type=gha,mode=max
```

**Benefits:**
- Build runs on GitHub runner
- GitHub Actions cache support
- Good for development and testing

### ACR Tasks (prod.yml)

```yaml
- name: Build and Push using ACR Tasks
  run: |
    az acr build \
      --registry ${{ env.ACR_NAME }} \
      --image ${{ env.IMAGE_NAME }}:${{ env.IMAGE_TAG }} \
      --file Dockerfile \
      .
```

**Benefits:**
- Build runs on Azure (no GitHub runner resources)
- Faster for large images
- Built-in vulnerability scanning option
- Better for production workloads
- Native Azure integration

## Configuration

Update the `env` section in each workflow file:

```yaml
env:
  ACR_NAME: <YOUR_ACR_NAME>
  ACR_LOGIN_SERVER: <YOUR_ACR_NAME>.azurecr.io
  IMAGE_NAME: <YOUR_IMAGE_NAME>
  IMAGE_TAG: <ENVIRONMENT>-latest  # dev-latest, uat-latest, prod-latest
```

## Workflow Triggers

| Branch | Pipeline | Environment |
|--------|----------|-------------|
| `dev` | dev.yml | Development |
| `uat` | uat.yml | User Acceptance Testing |
| `prod` | prod.yml | Production |

## Usage

### Create branches and push

```bash
# Push to dev
git checkout -b dev
git push origin dev

# Push to uat
git checkout -b uat
git push origin uat

# Push to prod
git checkout -b prod
git push origin prod
```

### Manual dispatch

1. Go to Actions tab
2. Select "Main Pipeline Router"
3. Click "Run workflow"
4. Select branch (dev/uat/prod)

## Verify Deployment

```bash
# List images in ACR
az acr repository list --name <YOUR_ACR_NAME>

# Show tags for the image
az acr repository show-tags --name <YOUR_ACR_NAME> --repository <YOUR_IMAGE_NAME>

# Show specific environment tag
az acr repository show --name <YOUR_ACR_NAME> --image <YOUR_IMAGE_NAME>:dev-latest
```

## Troubleshooting

### Federated Identity Error

```
Error: AADSTS700213: No matching federated identity record found
```

**Solution:** Recreate federated credential with correct subject:

```bash
# Delete old credential
az identity federated-credential delete \
  --resource-group <YOUR_RESOURCE_GROUP> \
  --identity-name <YOUR_MANAGED_IDENTITY_NAME> \
  --name github-actions \
  --yes

# Create with wildcard subject
az identity federated-credential create \
  --name github-actions \
  --resource-group <YOUR_RESOURCE_GROUP> \
  --identity-name <YOUR_MANAGED_IDENTITY_NAME> \
  --issuer https://token.actions.githubusercontent.com \
  --subject repo:<YOUR_GITHUB_USER>/<YOUR_REPO> \
  --audiences api://AzureADTokenExchange
```

### Check Managed Identity

```bash
az identity show --resource-group <YOUR_RESOURCE_GROUP> --name <YOUR_MANAGED_IDENTITY_NAME>
```

### Check Federated Credential

```bash
az identity federated-credential list --resource-group <YOUR_RESOURCE_GROUP> --identity-name <YOUR_MANAGED_IDENTITY_NAME>
```

### Check ACR Permissions

```bash
az role assignment list \
  --assignee <YOUR_CLIENT_ID> \
  --scope /subscriptions/<YOUR_SUBSCRIPTION_ID>/resourceGroups/<YOUR_RESOURCE_GROUP>/providers/Microsoft.ContainerRegistry/registries/<YOUR_ACR_NAME>
```

### ACR Tasks Build Logs

```bash
# View recent ACR task runs
az acr task list-runs --registry <YOUR_ACR_NAME>

# View logs for specific run
az acr task logs --registry <YOUR_ACR_NAME> --run-id <RUN_ID>
```

## Security Best Practices

1. **Use GitHub Environments** - Add protection rules for uat and prod
2. **Limit branch access** - Configure which branches can deploy to each environment
3. **Required reviewers** - Add required approvers for production deployments
4. **Secret rotation** - Regularly review and rotate secrets
5. **Least privilege** - Ensure Managed Identity has minimum required permissions

## Clean Up

```bash
# Delete federated credential
az identity federated-credential delete \
  --resource-group <YOUR_RESOURCE_GROUP> \
  --identity-name <YOUR_MANAGED_IDENTITY_NAME> \
  --name github-actions \
  --yes
```
