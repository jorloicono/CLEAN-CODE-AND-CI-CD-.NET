# Complete Guide to Factory Method and Abstract Factory Patterns in .NET

---

## Introduction

The Factory Method and Abstract Factory patterns are **creational design patterns** that provide flexible ways to instantiate objects without directly coupling client code to concrete classes. These patterns are fundamental for building maintainable, scalable, and testable .NET applications.

### Why Use Factory Patterns?

- **Decoupling**: Client code doesn't depend on concrete implementations
- **Flexibility**: Easy to introduce new types without modifying existing code
- **Testability**: Simplifies unit testing through dependency injection and mocking
- **Maintainability**: Centralized object creation logic
- **Open/Closed Principle**: Open for extension, closed for modification

---

## Factory Method Pattern

### Definition

The Factory Method pattern defines an interface for creating objects, but lets **subclasses decide which class to instantiate**. It encapsulates object creation in a method that can be overridden by subclasses.

### Components

1. **Product Interface/Abstract Class**: Defines the interface for objects the factory creates
2. **Concrete Products**: Specific implementations of the Product interface
3. **Creator (Abstract/Interface)**: Declares the abstract factory method returning a Product
4. **Concrete Creators**: Implement the factory method to return specific Concrete Products
5. **Client**: Uses the Creator interface to get Products without knowing concrete types

### Structure Diagram

```
Creator (abstract)
├── +FactoryMethod(): Product
└── +UseProduct(): void

ConcreteCreatorA : Creator         ConcreteCreatorB : Creator
├── +FactoryMethod(): Product      ├── +FactoryMethod(): Product
│   return ConcreteProductA()       │   return ConcreteProductB()
└── inherits UseProduct()           └── inherits UseProduct()

Product (interface)                ConcreteProductA : Product
├── +Execute(): void                └── +Execute(): void

                                  ConcreteProductB : Product
                                  └── +Execute(): void
```

### When to Use Factory Method

- You don't know the exact types needed until runtime
- You want subclasses to decide which product type to create
- You need a single product at a time
- You're building a plugin system where types are unknown beforehand
- Object creation requires specific initialization logic

### Code Example 1: Basic Factory Method

```csharp
// Product Interface
public interface ITransport
{
    void Deliver(string cargo);
}

// Concrete Products
public class Truck : ITransport
{
    public void Deliver(string cargo)
    {
        Console.WriteLine($"Delivering {cargo} by truck");
    }
}

public class Ship : ITransport
{
    public void Deliver(string cargo)
    {
        Console.WriteLine($"Delivering {cargo} by ship");
    }
}

// Creator (Abstract Class)
public abstract class Logistics
{
    // Factory Method
    public abstract ITransport CreateTransport();
    
    // Business Logic
    public void PlanDelivery(string cargo)
    {
        ITransport transport = CreateTransport();
        transport.Deliver(cargo);
    }
}

// Concrete Creators
public class RoadLogistics : Logistics
{
    public override ITransport CreateTransport()
    {
        Console.WriteLine("RoadLogistics: Creating Truck");
        return new Truck();
    }
}

public class SeaLogistics : Logistics
{
    public override ITransport CreateTransport()
    {
        Console.WriteLine("SeaLogistics: Creating Ship");
        return new Ship();
    }
}

// Client Code
public class Program
{
    public static void Main()
    {
        // Client doesn't know about Truck or Ship directly
        Logistics roadLogistics = new RoadLogistics();
        roadLogistics.PlanDelivery("Books"); 
        // Output: RoadLogistics: Creating Truck
        //         Delivering Books by truck
        
        Logistics seaLogistics = new SeaLogistics();
        seaLogistics.PlanDelivery("Electronics");
        // Output: SeaLogistics: Creating Ship
        //         Delivering Electronics by ship
    }
}
```

### Code Example 2: Browser Factory for QA Automation

```csharp
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Edge;

// Product Interface
public interface IBrowser
{
    void Navigate(string url);
    IWebElement FindElement(By locator);
    void Close();
}

// Concrete Products
public class ChromeBrowser : IBrowser
{
    private readonly IWebDriver _driver;

    public ChromeBrowser()
    {
        var options = new ChromeOptions();
        options.AddArgument("--start-maximized");
        _driver = new ChromeDriver(options);
    }

    public void Navigate(string url)
    {
        _driver.Navigate().GoToUrl(url);
        Console.WriteLine($"Chrome: Navigating to {url}");
    }

    public IWebElement FindElement(By locator)
    {
        return _driver.FindElement(locator);
    }

    public void Close()
    {
        _driver.Quit();
    }
}

public class FirefoxBrowser : IBrowser
{
    private readonly IWebDriver _driver;

    public FirefoxBrowser()
    {
        var options = new FirefoxOptions();
        _driver = new FirefoxDriver(options);
    }

    public void Navigate(string url)
    {
        _driver.Navigate().GoToUrl(url);
        Console.WriteLine($"Firefox: Navigating to {url}");
    }

    public IWebElement FindElement(By locator)
    {
        return _driver.FindElement(locator);
    }

    public void Close()
    {
        _driver.Quit();
    }
}

public class EdgeBrowser : IBrowser
{
    private readonly IWebDriver _driver;

    public EdgeBrowser()
    {
        _driver = new EdgeDriver();
    }

    public void Navigate(string url)
    {
        _driver.Navigate().GoToUrl(url);
        Console.WriteLine($"Edge: Navigating to {url}");
    }

    public IWebElement FindElement(By locator)
    {
        return _driver.FindElement(locator);
    }

    public void Close()
    {
        _driver.Quit();
    }
}

// Creator (Abstract)
public abstract class BrowserFactory
{
    public abstract IBrowser CreateBrowser();

    public void RunTest(string testName, string url)
    {
        Console.WriteLine($"--- Running Test: {testName} ---");
        IBrowser browser = CreateBrowser();
        browser.Navigate(url);
        // Simulate test actions
        browser.Close();
    }
}

// Concrete Creators
public class ChromeFactory : BrowserFactory
{
    public override IBrowser CreateBrowser()
    {
        return new ChromeBrowser();
    }
}

public class FirefoxFactory : BrowserFactory
{
    public override IBrowser CreateBrowser()
    {
        return new FirefoxBrowser();
    }
}

public class EdgeFactory : BrowserFactory
{
    public override IBrowser CreateBrowser()
    {
        return new EdgeBrowser();
    }
}

// Client Code
public class TestAutomationRunner
{
    public static void Main()
    {
        BrowserFactory[] factories = new BrowserFactory[]
        {
            new ChromeFactory(),
            new FirefoxFactory(),
            new EdgeFactory()
        };

        foreach (var factory in factories)
        {
            factory.RunTest("Login Test", "https://example.com/login");
        }
    }
}
```

### Code Example 3: Using Enum-Based Factory Method

```csharp
public enum BrowserType
{
    Chrome,
    Firefox,
    Edge
}

public class SimpleBrowserFactory
{
    public static IBrowser CreateBrowser(BrowserType browserType)
    {
        return browserType switch
        {
            BrowserType.Chrome => new ChromeBrowser(),
            BrowserType.Firefox => new FirefoxBrowser(),
            BrowserType.Edge => new EdgeBrowser(),
            _ => throw new ArgumentException("Unknown browser type", nameof(browserType))
        };
    }
}

// Usage
IBrowser browser = SimpleBrowserFactory.CreateBrowser(BrowserType.Chrome);
browser.Navigate("https://example.com");
```

---

## Abstract Factory Pattern

### Definition

The Abstract Factory pattern provides an interface for creating **families of related or dependent objects** without specifying their concrete classes. It's a **factory of factories** that ensures related products are used together.

### Components

1. **Abstract Products**: Interfaces for different product families
2. **Concrete Products**: Specific implementations grouped by variant
3. **Abstract Factory**: Interface declaring methods to create abstract products
4. **Concrete Factories**: Implement creation methods for specific product variants
5. **Client**: Works with products through abstract interfaces

### Structure Diagram

```
AbstractFactory (interface)
├── +CreateProductA(): IAbstractProductA
└── +CreateProductB(): IAbstractProductB

ConcreteFactory1 : AbstractFactory      ConcreteFactory2 : AbstractFactory
├── +CreateProductA(): ProductA1        ├── +CreateProductA(): ProductA2
└── +CreateProductB(): ProductB1        └── +CreateProductB(): ProductB2

IAbstractProductA                       IAbstractProductB
├── ProductA1 : IAbstractProductA       ├── ProductB1 : IAbstractProductB
└── ProductA2 : IAbstractProductA       └── ProductB2 : IAbstractProductB
```

### When to Use Abstract Factory

- You need to create **families of related products**
- Products must be used together (consistency is important)
- You want to provide a library of products revealing only interfaces
- You need to switch product families (e.g., themes, databases)
- Complex systems with multiple interdependent objects
- UI frameworks supporting multiple platforms (Windows, macOS)

### Code Example 1: Furniture Shop Application

```csharp
// Abstract Products
public interface IChair
{
    void SitOn();
    string GetStyle();
}

public interface ISofa
{
    void LieOn();
    string GetStyle();
}

public interface ICoffeeTable
{
    void PlaceOn();
    string GetStyle();
}

// Modern Family - Concrete Products
public class ModernChair : IChair
{
    public void SitOn() => Console.WriteLine("Sitting on modern chair");
    public string GetStyle() => "Modern";
}

public class ModernSofa : ISofa
{
    public void LieOn() => Console.WriteLine("Lying on modern sofa");
    public string GetStyle() => "Modern";
}

public class ModernCoffeeTable : ICoffeeTable
{
    public void PlaceOn() => Console.WriteLine("Placing on modern table");
    public string GetStyle() => "Modern";
}

// Victorian Family - Concrete Products
public class VictorianChair : IChair
{
    public void SitOn() => Console.WriteLine("Sitting on Victorian chair");
    public string GetStyle() => "Victorian";
}

public class VictorianSofa : ISofa
{
    public void LieOn() => Console.WriteLine("Lying on Victorian sofa");
    public string GetStyle() => "Victorian";
}

public class VictorianCoffeeTable : ICoffeeTable
{
    public void PlaceOn() => Console.WriteLine("Placing on Victorian table");
    public string GetStyle() => "Victorian";
}

// Abstract Factory
public interface IFurnitureFactory
{
    IChair CreateChair();
    ISofa CreateSofa();
    ICoffeeTable CreateCoffeeTable();
}

// Concrete Factories
public class ModernFurnitureFactory : IFurnitureFactory
{
    public IChair CreateChair() => new ModernChair();
    public ISofa CreateSofa() => new ModernSofa();
    public ICoffeeTable CreateCoffeeTable() => new ModernCoffeeTable();
}

public class VictorianFurnitureFactory : IFurnitureFactory
{
    public IChair CreateChair() => new VictorianChair();
    public ISofa CreateSofa() => new VictorianSofa();
    public ICoffeeTable CreateCoffeeTable() => new VictorianCoffeeTable();
}

// Client Code
public class FurnitureShop
{
    private readonly IChair _chair;
    private readonly ISofa _sofa;
    private readonly ICoffeeTable _table;

    public FurnitureShop(IFurnitureFactory factory)
    {
        _chair = factory.CreateChair();
        _sofa = factory.CreateSofa();
        _table = factory.CreateCoffeeTable();
    }

    public void ShowroomDemo()
    {
        Console.WriteLine("Furniture styles match!");
        _chair.SitOn();
        _sofa.LieOn();
        _table.PlaceOn();
        Console.WriteLine($"Style: {_chair.GetStyle()}");
    }
}

// Usage
IFurnitureFactory modernFactory = new ModernFurnitureFactory();
FurnitureShop modernShop = new FurnitureShop(modernFactory);
modernShop.ShowroomDemo();
// Output: Furniture styles match!
//         Sitting on modern chair
//         Lying on modern sofa
//         Placing on modern table
//         Style: Modern
```

### Code Example 2: Database Connection Factory

```csharp
// Abstract Products
public interface IDBConnection
{
    void Connect();
    void Disconnect();
}

public interface IDBCommand
{
    void Execute(string query);
}

// SQL Server Family
public class SqlServerConnection : IDBConnection
{
    public void Connect() => Console.WriteLine("Connected to SQL Server");
    public void Disconnect() => Console.WriteLine("Disconnected from SQL Server");
}

public class SqlServerCommand : IDBCommand
{
    private readonly IDBConnection _connection;

    public SqlServerCommand(IDBConnection connection)
    {
        _connection = connection;
    }

    public void Execute(string query)
    {
        _connection.Connect();
        Console.WriteLine($"Executing SQL Server query: {query}");
        _connection.Disconnect();
    }
}

// PostgreSQL Family
public class PostgreSqlConnection : IDBConnection
{
    public void Connect() => Console.WriteLine("Connected to PostgreSQL");
    public void Disconnect() => Console.WriteLine("Disconnected from PostgreSQL");
}

public class PostgreSqlCommand : IDBCommand
{
    private readonly IDBConnection _connection;

    public PostgreSqlCommand(IDBConnection connection)
    {
        _connection = connection;
    }

    public void Execute(string query)
    {
        _connection.Connect();
        Console.WriteLine($"Executing PostgreSQL query: {query}");
        _connection.Disconnect();
    }
}

// Abstract Factory
public interface IDBFactory
{
    IDBConnection CreateConnection();
    IDBCommand CreateCommand(IDBConnection connection);
}

// Concrete Factories
public class SqlServerFactory : IDBFactory
{
    public IDBConnection CreateConnection() => new SqlServerConnection();
    public IDBCommand CreateCommand(IDBConnection connection) => new SqlServerCommand(connection);
}

public class PostgreSqlFactory : IDBFactory
{
    public IDBConnection CreateConnection() => new PostgreSqlConnection();
    public IDBCommand CreateCommand(IDBConnection connection) => new PostgreSqlCommand(connection);
}

// Client Code
public class DataRepository
{
    private readonly IDBFactory _factory;

    public DataRepository(IDBFactory factory)
    {
        _factory = factory;
    }

    public void FetchData(string query)
    {
        var connection = _factory.CreateConnection();
        var command = _factory.CreateCommand(connection);
        command.Execute(query);
    }
}

// Usage
IDBFactory sqlFactory = new SqlServerFactory();
DataRepository sqlRepo = new DataRepository(sqlFactory);
sqlRepo.FetchData("SELECT * FROM Users");

IDBFactory pgFactory = new PostgreSqlFactory();
DataRepository pgRepo = new DataRepository(pgFactory);
pgRepo.FetchData("SELECT * FROM Users");
```

---

## Practical Implementation

### Using Factory Pattern with Dependency Injection

Modern .NET applications use dependency injection. Here's how to integrate factories:

```csharp
using Microsoft.Extensions.DependencyInjection;

public interface IBrowserFactory
{
    IBrowser CreateBrowser(string browserType);
}

public class DIPBrowserFactory : IBrowserFactory
{
    private readonly IServiceProvider _serviceProvider;

    public DIPBrowserFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IBrowser CreateBrowser(string browserType)
    {
        return browserType.ToLower() switch
        {
            "chrome" => _serviceProvider.GetRequiredService<ChromeBrowser>(),
            "firefox" => _serviceProvider.GetRequiredService<FirefoxBrowser>(),
            "edge" => _serviceProvider.GetRequiredService<EdgeBrowser>(),
            _ => throw new ArgumentException($"Unknown browser: {browserType}")
        };
    }
}

// Startup Configuration
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Register browser implementations
        services.AddTransient<ChromeBrowser>();
        services.AddTransient<FirefoxBrowser>();
        services.AddTransient<EdgeBrowser>();
        
        // Register factory
        services.AddSingleton<IBrowserFactory, DIPBrowserFactory>();
    }
}

// Test Class Usage
public class LoginTest
{
    private readonly IBrowserFactory _factory;
    private IBrowser _browser;

    public LoginTest(IBrowserFactory factory)
    {
        _factory = factory;
    }

    public void TestLogin()
    {
        _browser = _factory.CreateBrowser("chrome");
        _browser.Navigate("https://example.com/login");
        // Perform test
        _browser.Close();
    }
}
```

### Abstract Factory with Dependency Injection

```csharp
public interface IThemeFactory
{
    IButton CreateButton();
    ILabel CreateLabel();
}

public class ModernThemeFactory : IThemeFactory
{
    public IButton CreateButton() => new ModernButton();
    public ILabel CreateLabel() => new ModernLabel();
}

public class DarkThemeFactory : IThemeFactory
{
    public IButton CreateButton() => new DarkButton();
    public ILabel CreateLabel() => new DarkLabel();
}

// DI Registration
public void ConfigureServices(IServiceCollection services)
{
    // Determine theme from config
    var isDarkMode = _configuration.GetValue<bool>("UI:DarkMode");
    
    services.AddSingleton<IThemeFactory>(
        isDarkMode 
            ? new DarkThemeFactory() 
            : new ModernThemeFactory()
    );
}

// Usage in application
public class UIRenderer
{
    private readonly IThemeFactory _factory;

    public UIRenderer(IThemeFactory factory)
    {
        _factory = factory;
    }

    public void RenderUI()
    {
        var button = _factory.CreateButton();
        var label = _factory.CreateLabel();
        
        button.Render();
        label.Render();
    }
}
```

---

## Practice Exercises

### Exercise 1: Browser Factory Enhancement

**Difficulty**: Beginner

Create a browser factory that supports the following browsers: Chrome, Firefox, Edge, and Safari. Each browser should:
- Have different timeout configurations
- Support headless mode option
- Log creation/closing events

**Expected Code Structure**:
```csharp
public interface IBrowser
{
    void Navigate(string url);
    void Close();
    void SetTimeout(int seconds);
}

public abstract class BrowserFactory
{
    public abstract IBrowser CreateBrowser();
    // Add headless mode support
}
```

**Questions**:
1. How would you extend this to support a parameterized factory (headless mode)?
2. What happens if you add a new browser type? How many files must you change?
3. How would you unit test the factory without creating actual browsers?

---

### Exercise 2: Multi-Database Support

**Difficulty**: Intermediate

Implement an abstract factory that supports multiple databases:
- SQL Server
- PostgreSQL
- MySQL

Each database family must include:
- Connection manager
- Query executor
- Transaction handler

All three must work together consistently.

**Implementation Steps**:
1. Define abstract products (IConnection, IQueryExecutor, ITransactionHandler)
2. Implement concrete products for each database
3. Create abstract factory interface
4. Implement concrete factories
5. Test factory ensures products from the same family work together

**Sample Test Case**:
```csharp
[Test]
public void TestConsistentProductFamily()
{
    IDBFactory factory = new SqlServerFactory();
    var connection = factory.CreateConnection();
    var executor = factory.CreateQueryExecutor();
    var transaction = factory.CreateTransactionHandler();
    
    // All should be SQL Server implementations
    Assert.IsInstanceOf<SqlServerConnection>(connection);
    Assert.IsInstanceOf<SqlServerQueryExecutor>(executor);
    Assert.IsInstanceOf<SqlServerTransactionHandler>(transaction);
}
```

---

### Exercise 3: Test Report Generator

**Difficulty**: Intermediate-Advanced

Create a system using both Factory Method and Abstract Factory:
- **Factory Method**: Choose report format (HTML, PDF, JSON)
- **Abstract Factory**: Choose styling theme (Corporate, Minimal, Colorful)

Requirements:
- Each report format must support all themes
- Report data structure is consistent
- Themes affect styling/formatting only

**Structure**:
```
Report Types (Factory Method)
├── HTMLReport
├── PDFReport
└── JSONReport

Themes (Abstract Factory)
├── CorporateTheme
├── MinimalTheme
└── ColorfulTheme

Each Report × Theme combination must work together
```

**Bonus Challenge**: 
- Add template selection (predefined layouts)
- Support custom CSS for themes
- Implement report builder pattern for complex reports

---

### Exercise 4: UI Component Factory with Configuration

**Difficulty**: Advanced

Build a factory system for UI components that:
1. Reads configuration from appsettings.json
2. Creates appropriate component families based on config
3. Supports hot-swapping themes without recompilation
4. Works with dependency injection

**Configuration Example**:
```json
{
  "UI": {
    "Theme": "Modern",
    "Components": "MaterialDesign",
    "DarkMode": false
  }
}
```

**Implementation Requirements**:
- Use IOptions<UIConfig>
- Implement IConfigureOptions for validation
- Support factory registration in startup
- Create components through injected factory

---

### Exercise 5: Logging Factory System

**Difficulty**: Intermediate

Create a logging factory that produces different logger implementations:
- Console logger
- File logger
- Database logger
- Cloud logger (e.g., Azure)

Requirements:
- Support multiple loggers simultaneously
- Each logger has specific configuration
- Factory manages logger lifecycle
- Support log levels (Debug, Info, Warning, Error)

**Bonus Features**:
- Composite logger (logs to multiple destinations)
- Logger factory pool management
- Async logging support
- Context-aware logging

---

## Comparison and Decision Guide

### Factory Method vs Abstract Factory

| Aspect | Factory Method | Abstract Factory |
|--------|---|---|
| **Purpose** | Create single product type | Create families of related products |
| **Number of Products** | One | Multiple (usually 2+) |
| **Inheritance** | Uses inheritance (abstract method) | Uses composition/interface |
| **Subclass Needed** | Yes, one per product | Yes, one per family |
| **Coupling** | Tight to single product | Loose across product families |
| **Complexity** | Lower | Higher |
| **Extensibility** | Add subclass for new type | Add concrete factory for new family |
| **Testing** | Mock single creator | Mock factory interface |

### Quick Decision Tree

```
Do you need to create multiple RELATED products together?
├─ YES → Use ABSTRACT FACTORY
│        (UI elements, database connections, theme components)
└─ NO → Do you need to vary which class is instantiated?
        ├─ YES → Use FACTORY METHOD
        │        (Transport, browsers, report formats)
        └─ NO → Direct instantiation or simple factory method
```

### Real-World Scenarios

**Use Factory Method When**:
- Creating different report formats (HTML, PDF, Excel)
- Supporting multiple browsers in test automation
- Building plugin systems
- Document parsers (XML, JSON, YAML)
- Payment processors (PayPal, Stripe, Square)

**Use Abstract Factory When**:
- Building cross-platform UI (Windows, macOS, Linux)
- Supporting multiple databases with full abstraction
- Theme/style systems (light/dark mode with all components)
- Restaurant franchises with location-specific menus
- Device families (printers, monitors with manufacturers)

---

## Answer Key for Exercises

### Exercise 1 Answers

**Question 1**: Extend factory method with parameter:
```csharp
public abstract class BrowserFactory
{
    public abstract IBrowser CreateBrowser(BrowserOptions options);
}

public class ChromeFactory : BrowserFactory
{
    public override IBrowser CreateBrowser(BrowserOptions options)
    {
        var opts = new ChromeOptions();
        if (options.Headless) opts.AddArgument("--headless");
        return new ChromeBrowser(opts);
    }
}
```

**Question 2**: Must change:
- Create new concrete browser class
- Create new concrete factory class
- Update factory selector logic
Total: 3 changes (good design!)

**Question 3**: Create mock/fake browser:
```csharp
public class MockBrowser : IBrowser
{
    public void Navigate(string url) { }
    public void Close() { }
}

// Unit test
[Test]
public void TestFactoryCreatesChromeBrowser()
{
    var factory = new ChromeFactory();
    var browser = factory.CreateBrowser(new BrowserOptions());
    Assert.IsInstanceOf<ChromeBrowser>(browser);
}
```

---

## Advanced Topics

### Async Factory Methods

```csharp
public interface IAsyncBrowserFactory
{
    Task<IBrowser> CreateBrowserAsync(string browserType);
}

public class AsyncBrowserFactory : IAsyncBrowserFactory
{
    public async Task<IBrowser> CreateBrowserAsync(string browserType)
    {
        var browser = browserType switch
        {
            "chrome" => new ChromeBrowser(),
            "firefox" => new FirefoxBrowser(),
            _ => throw new ArgumentException()
        };
        
        await browser.InitializeAsync();
        return browser;
    }
}
```

### Object Pooling with Factory

```csharp
public class PooledBrowserFactory : IBrowserFactory
{
    private readonly Queue<IBrowser> _availableBrowsers = new();
    private readonly HashSet<IBrowser> _activeBrowsers = new();
    private readonly int _maxPoolSize;

    public IBrowser GetBrowser()
    {
        if (_availableBrowsers.Count > 0)
            return _availableBrowsers.Dequeue();
        
        if (_activeBrowsers.Count < _maxPoolSize)
        {
            var browser = new ChromeBrowser();
            _activeBrowsers.Add(browser);
            return browser;
        }
        
        throw new InvalidOperationException("No browsers available");
    }

    public void ReturnBrowser(IBrowser browser)
    {
        _availableBrowsers.Enqueue(browser);
    }
}
```

### Factory with Generic Type Support

```csharp
public interface IGenericFactory<T> where T : class
{
    T Create(string key);
}

public class GenericFactory<T> : IGenericFactory<T> where T : class
{
    private readonly Dictionary<string, Func<T>> _creators;

    public void Register(string key, Func<T> creator)
    {
        _creators[key] = creator;
    }

    public T Create(string key)
    {
        if (_creators.TryGetValue(key, out var creator))
            return creator();
        
        throw new KeyNotFoundException($"No creator for {key}");
    }
}
```

---

## Best Practices

1. **Keep Factory Simple**: Avoid complex logic in factories
2. **Use Interfaces**: Always program to interfaces, not implementations
3. **Consistent Naming**: Factory, Creator, Provider conventions
4. **Document Product Families**: Make relationships clear
5. **Test Factory Creation**: Unit test the factory logic
6. **Use DI**: Integrate with dependency injection frameworks
7. **Version Products**: Consider API versioning in factories
8. **Handle Errors**: Provide meaningful exceptions

---

## Resources and Further Learning

- **Official Documentation**: Microsoft Learn - Design Patterns
- **SOLID Principles**: Understanding SOLID for factory design
- **Refactoring Guru**: https://refactoring.guru/design-patterns
- **C# Language Features**: Pattern matching for modern factories
- **Performance**: Consider lazy initialization and caching strategies
