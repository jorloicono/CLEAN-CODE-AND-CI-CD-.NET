using System;

namespace IspExercise
{
    // "Fat" interface: not all devices support all operations.
    public interface IBasicFunctionDevice
    {
        void Print(string content);
        void Scan(string content);
    }
	
	public interface IComplexFunctionDevice : IBasicFunctionDevice
	{
		void Fax(string content);
        void PrintDuplex(string content);	
	}

    // High-end printer: supports everything.
    public class OfficeAllInOne : IComplexFunctionDevice
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
    public class HomeInkjet : IBasicFunctionDevice
    {
        public void Print(string content)
        {
            Console.WriteLine($"Printing: {content}");
        }

        public void Scan(string content)
        {
            Console.WriteLine($"Scanning: {content}");
        }
    }

    public class Program
    {
        static void Main()
        {
            IComplexFunctionDevice office = new OfficeAllInOne();
            IBasicFunctionDevice home = new HomeInkjet();

            office.Print("Quarterly report");
            office.Fax("Legal document");

            home.Print("Boarding pass");
        }
    }
}