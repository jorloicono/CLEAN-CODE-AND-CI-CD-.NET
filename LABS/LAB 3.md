## Exercise: Fix the LSP Violation in C#

### Context

The Liskov Substitution Principle says that objects of a derived type must be usable wherever their base type is expected, without breaking the correctness of the program.
***

### Starting code 

```csharp
using System;
using System.Collections.Generic;

namespace LspExercise
{
    public abstract class Document
    {
        public string Title { get; set; }

        public virtual void Print()
        {
            Console.WriteLine($"Printing document: {Title}");
        }

        public virtual void Sign(string signer)
        {
            Console.WriteLine($"Document '{Title}' signed by {signer}");
        }
    }

    public class Contract : Document
    {
        public DateTime ExpirationDate { get; set; }

        // Inherits Sign behavior: Contracts are signable.
    }

    public class ReadOnlyReport : Document
    {
        // ReadOnlyReport must never be signed,
        // but it still overrides Sign from Document.
        public override void Sign(string signer)
        {
            // Behavior changes: now it throws instead of signing.
            throw new InvalidOperationException("Read-only report cannot be signed.");
        }
    }

    public static class DocumentProcessor
    {
        public static void SignAll(IEnumerable<Document> documents, string signer)
        {
            foreach (var doc in documents)
            {
                // Potential runtime failure if doc is ReadOnlyReport.
                doc.Sign(signer);
            }
        }
    }

    public class Program
    {
        public static void Main()
        {
            var docs = new List<Document>
            {
                new Contract { Title = "Supplier Contract", ExpirationDate = DateTime.UtcNow.AddYears(1) },
                new ReadOnlyReport { Title = "Q4 Financial Report" }
            };

            // This call currently throws at runtime.
            DocumentProcessor.SignAll(docs, "Alice");
        }
    }
}
```

This design violates LSP because a `ReadOnlyReport` cannot safely replace a `Document` in all contexts: client code that expects every `Document` to be signable will break when it receives a `ReadOnlyReport`.

***

### Task for the student

Change the code so that:

- **Any** object used as a `Document` can safely be substituted without causing unexpected runtime failures when client code only relies on guarantees of `Document`.
- Non‑signable documents (like `ReadOnlyReport`) cannot be passed to code that expects to sign documents, ideally enforced by the type system instead of runtime exceptions.

Guidance (keep this, or hide it if you want the task more open‑ended):

- Consider extracting a separate abstraction (e.g., an interface) for “signable” documents.  
- Adjust `DocumentProcessor.SignAll` and `Main` so that only signable types are passed for signing.

The student should:

1. Modify the class hierarchy and/or introduce interfaces.  
2. Update `DocumentProcessor.SignAll`.  
3. Update `Main` so the program runs without throwing and respects LSP.

***

## Questions after the refactor

Ask these once the student has changed the code.

### Understanding the original problem

1. In the original code, which line in `DocumentProcessor.SignAll` caused the runtime exception, and why is that a violation of the Liskov Substitution Principle?  
2. What implicit “contract” did `Document.Sign` communicate to calling code, and how did `ReadOnlyReport` break that contract by overriding it?

### About the refactor

3. After your changes, what is now the correct abstraction to depend on when the code needs “signable” behavior (e.g., an interface, base class)? Explain briefly.  
4. Show the new method signature of `DocumentProcessor.SignAll`. How does this new signature prevent misuse at compile time?  
5. How does your new design ensure that a `ReadOnlyReport` can still be treated as a `Document` in contexts where only printing (not signing) is required?

