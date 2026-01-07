## Lab overview

You will start from the **bad** code below and, for each exercise, answer the questions and refactor it into **good** code touching:

- Variable naming and searchability  
- DRY and descriptive arguments  
- Encapsulation, access modifiers, getters/setters  
- Composition vs inheritance  
- Magic strings, large functions, global contamination, unused code, etc.[2][1]

***

## Starter bad code

Put this into a new console project as `Program.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;

public class helper
{
    public static Dictionary<string, string> Config()
    {
        return new Dictionary<string, string>()
        {
            ["prd"] = "shield",
            ["r"] = "Admin",
            ["u"] = "https://api.byteapp.dev/getusers",
        };
    }
}

public class CarEngineData : Car
{
    private string model { get; set; }
    private string brand { get; set; }
    public string d;
    public double h;

    public CarEngineData(string m, string b, string displacement, double horses)
    {
        model = m;
        brand = b;
        d = displacement;
        h = horses;
    }
}

public class Car
{
    public string m { get; set; }
    public string b { get; set; }
    public string c { get; set; }

    public Car(string model, string brand, string color)
    {
        m = model;
        b = brand;
        c = color;
    }
}

public class BankAccount
{
    public double balance = 5000;

    public void op(bool t, double a)
    {
        if (t)
        {
            balance = balance + a;
        }
        else
        {
            balance = balance - a;
        }
    }
}

public class SlackNotification
{
    private string _to;
    private string _body;
    private List<string> _files;

    public SlackNotification(string to, string body, List<string> files)
    {
        _to = to;
        _body = body;
        _files = files;
    }

    public void Handle()
    {
        Console.WriteLine("Sending slack...");
        Console.WriteLine(_to + " " + _body);
        foreach (var f in _files)
        {
            Console.WriteLine("file=" + f);
        }
    }
}

public class Program
{
    static string role = "Admin";
    static string url = "https://api.byteapp.dev/getusers";

    public static void Main(string[] args)
    {
        var yyyymmdstr = DateTime.Now.ToString("YYYY/MM/DD");
        Console.WriteLine("today: " + yyyymmdstr);

        var car = new Car("mx", "Trek", "Green");
        var car2 = new Car("mx", "Trek", "Red");
        var car3 = new Car("mx", "Trek", "Blue");

        var ba = new BankAccount();
        ba.balance -= 100;
        ba.balance += 50;

        var address = "One Infinite Loop, Cupertino 95014";
        var r = new Regex(@"/ ^[^,\\] +[,\\\s] + (.+?)\s * (\d{ 5 })?$/").Matches(address);
        Save(r[0].Value, r[1].Value);

        if (role == "Admin")
        {
            ClearBacklog(new List<string>() { "a", "b", "c" }, 86400000);
        }

        helper.Config(); // not really used

        ProtectApplication("./app.exe", "./shield.config.json", "./app_protected.exe",
            new CancellationTokenSource().Token);

        var s = new SlackNotification("bytehide", "get users", new List<string>() { "log.txt" });
        s.Handle();

        OldRequestMethod(url);
    }

    public static void Save(string c, string z)
    {
        Console.WriteLine("saving city=" + c + " zip=" + z);
    }

    public static void ClearBacklog(List<string> backlog, int ms)
    {
        Console.WriteLine("clearing backlog after " + ms + " ms");
    }

    public static void ProtectApplication(string path, string configurationPath, string outPutPath,
        CancellationToken cancellationToken)
    {
        Console.WriteLine("protecting " + path + " to " + outPutPath + " using " + configurationPath);
    }

    public static void OldRequestMethod(string url)
    {
        Console.WriteLine("OLD REQUEST to " + url);
    }

    public static void NewRequestMethod(string url)
    {
        Console.WriteLine("NEW REQUEST to " + url);
    }

    public static string ParseBetterJSAlternative(string code)
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

        var ast = new List<string>();
        foreach (var token in tokens)
        {
            ast.Add(token + "_ast");
        }

        foreach (var node in ast)
        {
            Console.WriteLine("parse " + node);
        }

        return string.Join(",", ast);
    }
}
```

This code intentionally violates most tips from the article.[][]

***

## Exercise 1 – Better variable names

Focus on: easy-to-remember, consistent and searchable names.[]

Questions:

1. Rename `yyyymmdstr`, `m`, `b`, `c`, `ba`, `r`, `ms`, `s`, `url`, `role` to names that clearly express their purpose.  
2. Unify synonyms like `m`/`model`/`Model`/`mx` into a single consistent naming vocabulary (e.g., `Model`).  
3. Replace the magic number `86400000` with a named constant. Where would you declare this constant?

Refactor the code in `Main`, `Car`, `ClearBacklog` and the regex section accordingly.

***

## Exercise 2 – DRY and descriptive parameters

Focus on: avoiding repetition and using descriptive arguments.[]

Tasks and questions:

1. The three `Car` instances repeat the same brand and model.  
   - How can you reduce repetition while keeping the code readable?  
2. Method `Save(string c, string z)` uses cryptic parameter names.  
   - Rename the parameters and their uses to be self‑documenting.  
3. The method `op(bool t, double a)` in `BankAccount` is unclear.  
   - Split this into explicit operations (e.g., `Deposit`, `Withdraw`) or use clearer parameter names. Which design do you choose and why?

After answering, refactor `BankAccount` and call sites in `Main`.

***

## Exercise 3 – Encapsulation, access modifiers, getters/setters

Focus on: private/protected members, getters, and controlling modifications.[][]

Questions:

1. What problems can `public double balance = 5000;` cause?  
2. Turn `BankAccount` into a properly encapsulated class:  
   - Make balance private.  
   - Expose a read‑only property `Balance`.  
   - Provide `Deposit` and `Withdraw` methods with validation.  
3. In `Car`, do you really want `m`, `b`, `c` to have public setters?  
   - Make a design decision and apply appropriate access modifiers.

Update all usages in `Main` to compile with the new API.

***

## Exercise 4 – Composition over inheritance

Focus on: `CarEngineData : Car` and “is‑a” vs “has‑a”.[]

Questions:

1. Is `CarEngineData` really a kind of `Car`, or should a `Car` **have** engine data?  
2. Refactor so that:  
   - `CarEngineData` is an independent class with clear properties.  
   - `Car` has a property `EngineData` and a method like `SetEngine(...)`.  
3. Show how you would create a `Car` and set its engine in `Main`.

Remove any duplicated model/brand fields from `CarEngineData`.

***

## Exercise 5 – Magic strings and magic numbers

Focus on: roles, URLs, product codes, timeouts, etc.[][]

Questions:

1. What are the magic strings and numbers in `Program` and `helper.Config`?  
2. Extract them into constants or configuration objects. For example:  
   - `const string AdminRole = "Admin";`  
   - `const string GetUsersEndpoint = "https://api.byteapp.dev/getusers";`  
3. Where should those constants live to avoid duplication yet keep cohesion?

Refactor `Program` and `helper` using your constants/config types.

***

## Exercise 6 – Function arguments and data objects

Focus on: limiting function parameters and grouping them.[]

Questions:

1. `ProtectApplication` has four parameters.  
   - Design a small configuration class (`ProtectionConfig`) and refactor the method to accept a single parameter.  
2. Update the call site in `Main` so intent is obvious from the object initialization.  
3. Are there other functions where grouping parameters would increase clarity?

Implement `ProtectionConfig` and adjust `ProtectApplication`.

***

## Exercise 7 – Descriptive function names

Focus on: naming functions for what they do.[]

Questions:

1. `Handle()` in `SlackNotification` is vague. What should it be called instead to reflect its purpose?  
2. Rename `OldRequestMethod` and `NewRequestMethod` to something that reflects their **responsibility** rather than their age.  
3. If a method does more than one obvious thing, should the name change, or should you split the method?

Refactor the class/method names and their usages.

***

## Exercise 8 – One level of abstraction per function

Focus on: `ParseBetterJSAlternative` which mixes several steps.[]

Questions:

1. Identify the different abstraction steps in `ParseBetterJSAlternative`.  
2. Extract at least two classes or methods:  
   - A `Tokenizer` that turns code into tokens.  
   - A `Lexer` that turns tokens into an AST.  
3. Make a class (e.g., `BetterJsAlternative`) that coordinates them like in the article, and adjust `ParseBetterJSAlternative` or remove it in favor of the new design.

Implement the new classes so the parsing pipeline can be tested independently.

***

## Exercise 9 – Globals, configuration and unused code

Focus on: avoiding global functions and dead code.[][]

Questions:

1. Why is `helper.Config()` as a global static method risky for larger projects?  
2. Wrap configuration in a dedicated class with instance methods, similar to the article’s `Configuration` class, and use it from `Main`.  
3. Identify unused or obsolete code (e.g., `OldRequestMethod`) and delete it. Where can you recover it if needed?

Refactor `helper` into a configuration type and prune dead code.

***

[19](https://www.codeguru.com/csharp/tips-clean-code-c-sharp/)
[20](https://github.com/bytehide)
[21](https://dev.to/bytehide/how-to-write-clean-c-code-5-tips-that-will-save-you-hours-of-programming-d36?comments_sort=top)
