# ACR Build and Push Pipeline

GitHub Actions workflow for building and pushing Docker images to Azure Container Registry using OIDC authentication.

## Prerequisites

### 1. Create Federated Identity Credential

Run this command on Azure CLI:

```bash
az identity federated-credential create \
  --name github-actions \
  --resource-group <YOUR_RESOURCE_GROUP> \
  --identity-name <YOUR_MANAGED_IDENTITY_NAME> \
  --issuer https://token.actions.githubusercontent.com \
  --subject repo:<YOUR_GITHUB_USER>/<YOUR_REPO>:ref:refs/heads/main \
  --audiences api://AzureADTokenExchange
```

### 2. Add GitHub Secrets

Go to: Repository → Settings → Secrets and variables → Actions

Add these secrets:

| Secret Name | Value |
|-------------|-------|
| `AZURE_CLIENT_ID` | Your Managed Identity Client ID |
| `AZURE_SUBSCRIPTION_ID` | Your Azure Subscription ID |
| `AZURE_TENANT_ID` | Your Azure Tenant ID |

## Configuration

Update the `env` section in `.github/workflows/acr-build-push.yml`:

| Setting | Description |
|---------|-------------|
| `ACR_NAME` | Your ACR name (without .azurecr.io) |
| `ACR_LOGIN_SERVER` | Your ACR login server (name.azurecr.io) |
| `IMAGE_NAME` | Docker image name |

## Workflow Triggers

- Push to `main` or `master` branch
- Pull requests to `main` or `master` branch
- Manual dispatch from Actions tab

## Image Tag

The workflow pushes a single tag:
- `<ACR_LOGIN_SERVER>/<IMAGE_NAME>:latest`

## Verify Deployment

```bash
# List images in ACR
az acr repository list --name <YOUR_ACR_NAME>

# Show tags for the app image
az acr repository show-tags --name <YOUR_ACR_NAME> --repository <IMAGE_NAME>
```

## Troubleshooting

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
