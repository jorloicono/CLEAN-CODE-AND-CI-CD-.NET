# GitHub Actions for .NET Projects

## Introduction

GitHub Actions is a continuous integration and continuous deployment (CI/CD) platform that allows you to automate your build, test, and deployment pipeline. This guide covers creating workflows for .NET projects with multiple actions and advanced configurations.

## Project Structure

First, create the workflow configuration folder in your repository:

```
.github/workflows/
```

This folder will contain YAML files that define your GitHub Actions workflows. Create a file named `dotnet-workflow.yml`.

## Basic .NET Workflow

### Initial Configuration

```yaml
name: Build and Test .NET Project

on: [push]

jobs:
  build:
    runs-on: ubuntu-latest

    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '7.0'
      - name: Restore dependencies
        run: dotnet restore
      - name: Build
        run: dotnet build --no-restore
      - name: Run tests
        run: dotnet test --no-build --verbosity normal
```

### Workflow File Breakdown

| Component | Description |
|-----------|-------------|
| `name: Build and Test .NET Project` | Display name for the workflow |
| `on: [push]` | Trigger event (executes on every push) |
| `jobs:` | Section containing one or more jobs |
| `build:` | Job identifier (required) |
| `runs-on: ubuntu-latest` | Runner environment (ubuntu-latest, windows-latest, macos-latest) |
| `steps:` | Sequence of tasks within the job |

## Available Actions for .NET

### 1. **Checkout Action**

```yaml
- uses: actions/checkout@v3
```

- **Purpose**: Checks out your repository code onto the runner
- **When to use**: Required at the start of almost every workflow
- **Latest version**: v3

### 2. **Setup .NET Action**

```yaml
- name: Setup .NET
  uses: actions/setup-dotnet@v3
  with:
    dotnet-version: '7.0'
```

- **Purpose**: Installs a specific .NET SDK version
- **Supported versions**: 3.1, 5.0, 6.0, 7.0, 8.0
- **Documentation**: [Setup .NET Action](https://github.com/actions/setup-dotnet)

### 3. **Restore Dependencies**

```yaml
- name: Restore dependencies
  run: dotnet restore
```

- **Purpose**: Restores NuGet packages
- **Command**: `dotnet restore` or `dotnet restore --force` (force fresh download)

### 4. **Build Action**

```yaml
- name: Build
  run: dotnet build --no-restore --configuration Release
```

- **Configuration options**: Debug, Release
- **Flags**: 
  - `--no-restore`: Skip restore step
  - `--verbosity`: quiet, minimal, normal, detailed, diagnostic

### 5. **Test Action**

```yaml
- name: Run tests
  run: dotnet test --no-build --verbosity normal --logger "trx;LogFileName=test-results.trx"
```

- **Test frameworks**: xUnit, NUnit, MSTest
- **Output**: Test results in TRX format (Visual Studio Test Results)

### 6. **Publish Action**

```yaml
- name: Publish
  run: dotnet publish --configuration Release --output ./publish --no-build
```

- **Purpose**: Prepares application for deployment
- **Output directory**: ./publish

### 7. **Code Coverage**

```yaml
- name: Generate coverage report
  run: dotnet test --collect:"XPlat Code Coverage" --verbosity normal
```

- **Tool**: XPlat Code Coverage (cross-platform)

## Advanced Workflow Example

```yaml
name: .NET Complete CI/CD Pipeline

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: ${{ matrix.os }}
    
    strategy:
      matrix:
        os: [ubuntu-latest, windows-latest]
        dotnet-version: ['6.0', '7.0']
    
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET ${{ matrix.dotnet-version }}
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: ${{ matrix.dotnet-version }}
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Build
        run: dotnet build --no-restore --configuration Release
      
      - name: Run unit tests
        run: dotnet test --no-build --filter Category=Unit --verbosity normal
      
      - name: Run integration tests
        run: dotnet test --no-build --filter Category=Integration --verbosity normal
      
      - name: Generate code coverage
        run: dotnet test /p:CollectCoverageMetrics=true /p:CoverageReportFormat=opencover
      
      - name: Publish test results
        uses: actions/upload-artifact@v3
        if: always()
        with:
          name: test-results-${{ matrix.os }}-${{ matrix.dotnet-version }}
          path: '**/test-results.trx'
      
      - name: Publish application
        run: dotnet publish --configuration Release --output ./publish --no-build
      
      - name: Upload artifacts
        uses: actions/upload-artifact@v3
        with:
          name: build-artifacts-${{ matrix.os }}
          path: ./publish

  code-quality:
    runs-on: ubuntu-latest
    needs: build
    
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '7.0'
      
      - name: Install code analysis tools
        run: dotnet tool install -g dotnet-format
      
      - name: Check code formatting
        run: dotnet format --verify-no-changes
      
      - name: Run SonarQube analysis
        env:
          SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}
        run: |
          dotnet sonarscanner begin /k:"project-key" /o:"organization" /d:sonar.host.url="https://sonarcloud.io" /d:sonar.login="${{ secrets.SONAR_TOKEN }}"
          dotnet build
          dotnet sonarscanner end /d:sonar.login="${{ secrets.SONAR_TOKEN }}"

  deploy:
    runs-on: ubuntu-latest
    needs: [build, code-quality]
    if: github.ref == 'refs/heads/main'
    
    steps:
      - uses: actions/checkout@v3
      
      - name: Download artifacts
        uses: actions/download-artifact@v3
        with:
          name: build-artifacts-ubuntu-latest
      
      - name: Deploy to Azure
        uses: azure/webapps-deploy@v2
        with:
          app-name: ${{ secrets.AZURE_APP_NAME }}
          publish-profile: ${{ secrets.AZURE_PUBLISH_PROFILE }}
          package: .
```

## Common Actions Library

| Action | Purpose | Example |
|--------|---------|---------|
| `actions/checkout@v3` | Clone repository | `uses: actions/checkout@v3` |
| `actions/setup-dotnet@v3` | Install .NET SDK | `uses: actions/setup-dotnet@v3` |
| `actions/upload-artifact@v3` | Store build outputs | `uses: actions/upload-artifact@v3` |
| `actions/download-artifact@v3` | Retrieve stored outputs | `uses: actions/download-artifact@v3` |
| `actions/cache@v3` | Cache dependencies | `uses: actions/cache@v3` |
| `github/codeql-action/init@v2` | Security scanning | `uses: github/codeql-action/init@v2` |
| `azure/login@v1` | Azure authentication | `uses: azure/login@v1` |
| `docker/build-push-action@v4` | Docker build and push | `uses: docker/build-push-action@v4` |

## Conditional Execution

```yaml
- name: Publish test results
  if: always()  # Run even if previous steps fail
  uses: actions/upload-artifact@v3
  with:
    name: test-results
    path: '**/test-results.trx'

- name: Deploy to production
  if: github.ref == 'refs/heads/main' && success()
  run: ./deploy.sh
```

## Matrix Builds

Test across multiple configurations:

```yaml
strategy:
  matrix:
    dotnet-version: ['6.0', '7.0', '8.0']
    os: [ubuntu-latest, windows-latest, macos-latest]
```

## Environment Variables and Secrets

```yaml
env:
  CONFIGURATION: Release
  BUILD_NUMBER: ${{ github.run_number }}

steps:
  - name: Deploy
    env:
      DEPLOY_TOKEN: ${{ secrets.DEPLOY_TOKEN }}
    run: dotnet publish -c Release
```

## Best Practices

✅ **Do:**
- Use specific action versions (avoid `@latest`)
- Run tests before build on main branches
- Cache dependencies for faster builds
- Use matrix strategy for multiple configurations
- Store sensitive data in GitHub Secrets
- Add meaningful step names
- Use conditional execution wisely

❌ **Don't:**
- Commit credentials or tokens
- Use `push` trigger for all branches without filtering
- Run unnecessary jobs sequentially
- Leave jobs without timeout specifications
- Forget to handle failed test scenarios

## Running Your Workflow

1. Create the `.github/workflows/` directory in your repository root
2. Add your YAML workflow file
3. Commit and push:
   ```bash
   git add .github/
   git commit -m "Add GitHub Actions workflow"
   git push
   ```
4. Navigate to your GitHub repository
5. Click the **Actions** tab to view workflow execution
6. Check logs for each job and step

***
