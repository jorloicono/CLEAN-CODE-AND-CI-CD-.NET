## Exercise: Fix the Dependency Inversion Violation in C#

### Context

The Dependency Inversion Principle states that high-level modules should not depend on low-level modules; both should depend on abstractions. Abstractions should not depend on details; details should depend on abstractions.

***

### Starting code 

```csharp
using System;
using System.Collections.Generic;

namespace DipExercise
{
    // Low-level module: concrete data access.
    public class SqlEmployeeRepository
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
        private SqlEmployeeRepository _repository;

        public EmployeeService()
        {
            // High-level depends on concrete low-level: tight coupling.
            _repository = new SqlEmployeeRepository();
        }

        public decimal CalculateAverageSalary()
        {
            var employees = _repository.GetEmployees();
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
            var employees = _repository.GetEmployees();
            return employees.Find(e => e.Id == id);
        }
    }

    public class Program
    {
        static void Main()
        {
            EmployeeService service = new EmployeeService();
            Console.WriteLine($"Average salary: {service.CalculateAverageSalary():C}");
            var employee = service.GetEmployee(1);
            Console.WriteLine($"Employee: {employee.Name}, Salary: {employee.Salary:C}");
        }
    }
}
```

This violates DIP because the high-level `EmployeeService` directly depends on the concrete low-level `SqlEmployeeRepository` and `Employee`, making it hard to swap data sources (e.g., for testing or MongoDB) without changing the high-level code.

***

### Task for the student

Change the code so that:

- High-level modules (`EmployeeService`) depend only on abstractions, not concrete low-level classes.
- Low-level modules (`SqlEmployeeRepository`) implement the same abstractions.  
- The system is flexible enough to easily swap the repository (e.g., for unit tests with a mock or a different database).

Guidance (optional, hide for harder task):

- Introduce an interface for the repository abstraction, e.g., `IEmployeeRepository`.  
- Make `SqlEmployeeRepository` implement it.  
- Inject the abstraction into `EmployeeService` via constructor (Dependency Injection).  
- Update `Main` to create and pass the concrete implementation.

The student should:

1. Create the abstraction(s) for data access.  
2. Refactor `EmployeeService` to depend on abstraction.  
3. Adjust instantiation in `Main` (no `new SqlEmployeeRepository()` inside `EmployeeService`).

***

## Questions after the refactor

### Understanding the original problem

1. Why does the original `EmployeeService` constructor violate the first part of the Dependency Inversion Principle (high-level depending on low-level)?  
2. What problems arise if you want to test `EmployeeService` with fake data or switch to a `MongoEmployeeRepository`?

### About the abstraction

3. What abstraction (interface) did you create for the repository? What methods does it declare, and why exactly those?  
4. After your changes, does the abstraction (`IEmployeeRepository`) depend on details like `SqlEmployeeRepository`, or the other way around? Explain.

### About injection and coupling

5. How is the dependency now provided to `EmployeeService` (e.g., constructor parameter)? Show the new constructor signature.  
6. In `Main`, what type do you now pass to `EmployeeService`, and why is that better than the original instantiation?

### Deeper reasoning

7. Suppose you add a `FakeEmployeeRepository` for unit tests. How many lines would you need to change in `EmployeeService` to use it (zero, one, more)?  
8. How does this refactor make the code more testable and extensible if a new data source (e.g., CSV file) is added later?
