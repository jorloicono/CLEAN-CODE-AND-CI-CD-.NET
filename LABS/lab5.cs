using System;
using System.Collections.Generic;

namespace DipExercise
{
	public interface IDatabase 
	{
		List<Employee> GetEmployees();
	}
	
    // Low-level module: concrete data access.
    public class SqlEmployeeRepository : IDatabase
    {
        public List<Employee> GetEmployees()
        {
            // Simulate SQL query.
            Console.WriteLine("Executing SQL query to fetch employees...");
            return new List<Employee>
            {
                new Employee { Id = 1, Name = "Alice", Salary = 50000 },
                new Employee { Id = 2, Name = "Bob", Salary = 60000 }
            };
        }
    }

    // Low-level data structure.
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Salary { get; set; }
    }

    // High-level module: business logic that depends directly on concrete low-level details.
    public class EmployeeService
    {
		private readonly IDatabase _db;

        public EmployeeService(IDatabase db)
        {
            // High-level depends on concrete low-level: tight coupling.
            _db = db;
        }

        public decimal CalculateAverageSalary()
        {
            var employees = _db.GetEmployees();
            if (employees.Count == 0) return 0;

            decimal total = 0;
            foreach (var emp in employees)
            {
                total += emp.Salary;
            }
            return total / employees.Count;
        }

        public Employee GetEmployee(int id)
        {
            var employees = _db.GetEmployees();
            return employees.Find(e => e.Id == id);
        }
    }

    public class Program 
    {
        static void Main()
        {
			IDatabase db = new SqlEmployeeRepository();
            EmployeeService service = new EmployeeService(db);
            Console.WriteLine($"Average salary: {service.CalculateAverageSalary():C}");
            var employee = service.GetEmployee(1);
            Console.WriteLine($"Employee: {employee.Name}, Salary: {employee.Salary:C}");
        }
    }
}