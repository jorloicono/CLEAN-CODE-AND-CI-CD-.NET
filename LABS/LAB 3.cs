using System;
using System.Collections.Generic;

namespace LspExercise
{
    // Base abstraction: all documents can be printed
    public abstract class Document
    {
        public string Title { get; set; }

        public virtual void Print()
        {
            Console.WriteLine($"Printing document: {Title}");
        }
    }

    // Separate abstraction for signable behavior
    public interface ISignable
    {
        void Sign(string signer);
    }

    // Contracts are documents AND signable
    public class Contract : Document, ISignable
    {
        public DateTime ExpirationDate { get; set; }

        public void Sign(string signer)
        {
            Console.WriteLine($"Contract '{Title}' signed by {signer}");
        }
    }

    // Read-only reports are documents, but NOT signable
    public class ReadOnlyReport : Document
    {
        // No Sign method at all
    }

    public static class DocumentProcessor
    {
        // Now explicitly requires signable documents
        public static void SignAll(IEnumerable<ISignable> documents, string signer)
        {
            foreach (var doc in documents)
            {
                doc.Sign(signer);
            }
        }
    }

    public class Program
    {
        public static void Main()
        {
            var contract = new Contract
            {
                Title = "Supplier Contract",
                ExpirationDate = DateTime.UtcNow.AddYears(1)
            };

            var report = new ReadOnlyReport
            {
                Title = "Q4 Financial Report"
            };

            // Only signable documents can be passed here (compile-time safety)
            DocumentProcessor.SignAll(
                new List<ISignable> { contract },
                "Alice"
            );

            // Both can still be used as Documents
            var allDocuments = new List<Document> { contract, report };
            foreach (var doc in allDocuments)
            {
                doc.Print();
            }
        }
    }
}
