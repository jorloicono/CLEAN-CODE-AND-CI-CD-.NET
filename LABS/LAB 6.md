# Decouple code using Interfaces

In object-oriented programming, interfaces define a contract that classes can implement. They specify method signatures and properties that implementing classes must provide. This allows for consistent behavior across different types while enabling flexibility in implementation. In C#, interfaces are defined using the `interface` keyword, and classes implement them using the `: InterfaceName` syntax.

In this exercise, you will refactor a tightly coupled console application to use interfaces. By introducing interfaces, you will decouple the application logic from specific implementations, making the code more flexible and easier to maintain.

This exercise demonstrates how to use interfaces in C# to create flexible, reusable, and loosely coupled code. You’ll learn to define and implement interfaces with default methods, use interfaces as method parameters, and work with system-defined interfaces like `IComparable` and `IEnumerable`. By the end, you’ll apply these concepts to a scenario.

## Exercise scenario

Suppose you’re a software developer at a tech company working on a new project. Your task is to design a system that models people in different roles, such as teachers and students, while ensuring the code is flexible, reusable, and easy to maintain.  You’ll achieve this by leveraging interfaces and system-defined features in C#. In this exercise, you will build a console application that demonstrates how to use interfaces to decouple code, implement default methods, and create a dynamic classroom system that supports sorting and iteration.

## Task 1: Create a new C# project

To start, you need to create a new C# project in your development environment. This project will serve as the foundation for creating decoupled code using interfaces.

- Open Visual Studio Code.
- Open the terminal in Visual Studio Code by selecting `View > Terminal`.
- Navigate to the directory where you want to create your project.

**Run the following command to create a new console application:**

```bash
dotnet new console -n DecoupleWithInterfaces
```

This command creates a new console application named `DecoupleWithInterfaces`, which will serve as the starting point for the exercise.

**Navigate into the newly created project directory:**

```bash
cd DecoupleWithInterfaces
```

This step ensures that you are working within the correct project directory.

Open the project in Visual Studio Code:

```bash
code .
```

Opening the project in Visual Studio Code allows you to edit and manage the files easily.

**Paste the following command at the `DecoupleWithInterfaces` directory terminal prompt and press Enter:**

```bash
echo namespace DecoupleWithInterfaces; > Classroom.cs
echo <Project Sdk="Microsoft.NET.Sdk"> > DecoupleWithInterfaces.csproj
echo namespace DecoupleWithInterfaces; > IPerson.cs
echo namespace DecoupleWithInterfaces; > PersonUtilities.cs
echo namespace DecoupleWithInterfaces; > Program.cs
echo namespace DecoupleWithInterfaces; > Student.cs
echo namespace DecoupleWithInterfaces; > Teacher.cs
echo "Done"
```

Verify files are created in the Visual Studio Code Explorer:

- `Classroom.cs` for the `Classroom` class.
- `DecoupleWithInterfaces.csproj` for the project file.
- `IPerson.cs` for the `IPerson` interface.
- `PersonUtilities.cs` for the utility class.
- `Program.cs` for the main entry point of the application.
- `Teacher.cs` for the `Teacher` class.
- `Student.cs` for the `Student` class.

## Task 2: Extend the IPerson interface

You start by adding a new property and a default method to the `IPerson` interface. Default methods allow you to provide functionality directly in the interface, which can be overridden by implementing classes if needed.

Add the code for the `IPerson` interface to the file named `IPerson.cs`:

```csharp
namespace DecoupleWithInterfaces;

public interface IPerson
{
    string Name { get; set; }
    int Age { get; set; }
    void DisplayInfo();

    // New property
    string Role { get; }

    // Default method
    void Greet()
    {
        Console.WriteLine($"Hello, my name is {Name} and I am a {Role}.");
    }
}
```

This code introduces a default method and a new property to the interface, enabling shared functionality across implementing classes.

Notice the `IPerson` interface includes the `Role` property and the `Greet` method with a default implementation.

## Task 3: Update Teacher and Student classes

The `Teacher` and `Student` classes now implement the new `Role` property. The `Teacher` class overrides the default `Greet` method, while the `Student` class uses the default implementation.

Add the code for the `Teacher` class to the file named `Teacher.cs`:

```csharp
namespace DecoupleWithInterfaces;

public class Teacher : IPerson
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; } = 0;
    public string Role => "Teacher";

    public void DisplayInfo()
    {
        Console.WriteLine($"Teacher Name: {Name}, Age: {Age}");
    }

    public void Greet()
    {
        Console.WriteLine($"Hello, I am {Name}, and I am a teacher.");
    }
}
```

This code demonstrates how the `Teacher` class implements the `IPerson` interface, defines the `Role` property, and overrides the default `Greet` method.

Add the code for the `Student` class to the file named `Student.cs`:

```csharp
namespace DecoupleWithInterfaces;

public class Student : IPerson, IComparable
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; } = 0;
    public string Role => "Student";

    public void DisplayInfo()
    {
        Console.WriteLine($"Student Name: {Name}, Age: {Age}");
    }

    public int CompareTo(Student? other)
    {
        if (other == null) return 1;
        return this.Age.CompareTo(other.Age);
    }
}
```

This code shows how the `Student` class implements the `IPerson` interface, defines the `Role` property, and uses the default `Greet` method while adding support for sorting through the `IComparable` interface.
Observe that:

- The `Teacher` class overrides the default `Greet` method with a custom implementation.
- The `Student` class uses the default `Greet` method and implements the `IComparable` interface for sorting.

## Task 4: Use interfaces as method parameters

In this task, you will create a utility class that uses an interface as a method parameter. This demonstrates how interfaces allow you to handle multiple object types generically, enabling flexibility and reusability in your code.

Add the code for the `PersonUtilities` class to the file named `PersonUtilities.cs`:

```csharp
namespace DecoupleWithInterfaces;

public class PersonUtilities
{
    public static void PrintPersonDetails(IPerson person)
    {
        person.DisplayInfo();
        person.Greet();
    }
}
```

This code demonstrates how to use an interface as a method parameter to handle multiple object types generically.

The `PrintPersonDetails` method accepts an `IPerson` object as a parameter, allowing the method to work with any class that implements `IPerson`, such as `Teacher` or `Student`. Inside the method, the `DisplayInfo` and `Greet` methods are called on the `IPerson` object.

## Task 5: Create a Classroom with IEnumerable

In this task, you will create a `Classroom` class that uses `List<T>` to store students dynamically and implements `IEnumerable` to allow iteration over the collection. You will also test the `Classroom` class by adding, sorting, and displaying students.

Create the `Classroom` class in the file named `Classroom.cs` and add the following code:

```csharp
namespace DecoupleWithInterfaces;

using System.Collections;
using System.Collections.Generic;

public class Classroom : IEnumerable<Student>
{
    private List<Student> students = new List<Student>();

    public void AddStudent(Student student)
    {
        students.Add(student);
    }

    public void SortStudentsByAge()
    {
        students.Sort(); // Uses the IComparable implementation in Student
    }

    public IEnumerator<Student> GetEnumerator()
    {
        return students.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
```

This code shows how to create a custom collection class that supports dynamic storage, sorting, and iteration using `List<T>` and `IEnumerable`.
Observe the `Classroom` class:

- The `Classroom` class implements `IEnumerable<Student>`, providing an enumerator through the `GetEnumerator` method so a `foreach` loop can iterate over the `students` list directly.
- The `foreach` loop automatically uses `GetEnumerator`, which handles the iteration process internally by calling `MoveNext()` and accessing the `Current` property.

## Task 6: Update the Program class

Update the `Program.cs` file to demonstrate the functionality of the `Classroom` class.

```csharp
namespace DecoupleWithInterfaces;

class Program
{
    static void Main(string[] args)
    {
        IPerson teacher = new Teacher { Name = "Helen Karu", Age = 35 };
        IPerson student1 = new Student { Name = "Eba Lencho", Age = 20 };
        IPerson student2 = new Student { Name = "Frederiek Eppink", Age = 22 };

        // Use the utility class
        PersonUtilities.PrintPersonDetails(teacher);
        PersonUtilities.PrintPersonDetails(student1);

        // Create a classroom and add students
        Classroom classroom = new Classroom();
        classroom.AddStudent((Student)student1);
        classroom.AddStudent((Student)student2);

        // Sort students by age
        classroom.SortStudentsByAge();
        Console.WriteLine("\nSorted Students by Age:");
        foreach (Student student in classroom)
        {
            student.DisplayInfo();
        }

        // Demonstrate ArgumentException for incompatible comparison
        try
        {
            Console.WriteLine("\nAttempting to compare a Student with a Teacher...");
            Student student = (Student)student1;
            int comparisonResult = student.CompareTo(teacher); // This will throw an exception
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
```

This code demonstrates how to use the `Classroom` class to store, sort, and display students dynamically.

The `Program` class:

- Creates `Teacher` and `Student` objects using the `IPerson` interface and passes them to `PersonUtilities.PrintPersonDetails`.
- Adds `Student` objects to the `Classroom` collection, which implements `IEnumerable<Student>` for iteration.
- Sorts the students by age using `SortStudentsByAge`, which relies on the `IComparable` implementation in `Student`.
- Iterates over the `Classroom` collection using a `foreach` loop to display the sorted student details.

## Task 7: Build and run the program

- Open the terminal in Visual Studio Code.

Run the following command to build the program:

```bash
dotnet build
```

Run the following command to execute the program:

```bash
dotnet run
```

The following is the expected console output (single-line formatting simplified for readability):

```text
Teacher Name: Helen Karu, Age: 35
Hello, I am Helen Karu, and I am a teacher.
Student Name: Eba Lencho, Age: 20
Hello, my name is Eba Lencho and I am a Student.

Sorted Students by Age:
Student Name: Eba Lencho, Age: 20
Student Name: Frederiek Eppink, Age: 22

Attempting to compare a Student with a Teacher...
Error: obj (Parameter 'The object being compared must be of type Student.')
```

The teacher and student details are printed using the `PersonUtilities` class, and the students in the `Classroom` are displayed in ascending order of age after sorting.  The `foreach` loop successfully iterates over the `Classroom` collection, and the attempted comparison between `Student` and `Teacher` demonstrates an `ArgumentException`.

Using interfaces helps decouple components, making your application more flexible and maintainable, because interfaces define clear contracts between parts of the system.  Working with system-defined interfaces like `IComparable` and `IEnumerable` further enhances your code’s functionality by enabling built-in sorting and iteration capabilities.
