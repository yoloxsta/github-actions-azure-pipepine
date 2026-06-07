# ACR Build and Push Pipeline

GitHub Actions workflow for building and pushing Docker images to Azure Container Registry.

## Prerequisites

### 1. Configure Federated Identity for OIDC

Run the following Azure CLI commands to set up the federated identity credential:

```bash
# Get your Managed Identity resource ID
IDENTITY_RESOURCE_ID=$(az identity show \
  --resource-group aia-mi \
  --name aia-mi \
  --query id -o tsv)

# Create federated identity credential for GitHub Actions
az identity federated-credential create \
  --name github-actions \
  --resource-group aia-mi \
  --identity-name mi-ado-agent \
  --issuer https://token.actions.githubusercontent.com \
  --subject repo:yoloxsta/github-actions-azure-pipepine:ref:refs/heads/main \
  --audiences api://AzureADTokenExchange
```

Repository: https://github.com/yoloxsta/github-actions-azure-pipepine

### 2. Add GitHub Secret

Add the following secret to your GitHub repository:

- Go to: Repository → Settings → Secrets and variables → Actions
- Add secret:
  - **Name**: `AZURE_TENANT_ID`
  - **Value**: Your Azure tenant ID (find it with `az account show --query tenantId -o tsv`)

### 3. ACR Permissions

Ensure the Managed Identity has the following role on the ACR:
- `AcrPush` role
- `AcrPull` role

You can verify with:
```bash
az role assignment list \
  --assignee 6965c5d5-bbe2-4ce9-82d9-51f4fd87f6ff \
  --scope /subscriptions/295e3026-e7e5-469f-b273-e0aed9438bc3/resourceGroups/aia-mi/providers/Microsoft.ContainerRegistry/registries/myacrlab12345
```

## Workflow Details

| Setting | Value |
|---------|-------|
| **ACR Name** | myacrlab12345 |
| **ACR Login Server** | myacrlab12345.azurecr.io |
| **Image Name** | app |
| **Subscription ID** | 295e3026-e7e5-469f-b273-e0aed9438bc3 |
| **Resource Group** | aia-mi |
| **Managed Identity Client ID** | 4aabd5c7-07bc-4b18-8895-478c2ced7741 |

## Authentication Method

This workflow uses **OIDC (OpenID Connect)** for passwordless authentication with Azure. Benefits:
- No secrets or credentials stored in GitHub
- Short-lived tokens (more secure)
- No credential rotation required

## Tags Applied

Each build creates two tags:
- `<sha>`: The Git commit SHA (e.g., `app:abc123`)
- `latest`: The latest build

## Triggering the Workflow

The workflow runs on:
- Push to `main` or `master` branches
- Pull requests to `main` or `master` branches
- Manual dispatch via GitHub Actions UI

## Verify Deployment

After a successful build, verify the image in ACR:

```bash
az acr repository list --name myacrlab12345
az acr repository show-tags --name myacrlab12345 --repository app
```

## Troubleshooting

### Common Issues

1. **Authentication failed**: Verify the federated identity is configured correctly and the GitHub secret `AZURE_TENANT_ID` is set.

2. **Permission denied on ACR**: Ensure the Managed Identity has `AcrPush` role on the registry.

3. **Docker build fails**: Check your Dockerfile is valid and all dependencies are available.

### Useful Commands

```bash
# Check Managed Identity
az identity show --resource-group aia-mi --name aia-mi

# List ACR repositories
az acr repository list --name myacrlab12345

# View workflow logs
# Go to GitHub → Actions → Select the workflow run
```
