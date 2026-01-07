using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;

// Ex 9: helper is renamed and turned into an instance class (ConfigurationService)
// This promotes dependency injection and improves testability over static globals.
public class ConfigurationService
{
    // Ex 5: Extract magic strings into constants to prevent duplication and typos.
    public const string ProductCode = "shield";
    public const string AdminRole = "Admin";
    public const string GetUsersEndpoint = "https://api.byteapp.dev/getusers";
    
    // Ex 9: Config is now an instance method, retrieving configuration.
    public Dictionary<string, string> GetConfig()
    {
        return new Dictionary<string, string>()
        {
            ["Product"] = ProductCode,
            ["Role"] = AdminRole,
            ["UsersApiUrl"] = GetUsersEndpoint,
        };
    }
}

// Ex 4: EngineData is an independent class used for composition.
public class EngineData
{
    // Ex 1: Renamed properties (d -> Displacement, h -> Horsepower)
    public string Displacement { get; }
    public double Horsepower { get; }

    public EngineData(string displacement, double horsepower)
    {
        Displacement = displacement;
        Horsepower = horsepower;
    }
}

// Ex 3: Car is encapsulated with read-only properties.
public class Car
{
    // Ex 1 & 3: Renamed to Model, Brand, Color. Read-only properties (no public setters).
    public string Model { get; }
    public string Brand { get; }
    public string Color { get; }
    
    // Ex 4: Property for EngineData for Composition ("Car has an EngineData")
    public EngineData EngineData { get; private set; }

    public Car(string model, string brand, string color)
    {
        Model = model;
        Brand = brand;
        Color = color;
    }
    
    // Ex 4: Method to set engine data
    public void SetEngine(EngineData engineData)
    {
        // Add optional validation here, e.g., if (EngineData != null) throw ...
        EngineData = engineData;
    }
}

// Ex 2 & 3: BankAccount is properly encapsulated with explicit, descriptive methods.
public class BankAccount
{
    // Ex 3: balance is private to prevent unvalidated changes.
    private double _balance = 5000;

    // Ex 3: Read-only property for Balance (safe access)
    public double Balance
    {
        get { return _balance; }
    }

    // Ex 2 & 3: Explicit, descriptive method replacing op(true, a). Includes validation.
    public void Deposit(double amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Deposit amount must be positive.");
        }
        _balance += amount;
    }

    // Ex 2 & 3: Explicit, descriptive method replacing op(false, a). Includes validation.
    public void Withdraw(double amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Withdrawal amount must be positive.");
        }
        if (_balance - amount < 0)
        {
            throw new InvalidOperationException("Insufficient funds.");
        }
        _balance -= amount;
    }
}

// Ex 7: Function name updated to reflect intent.
public class SlackNotification
{
    // Ex 1: Renamed private fields for clarity
    private string _toChannel;
    private string _messageBody;
    private List<string> _attachedFiles;

    public SlackNotification(string to, string body, List<string> files)
    {
        _toChannel = to;
        _messageBody = body;
        _attachedFiles = files;
    }

    // Ex 7: Renamed from Handle() to SendNotification()
    public void SendNotification()
    {
        Console.WriteLine("Sending slack...");
        Console.WriteLine($"Channel: {_toChannel}, Message: {_messageBody}");
        foreach (var file in _attachedFiles)
        {
            Console.WriteLine($"Attached file: {file}");
        }
    }
}

// Ex 6: Configuration class for ProtectApplication to reduce parameter count (Parameter Object pattern).
public class ProtectionConfig
{
    // Use read-only properties for immutability
    public string InputPath { get; }
    public string ConfigurationPath { get; }
    public string OutputPath { get; }
    public CancellationToken CancellationToken { get; }

    public ProtectionConfig(string inputPath, string configurationPath, string outputPath, CancellationToken cancellationToken)
    {
        InputPath = inputPath;
        ConfigurationPath = configurationPath;
        OutputPath = outputPath;
        CancellationToken = cancellationToken;
    }
}

// Ex 8: Tokenizer handles the lowest level of abstraction.
public class Tokenizer
{
    public List<string> Tokenize(string code)
    {
        var regexes = new[] { "a", "b" };
        var statements = code.Split(' ');
        var tokens = new List<string>();
        foreach (var regex in regexes)
        {
            foreach (var statement in statements)
            {
                tokens.Add(statement + regex);
            }
        }
        return tokens;
    }
}

// Ex 8: Lexer handles the next level of abstraction.
public class Lexer
{
    public List<string> ConvertToAst(List<string> tokens)
    {
        var ast = new List<string>();
        foreach (var token in tokens)
        {
            ast.Add(token + "_ast");
        }
        return ast;
    }
}

// Ex 8: Coordinator class handling the full parsing pipeline.
public class BetterJsAlternativeParser
{
    private readonly Tokenizer _tokenizer = new Tokenizer();
    private readonly Lexer _lexer = new Lexer();

    public string Parse(string code)
    {
        // One level of abstraction: coordinating the steps
        var tokens = _tokenizer.Tokenize(code);
        var ast = _lexer.ConvertToAst(tokens);

        foreach (var node in ast)
        {
            Console.WriteLine("Parse Node: " + node);
        }

        return string.Join(",", ast);
    }
}

public class Program
{
    // Ex 1 & 5: Magic number replaced with a named constant.
    private const int OneDayInMilliseconds = 86400000;
    
    // Ex 5: Global variables replaced with constants from ConfigurationService
    private const string CurrentUserRole = ConfigurationService.AdminRole;
    private const string UsersApiEndpoint = ConfigurationService.GetUsersEndpoint;

    public static void Main(string[] args)
    {
        // Ex 1: Renamed variable (yyyymmdstr -> todayDateString)
        var todayDateString = DateTime.Now.ToString("yyyy/MM/dd");
        Console.WriteLine("Today: " + todayDateString);

        // Ex 2: Reduced repetition for Car instances (DRY)
        const string DefaultModel = "mx";
        const string DefaultBrand = "Trek";
        var car1 = new Car(DefaultModel, DefaultBrand, "Green");
        var car2 = new Car(DefaultModel, DefaultBrand, "Red");
        var car3 = new Car(DefaultModel, DefaultBrand, "Blue");
        
        // Ex 4: Creating a Car with EngineData (Composition)
        var engineData = new EngineData("2.0L", 150.0);
        car1.SetEngine(engineData);
        Console.WriteLine($"Car 1 Engine: {car1.EngineData.Displacement}");

        // Ex 1 & 3: BankAccount usage updated (ba -> bankAccount, Deposit/Withdraw instead of direct field access)
        var bankAccount = new BankAccount();
        // Original: ba.balance -= 100; ba.balance += 50;
        bankAccount.Withdraw(100);
        bankAccount.Deposit(50);
        Console.WriteLine($"Current Balance: {bankAccount.Balance}");

        var address = "One Infinite Loop, Cupertino 95014";
        // Ex 1: Renamed variable (r -> addressMatches)
        // Note: The original regex was invalid C# syntax, but we proceed with the refactoring goal.
        var regex = new Regex(@"[^,]+,\s*(.+?)\s*(\d{5})?$");
        var addressMatches = regex.Matches(address);
        
        // Ex 2: Updated call to Save with clearer arguments (Save -> SaveAddressDetails)
        if (addressMatches.Count > 0 && addressMatches[0].Groups.Count > 2)
        {
            // Assuming Group 1 is City and Group 2 is Zip for demonstration
            SaveAddressDetails(addressMatches[0].Groups[1].Value, addressMatches[0].Groups[2].Value);
        }

        // Ex 5: Using constant (CurrentUserRole) and named constant (OneDayInMilliseconds)
        if (CurrentUserRole == ConfigurationService.AdminRole)
        {
            ClearBacklog(new List<string>() { "a", "b", "c" }, OneDayInMilliseconds);
        }

        // Ex 9: Configuration is now instantiated and used (not just called statically and ignored)
        var configService = new ConfigurationService();
        var config = configService.GetConfig();
        Console.WriteLine($"Product Code from config: {config["Product"]}"); 

        // Ex 6: Call to ProtectApplication updated to use ProtectionConfig
        var protectionConfig = new ProtectionConfig(
            inputPath: "./app.exe",
            configurationPath: "./shield.config.json",
            outputPath: "./app_protected.exe",
            cancellationToken: new CancellationTokenSource().Token // descriptive argument labels improve readability
        );
        ProtectApplication(protectionConfig);

        // Ex 1 & 7: Renamed variable and method call
        var slackNotification = new SlackNotification("bytehide", "get users", new List<string>() { "log.txt" });
        slackNotification.SendNotification();

        // Ex 7: Renamed method call (url -> UsersApiEndpoint)
        SendRequestToUsersApi(UsersApiEndpoint);
        
        // Ex 8: Call to new parser class using better abstraction
        var parser = new BetterJsAlternativeParser();
        parser.Parse("if true then do something");
    }

    // Ex 2: Renamed parameters for clarity (c -> city, z -> zipCode)
    public static void SaveAddressDetails(string city, string zipCode)
    {
        Console.WriteLine($"Saving City={city} Zip={zipCode}");
    }

    // Ex 1: Renamed parameter (ms -> timeoutInMilliseconds)
    public static void ClearBacklog(List<string> backlog, int timeoutInMilliseconds)
    {
        Console.WriteLine($"Clearing backlog after {timeoutInMilliseconds} ms");
    }

    // Ex 6: Method refactored to accept a single configuration object
    public static void ProtectApplication(ProtectionConfig config)
    {
        Console.WriteLine($"Protecting {config.InputPath} to {config.OutputPath} using {config.ConfigurationPath}");
    }

    // Ex 7: Renamed to reflect responsibility (OldRequestMethod removed)
    public static void SendRequestToUsersApi(string url)
    {
        Console.WriteLine($"Sending API request to {url}");
    }
    
    // Ex 9: Removed dead code: NewRequestMethod, OldRequestMethod, ParseBetterJSAlternative
}
