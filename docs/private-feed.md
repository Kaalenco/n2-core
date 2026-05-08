# Accessing the Private NuGet Feed

This repository depends on packages from a private Azure Artifacts feed (`vault-packages`).

## Feed details

| | |
|---|---|
| Name | `vault-packages` |
| URL | `https://pkgs.dev.azure.com/gjkaal/3b3decf8-eba1-45d0-b47a-d9c1ce84f739/_packaging/vault-packages/nuget/v3/index.json` |

## One-time setup

Install the [Azure Artifacts Credential Provider](https://github.com/microsoft/artifacts-credprovider). Open a **PowerShell** terminal and run:

```powershell
iex "& { $(irm https://aka.ms/install-artifacts-credprovider.ps1) }"
```

This installs the provider to `%USERPROFILE%\.nuget\plugins` and only needs to be done once per machine.

## Authenticating

After installing the provider, trigger an interactive login on the first restore:

```powershell
dotnet restore --interactive
```

A device-code prompt will open in your browser. Once you complete the login the token is cached; subsequent restores work without `--interactive`.

If you are already signed in via the Azure CLI (`az login` with an account that has feed access), the credential provider picks up those credentials automatically.

## CI / service accounts

For pipelines or service accounts, add the feed credentials via an environment variable or a `nuget.config` that references a PAT stored as a pipeline secret. Do not commit tokens to source control.
