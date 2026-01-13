# GitHub Actions with .NET: Complete Practical Guide

---

## GitHub Actions Fundamentals

### What is GitHub Actions?

GitHub Actions is a continuous integration/continuous delivery (CI/CD) platform that allows you to automate your build, test, and deployment pipeline. It's built directly into GitHub and triggers based on events (push, pull request, schedule, etc.).

### Key Concepts

- **Workflow**: A configured automation process defined in YAML
- **Event**: The activity that triggers a workflow (push, pull request, schedule)
- **Job**: A set of steps that execute on the same runner
- **Step**: An individual task that can run commands or actions
- **Action**: A reusable unit of code
- **Runner**: A server that executes your workflow

### Workflow Structure

```yaml
name: CI Pipeline                    # Workflow name
on:                                  # Trigger events
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]
  workflow_dispatch:                 # Manual trigger

jobs:
  build:                             # Job name
    runs-on: ubuntu-latest           # Runner environment
    steps:
      - uses: actions/checkout@v4    # Action (checkout code)
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - name: Build                  # Custom step name
        run: dotnet build             # Shell command
      - name: Test
        run: dotnet test
```

---

## Basic .NET CI/CD Pipeline

### Step 1: Create Repository Structure

```
MyProject/
├── .github/
│   └── workflows/
│       ├── ci.yml
│       ├── test.yml
│       └── deploy.yml
├── src/
│   └── MyApp/
│       ├── Program.cs
│       └── MyApp.csproj
├── tests/
│   └── MyApp.Tests/
│       ├── UnitTest1.cs
│       └── MyApp.Tests.csproj
├── MyProject.sln
└── README.md
```

### Step 2: Simple CI Workflow (ci.yml)

```yaml
name: Build and Test

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]
  workflow_dispatch:

env:
  DOTNET_VERSION: '8.0.x'

jobs:
  build:
    runs-on: ubuntu-latest
    name: Build .NET Application
    
    steps:
      - name: Checkout code
        uses: actions/checkout@v4
      
      - name: Setup .NET SDK
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}
      
      - name: Restore NuGet packages
        run: dotnet restore
      
      - name: Build solution
        run: dotnet build --configuration Release --no-restore
      
      - name: Run unit tests
        run: dotnet test --configuration Release --no-build --logger "trx;LogFileName=test-results.trx"
      
      - name: Upload test results
        if: always()
        uses: actions/upload-artifact@v3
        with:
          name: test-results
          path: '**/test-results.trx'

  code-quality:
    runs-on: ubuntu-latest
    name: Code Quality Checks
    
    steps:
      - uses: actions/checkout@v4
      
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}
      
      - name: Install code analysis tools
        run: |
          dotnet tool install -g dotnet-format
          dotnet tool install -g dotnet-codeanalysis
      
      - name: Run code formatting check
        run: dotnet format --verify-no-changes
```

---

## Complete Working Examples

### Example 1: Console Application Pipeline

**Project Structure**: Simple console app with unit tests

**Repository**: 
```
console-app/
├── .github/workflows/dotnet.yml
├── src/ConsoleApp/Program.cs
├── tests/ConsoleApp.Tests/ProgramTests.cs
└── ConsoleApp.sln
```

**Workflow File (.github/workflows/dotnet.yml)**:

```yaml
name: .NET Console App CI

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --configuration Release
    
    - name: Test
      run: dotnet test --configuration Release --no-build --verbosity normal

  publish:
    needs: build-and-test
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0'
    
    - name: Publish application
      run: dotnet publish src/ConsoleApp/ConsoleApp.csproj -c Release -o ./publish
    
    - name: Upload artifacts
      uses: actions/upload-artifact@v3
      with:
        name: published-app
        path: ./publish
```

### Example 2: Web API Pipeline with Docker

**Workflow for ASP.NET Core API**:

```yaml
name: Build and Deploy Web API

on:
  push:
    branches: [ main ]
    paths:
      - 'src/**'
      - '.github/workflows/webapi.yml'
  pull_request:
    branches: [ main ]

env:
  REGISTRY: ghcr.io
  IMAGE_NAME: ${{ github.repository }}

jobs:
  build:
    runs-on: ubuntu-latest
    
    permissions:
      contents: read
      packages: write
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0'
    
    - name: Restore
      run: dotnet restore
    
    - name: Build
      run: dotnet build --configuration Release --no-restore
    
    - name: Test
      run: dotnet test --configuration Release --no-build
    
    - name: Publish
      run: dotnet publish src/WebApi/WebApi.csproj -c Release -o ./app
    
    - name: Set up Docker Buildx
      uses: docker/setup-buildx-action@v2
    
    - name: Log in to Container Registry
      uses: docker/login-action@v2
      with:
        registry: ${{ env.REGISTRY }}
        username: ${{ github.actor }}
        password: ${{ secrets.GITHUB_TOKEN }}
    
    - name: Extract metadata
      id: meta
      uses: docker/metadata-action@v4
      with:
        images: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}
        tags: |
          type=ref,event=branch
          type=sha,prefix={{branch}}-
          type=semver,pattern={{version}}
    
    - name: Build and push Docker image
      uses: docker/build-push-action@v4
      with:
        context: .
        push: true
        tags: ${{ steps.meta.outputs.tags }}
        labels: ${{ steps.meta.outputs.labels }}
```

### Example 3: Multi-Framework Testing

**Test on .NET 6, 7, 8**:

```yaml
name: Multi-Version Testing

on:
  push:
    branches: [ main ]

jobs:
  test:
    runs-on: ubuntu-latest
    strategy:
      matrix:
        dotnet-version: ['6.0', '7.0', '8.0']
    name: Test on .NET ${{ matrix.dotnet-version }}
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET ${{ matrix.dotnet-version }}
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: ${{ matrix.dotnet-version }}
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Run tests
      run: dotnet test --no-build --logger "trx" --collect:"XPlat Code Coverage"
    
    - name: Upload coverage to Codecov
      uses: codecov/codecov-action@v3
      with:
        files: ./coverage.xml
        flags: unittests
        name: codecov-umbrella
```

### Example 4: Scheduled Workflow

**Run nightly builds**:

```yaml
name: Nightly Build

on:
  schedule:
    - cron: '0 2 * * *'  # 2 AM UTC daily
  workflow_dispatch:

jobs:
  nightly-test:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0'
    
    - run: dotnet restore
    
    - run: dotnet build
    
    - run: dotnet test --verbosity normal
    
    - name: Report results
      if: always()
      run: echo "Nightly build completed at $(date)"
```

---

## Hands-On Exercises

### Exercise 1: Create Your First Workflow

**Objective**: Set up a basic CI workflow for a .NET console application

**Steps**:

1. Create a GitHub repository
2. Clone locally: `git clone <repo-url>`
3. Create project structure:
   ```bash
   dotnet new sln -n MyProject
   dotnet new console -n MyApp -o src/MyApp
   dotnet sln add src/MyApp/MyApp.csproj
   ```

4. Create `.github/workflows/ci.yml`:
   ```yaml
   name: CI
   on: [push, pull_request]
   jobs:
     build:
       runs-on: ubuntu-latest
       steps:
         - uses: actions/checkout@v4
         - uses: actions/setup-dotnet@v4
           with:
             dotnet-version: '8.0'
         - run: dotnet restore
         - run: dotnet build
         - run: dotnet test
   ```

5. Push to GitHub
6. Check Actions tab to see workflow run

**Expected Output**: Green checkmark indicating successful build

---

### Exercise 2: Add Unit Tests and Coverage

**Objective**: Integrate testing and measure code coverage

**Steps**:

1. Create test project:
   ```bash
   dotnet new xunit -n MyApp.Tests -o tests/MyApp.Tests
   dotnet sln add tests/MyApp.Tests/MyApp.Tests.csproj
   dotnet add tests/MyApp.Tests reference src/MyApp/MyApp.csproj
   ```

2. Add coverage tool to test project `.csproj`:
   ```xml
   <ItemGroup>
     <PackageReference Include="coverlet.collector" Version="6.0.0">
       <PrivateAssets>all</PrivateAssets>
       <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
     </PackageReference>
   </ItemGroup>
   ```

3. Update workflow:
   ```yaml
   - name: Test with coverage
     run: dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
   
   - name: Upload coverage
     uses: codecov/codecov-action@v3
   ```

4. Commit and push

**Expected Outcome**: Coverage report in Actions and integrated with Codecov

---

### Exercise 3: Environment-Based Deployment

**Objective**: Deploy to different environments based on branch

**Steps**:

1. Create deployment workflow with environments:
   ```yaml
   name: Deploy
   on:
     push:
       branches: [main, staging]
   
   env:
     ENVIRONMENT: ${{ github.ref == 'refs/heads/main' && 'production' || 'staging' }}
   
   jobs:
     deploy:
       runs-on: ubuntu-latest
       environment: ${{ env.ENVIRONMENT }}
       
       steps:
         - uses: actions/checkout@v4
         - uses: actions/setup-dotnet@v4
           with:
             dotnet-version: '8.0'
         - run: dotnet publish -c Release -o ./publish
         - name: Deploy to ${{ env.ENVIRONMENT }}
           env:
             DEPLOY_KEY: ${{ secrets.DEPLOY_KEY }}
           run: |
             echo "Deploying to ${{ env.ENVIRONMENT }}"
             # Add deployment script here
   ```

2. Set up GitHub Environments with protection rules
3. Test with branch strategy

---

### Exercise 4: Notifications and Reporting

**Objective**: Get notified of build failures and generate reports

**Steps**:

1. Add Slack notification:
   ```yaml
   - name: Notify Slack on failure
     if: failure()
     uses: slackapi/slack-github-action@v1
     with:
       webhook-url: ${{ secrets.SLACK_WEBHOOK }}
       payload: |
         {
           "text": "Build failed in ${{ github.repository }}",
           "blocks": [
             {
               "type": "section",
               "text": {
                 "type": "mrkdwn",
                 "text": "*Build Failed* :x:\nRepo: ${{ github.repository }}\nBranch: ${{ github.ref_name }}"
               }
             }
           ]
         }
   ```

2. Add email notification for releases:
   ```yaml
   - name: Send email on release
     if: startsWith(github.ref, 'refs/tags/')
     uses: dawidd6/action-send-mail@v3
     with:
       server_address: ${{ secrets.MAIL_SERVER }}
       server_port: ${{ secrets.MAIL_PORT }}
       username: ${{ secrets.MAIL_USERNAME }}
       password: ${{ secrets.MAIL_PASSWORD }}
       subject: "Release ${{ github.ref_name }}"
       body: "New release published"
       to: team@example.com
   ```

---

## Selenium Test Automation with GitHub Actions

### Complete Selenium + GitHub Actions Setup

**Project Structure**:
```
selenium-automation/
├── .github/workflows/
│   ├── selenium-tests.yml
│   └── selenium-deploy.yml
├── src/
│   └── SeleniumTests/
│       ├── Tests/
│       │   ├── LoginTest.cs
│       │   ├── CheckoutTest.cs
│       │   └── BaseTest.cs
│       └── SeleniumTests.csproj
└── SeleniumTests.sln
```

### Example: Selenium Chrome Tests Workflow

```yaml
name: Selenium Chrome Tests

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]
  schedule:
    - cron: '0 2 * * *'  # Nightly tests

jobs:
  selenium-tests:
    runs-on: ubuntu-latest
    name: Run Selenium Tests
    
    services:
      chrome:
        image: selenium/standalone-chrome:latest
        ports:
          - 4444:4444
        options: >-
          --shm-size=2gb
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build test project
      run: dotnet build src/SeleniumTests/SeleniumTests.csproj --configuration Release
    
    - name: Run Selenium tests
      run: dotnet test src/SeleniumTests/SeleniumTests.csproj --configuration Release --logger "trx" -v normal
      env:
        SELENIUM_HUB_URL: http://localhost:4444
        TEST_BASE_URL: https://example.com
    
    - name: Upload test results
      if: always()
      uses: actions/upload-artifact@v3
      with:
        name: selenium-test-results
        path: '**/TestResults/*.trx'
    
    - name: Publish test report
      if: always()
      uses: dorny/test-reporter@v1
      with:
        name: Selenium Test Results
        path: '**/TestResults/*.trx'
        reporter: 'dotnet trx'
```

### Selenium Test Base Class

```csharp
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using System;

public abstract class BaseSeleniumTest : IDisposable
{
    protected IWebDriver Driver;
    protected string BaseUrl = Environment.GetEnvironmentVariable("TEST_BASE_URL") ?? "https://example.com";

    public virtual void Setup()
    {
        string hubUrl = Environment.GetEnvironmentVariable("SELENIUM_HUB_URL") ?? "http://localhost:4444";
        
        var options = new ChromeOptions();
        options.AddArgument("--headless");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--disable-blink-features=AutomationControlled");
        
        Driver = new RemoteWebDriver(new Uri(hubUrl), options);
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
    }

    public virtual void Cleanup()
    {
        Driver?.Quit();
    }

    public void Dispose()
    {
        Cleanup();
    }
}
```

### Selenium Test Example

```csharp
using Xunit;
using OpenQA.Selenium;

public class LoginTests : BaseSeleniumTest
{
    [Fact]
    public void TestValidLogin()
    {
        Setup();
        try
        {
            // Arrange
            Driver.Navigate().GoToUrl(BaseUrl + "/login");
            
            // Act
            var emailField = Driver.FindElement(By.Id("email"));
            var passwordField = Driver.FindElement(By.Id("password"));
            var loginButton = Driver.FindElement(By.Id("login-button"));
            
            emailField.SendKeys("user@example.com");
            passwordField.SendKeys("password123");
            loginButton.Click();
            
            // Assert
            System.Threading.Thread.Sleep(2000);
            Assert.True(Driver.Url.Contains("/dashboard"));
        }
        finally
        {
            Cleanup();
        }
    }

    [Fact]
    public void TestInvalidLogin()
    {
        Setup();
        try
        {
            Driver.Navigate().GoToUrl(BaseUrl + "/login");
            
            var emailField = Driver.FindElement(By.Id("email"));
            var passwordField = Driver.FindElement(By.Id("password"));
            var loginButton = Driver.FindElement(By.Id("login-button"));
            
            emailField.SendKeys("invalid@example.com");
            passwordField.SendKeys("wrongpassword");
            loginButton.Click();
            
            System.Threading.Thread.Sleep(1000);
            var errorMessage = Driver.FindElement(By.ClassName("error-message"));
            Assert.NotNull(errorMessage);
        }
        finally
        {
            Cleanup();
        }
    }
}
```

---

## Troubleshooting & Best Practices

### Common Issues and Solutions

#### Issue 1: Tests Pass Locally but Fail in GitHub Actions

**Problem**: Different environment (Linux vs Windows), missing dependencies

**Solution**:
```yaml
- name: Install system dependencies
  run: |
    sudo apt-get update
    sudo apt-get install -y chromium-browser
    sudo apt-get install -y xvfb

- name: Test with display
  run: |
    export DISPLAY=:99
    xvfb-run dotnet test
```

#### Issue 2: Timeouts in Selenium Tests

**Problem**: Tests timing out on slow runners

**Solution**:
```csharp
// Increase timeouts for CI environment
int timeout = Environment.GetEnvironmentVariable("CI") != null ? 30 : 10;
Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(timeout);

// Add explicit waits
var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeout));
wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("element")));
```

#### Issue 3: Secrets Not Available

**Problem**: GitHub secrets not accessible in workflow

**Solution**:
```yaml
- name: Use secret
  env:
    MY_SECRET: ${{ secrets.MY_SECRET }}
  run: |
    echo "Secret is set: ${MY_SECRET:+yes}"
    # Never echo the actual secret value
```

#### Issue 4: Artifacts Not Uploading

**Problem**: Path doesn't exist or permission denied

**Solution**:
```yaml
- name: Create results directory
  run: mkdir -p TestResults

- name: Run tests with results output
  run: dotnet test --logger "trx;LogFileName=TestResults/results.trx"

- name: Upload artifacts
  if: always()
  uses: actions/upload-artifact@v3
  with:
    name: test-results
    path: TestResults/
    if-no-files-found: warn
```

### Best Practices

#### 1. Use Environment Variables and Secrets

```yaml
env:
  DOTNET_VERSION: '8.0'
  CONFIGURATION: Release

jobs:
  build:
    steps:
      - run: dotnet build --configuration ${{ env.CONFIGURATION }}
      - run: echo "DB_CONNECTION=${{ secrets.DB_CONNECTION }}"
```

#### 2. Cache Dependencies

```yaml
- name: Setup .NET
  uses: actions/setup-dotnet@v4
  with:
    dotnet-version: '8.0'

- name: Cache NuGet packages
  uses: actions/cache@v3
  with:
    path: ~/.nuget/packages
    key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
    restore-keys: |
      ${{ runner.os }}-nuget-

- name: Restore
  run: dotnet restore
```

#### 3. Use Matrix Builds for Multiple Configurations

```yaml
strategy:
  matrix:
    os: [ubuntu-latest, windows-latest]
    dotnet-version: ['6.0', '7.0', '8.0']

runs-on: ${{ matrix.os }}

steps:
  - uses: actions/setup-dotnet@v4
    with:
      dotnet-version: ${{ matrix.dotnet-version }}
```

#### 4. Fail Fast on Critical Errors

```yaml
- name: Code analysis
  run: dotnet format --verify-no-changes
  continue-on-error: false  # Fail workflow if code isn't formatted

- name: Non-critical step
  run: dotnet tool restore
  continue-on-error: true  # Don't fail workflow if optional step fails
```

#### 5. Use Concurrency to Cancel Redundant Runs

```yaml
concurrency:
  group: ${{ github.workflow }}-${{ github.ref }}
  cancel-in-progress: true
```

#### 6. Document Your Workflow

```yaml
# Build and test .NET application on every push and PR
# Publishes test results and code coverage reports
name: CI Pipeline

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

# ... rest of workflow
```

#### 7. Use Conditional Steps

```yaml
- name: Build
  run: dotnet build

- name: Upload coverage (only on main)
  if: github.ref == 'refs/heads/main'
  run: dotnet test --collect:"XPlat Code Coverage"

- name: Notify on failure
  if: failure()
  run: echo "Build failed!"

- name: Always cleanup
  if: always()
  run: dotnet clean
```

---

## Security Best Practices

### Protecting Secrets

```yaml
# ✓ GOOD: Use secrets for sensitive data
- name: Deploy
  env:
    API_KEY: ${{ secrets.API_KEY }}
  run: deploy.sh

# ✗ BAD: Never hardcode secrets
- name: Deploy
  run: deploy.sh --api-key "hardcoded-secret"

# ✓ GOOD: Mask secrets in output
- name: Use secret
  run: |
    echo "::add-mask::${{ secrets.API_KEY }}"
    echo ${{ secrets.API_KEY }}  # Output will be masked
```

### Workflow Permissions

```yaml
permissions:
  contents: read           # Can read repository
  packages: write          # Can push to package registry
  id-token: write          # Can request OIDC token

jobs:
  deploy:
    permissions:
      contents: read       # This job only needs read access
```

### Reviewing Actions

- Only use official actions from trusted publishers
- Pin action versions: `uses: actions/setup-dotnet@v4` (not `@latest`)
- Review third-party action code before using
- Use GitHub-provided actions when possible

---

## Performance Optimization

### Parallel Job Execution

```yaml
jobs:
  test-unit:
    runs-on: ubuntu-latest
    steps:
      - run: dotnet test tests/Unit/ --no-build

  test-integration:
    runs-on: ubuntu-latest
    steps:
      - run: dotnet test tests/Integration/ --no-build

  test-selenium:
    runs-on: ubuntu-latest
    steps:
      - run: dotnet test tests/Selenium/ --no-build

  # All three run simultaneously
```

### Conditional Job Dependencies

```yaml
jobs:
  build:
    runs-on: ubuntu-latest
    # ...

  fast-tests:
    needs: build
    runs-on: ubuntu-latest
    # ...

  slow-tests:
    needs: build
    runs-on: ubuntu-latest
    # ...

  deploy:
    needs: [fast-tests, slow-tests]  # Only run after both complete
    runs-on: ubuntu-latest
    # ...
```

### Skip Workflows for Documentation-Only Changes

```yaml
on:
  push:
    branches: [main]
    paths-ignore:
      - '**.md'
      - 'docs/**'
      - '.gitignore'
```

---

## Monitoring and Observability

### GitHub Actions Dashboard

- Navigate to Actions tab in your repository
- See workflow history, run times, status
- Click on runs to see detailed logs
- Download artifacts directly

### Badges

Add status badge to README:

```markdown
[![Build Status](https://github.com/your-org/your-repo/actions/workflows/ci.yml/badge.svg)](https://github.com/your-org/your-repo/actions)
```

### Workflow Insights

- Run duration trends
- Success/failure rates
- Most common failures
- Job duration analysis

---

## Advanced Scenarios

### Release Automation

```yaml
name: Release

on:
  push:
    tags:
      - 'v*'

jobs:
  build:
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0'
      
      - run: dotnet publish -c Release -o ./publish
      
      - name: Create release
        uses: actions/create-release@v1
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
        with:
          tag_name: ${{ github.ref }}
          release_name: Release ${{ github.ref }}
          body: |
            Changes in this Release
            - Feature 1
            - Feature 2
          draft: false
          prerelease: false
```

### Database Testing

```yaml
services:
  sql-server:
    image: mcr.microsoft.com/mssql/server:2019-latest
    env:
      ACCEPT_EULA: Y
      SA_PASSWORD: MyP@ssw0rd
    options: >-
      --health-cmd="/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P MyP@ssw0rd -Q 'SELECT 1' || exit 1"
      --health-interval=10s
      --health-timeout=5s
      --health-retries=5
    ports:
      - 1433:1433

steps:
  - run: dotnet test
    env:
      CONNECTION_STRING: "Server=localhost;Database=TestDb;User Id=sa;Password=MyP@ssw0rd"
```

---

End of GitHub Actions with .NET guide. Ready to practice!
