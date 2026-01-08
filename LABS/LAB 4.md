## Exercise: Fix the Interface Segregation Violation in C#

### Context

The Interface Segregation Principle says that no client should be forced to depend on methods it does not use; instead of one “fat” interface, prefer multiple small, focused interfaces.

***

### Starting code 

```csharp
using System;

namespace IspExercise
{
    // "Fat" interface: not all devices support all operations.
    public interface IMultiFunctionDevice
    {
        void Print(string content);
        void Scan(string content);
        void Fax(string content);
        void PrintDuplex(string content);
    }

    // High-end printer: supports everything.
    public class OfficeAllInOne : IMultiFunctionDevice
    {
        public void Print(string content)
        {
            Console.WriteLine($"Printing: {content}");
        }

        public void Scan(string content)
        {
            Console.WriteLine($"Scanning: {content}");
        }

        public void Fax(string content)
        {
            Console.WriteLine($"Faxing: {content}");
        }

        public void PrintDuplex(string content)
        {
            Console.WriteLine($"Duplex printing: {content}");
        }
    }

    // Low-end home printer: only print and scan.
    public class HomeInkjet : IMultiFunctionDevice
    {
        public void Print(string content)
        {
            Console.WriteLine($"Printing: {content}");
        }

        public void Scan(string content)
        {
            Console.WriteLine($"Scanning: {content}");
        }

        // These capabilities do not exist on this device,
        // but the interface forces the class to implement them.
        public void Fax(string content)
        {
            // ISP violation symptom: method not really supported.
            throw new NotSupportedException("Fax is not supported on HomeInkjet.");
        }

        public void PrintDuplex(string content)
        {
            // Another unused / fake implementation.
            throw new NotSupportedException("Duplex printing is not supported on HomeInkjet.");
        }
    }

    public class Program
    {
        static void Main()
        {
            IMultiFunctionDevice office = new OfficeAllInOne();
            IMultiFunctionDevice home = new HomeInkjet();

            office.Print("Quarterly report");
            office.Fax("Legal document");

            // At compile time this looks fine, but at runtime it will throw.
            home.Print("Boarding pass");
            home.Fax("Photo of cat");
        }
    }
}
```

This design violates ISP because `HomeInkjet` is forced to implement `Fax` and `PrintDuplex` that it does not actually support, leading to fake or throwing implementations.

***

### Task for the student

Change the code so that:

- Each class implements **only** the operations it truly supports.
- No client is forced to depend on methods it does not need.  
- Code that needs only printing or scanning does not see fax/duplex methods, while code that needs “all features” can still work with a suitable abstraction.

Guidance (optional, can be hidden for students):

- Split `IMultiFunctionDevice` into smaller interfaces, such as `IPrinter`, `IScanner`, `IFax`, `IDuplexPrinter`.
- Have `OfficeAllInOne` implement multiple interfaces, and `HomeInkjet` implement only the ones it supports.  
- Adjust `Main` so that variables have the most specific interface type needed for their use.

The student should:

1. Redesign the interfaces (segregate the fat one).  
2. Update the classes to implement only relevant interfaces.  
3. Update `Main` so that calls are type‑safe and there are no `NotSupportedException` placeholders.

***

## Questions after the refactor

### Understanding the original problem

1. Which methods in `HomeInkjet` are clear indicators that the Interface Segregation Principle is being violated, and why?
2. Why is throwing `NotSupportedException` from interface methods typically a smell that the interface is too broad?

### About your new interfaces

3. What new interfaces did you create, and what responsibility does each one represent (e.g., printing, scanning)? Briefly describe each.  
4. After your changes, what are the implemented interfaces for `OfficeAllInOne` and for `HomeInkjet`? How does this reflect their real capabilities?

### About clients and usage

5. Show the new type declarations you used in `Main`. For example, which interface type do you assign to the home device variable now, and why?  
6. How does your refactor ensure that client code that only needs printing cannot accidentally call fax or duplex methods?

