## Lab overview

You will work on a tiny “Nerds Management” console app with one big **God class** and lots of SOLID violations.[4][1]

For each exercise:

- Answer the questions in comments.
- Refactor the code.
- Keep the app compiling and running.

***

## Starter bad code (put in `Program.cs`)

```csharp
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;

namespace NerdLab
{
    // BIG GOD CLASS: violates almost everything
    public class NerdManager
    {
        // Data model + state
        public class Nerd
        {
            public string Name;
            public int IQ;
            public double SuspenderTension;
            public double Radius;
            public bool IsSelected;
            public Image Image;
            public Point Position;
            public double Heading;

            // Storage stuff
            public string FilePath;

            // UI-ish stuff
            public string UiColorHex;
        }

        // State for drawing
        private readonly List<Nerd> _nerds = new List<Nerd>();
        private readonly List<Nerd> _selected = new List<Nerd>();
        private readonly string _storageFile = "nerds.xml";

        // Hard reference to concrete file+XML, platform-specific
        public void Load()
        {
            Console.WriteLine("Loading nerds...");
            if (File.Exists(_storageFile))
            {
                // impossible to test without touching disk
                var serializer = new XmlSerializer(typeof(List<Nerd>));
                using (var reader = new StreamReader(_storageFile))
                {
                    var result = (List<Nerd>)serializer.Deserialize(reader);
                    _nerds.Clear();
                    _nerds.AddRange(result);
                }
            }
            else
            {
                // Seed some fake nerds
                _nerds.Add(new Nerd
                {
                    Name = "Alice",
                    IQ = 130,
                    Radius = 10,
                    SuspenderTension = 1.2,
                    UiColorHex = "#FF0000",
                    FilePath = "alice.png"
                });
                _nerds.Add(new Nerd
                {
                    Name = "Bob",
                    IQ = 90,
                    Radius = 15,
                    SuspenderTension = 0.8,
                    UiColorHex = "#00FF00",
                    FilePath = "bob.png"
                });
            }
        }

        // Mix of selection logic, rendering policy, console IO
        public void DrawAllNerds(bool debugMode, object plotter = null)
        {
            Console.WriteLine("Drawing nerds (debug=" + debugMode + ")");

            foreach (var nerd in _nerds)
            {
                // selection logic inside drawing
                nerd.IsSelected = nerd.IQ > 100;
                if (nerd.IsSelected)
                {
                    _selected.Add(nerd);
                }

                // fake drawing
                Console.WriteLine("Drawing nerd " + nerd.Name + " at " + nerd.Position);

                // platform-specific debug drawing
                if (plotter != null)
                {
                    // pretend that we draw with PeachPuff
                    Console.WriteLine("Plotter debug circle PeachPuff radius=" + nerd.Radius);
                }

                // UI color decision mixed in
                if (nerd.IQ > 120)
                {
                    nerd.UiColorHex = "#FFD700"; // gold
                }
                else if (nerd.IQ < 95)
                {
                    nerd.UiColorHex = "#808080"; // gray
                }
            }
        }

        // Nasty OCP + LSP violations
        public void PrintSpecialDanceInstructions(Nerd n)
        {
            Console.WriteLine("Dance instructions for " + n.Name);

            if (n is GeniusNerd)
            {
                Console.WriteLine("Do complex disco with moonwalk.");
            }
            else if (n is ChildNerd)
            {
                // base class knows child type and changes behaviour
                Console.WriteLine("Child cannot dance disco, throwing...");
                throw new Exception("Can't");
            }
            else
            {
                Console.WriteLine("Basic disco.");
            }
        }

        // more responsibilities: saving
        public void Save()
        {
            Console.WriteLine("Saving nerds...");
            var serializer = new XmlSerializer(typeof(List<Nerd>));
            using (var writer = new StreamWriter(_storageFile))
            {
                serializer.Serialize(writer, _nerds);
            }
        }

        // dependency created directly here (no abstraction, no injection)
        public void EmailAllNerds()
        {
            Console.WriteLine("Emailing nerds...");
            var emailer = new EmailSender();
            foreach (var n in _nerds)
            {
                emailer.Send("Hello " + n.Name + ", welcome to NerdLab!", n.Name + "@example.com");
            }
        }

        // Weird utility: math + model mixed
        public IList<Nerd> FitNerdsIntoPaddedRoom(IList<Point> boundary)
        {
            Console.WriteLine("Fitting nerds into padded room with " + boundary.Count + " points");
            // super fake implementation
            foreach (var n in _nerds)
            {
                n.Position = boundary[0];
            }
            return _nerds;
        }

        // Overloaded method that tries to be "clever" with cloning
        public Mammal CloneAsMammal(GeniusNerd n)
        {
            // Try to look like a polymorphic clone but isn't
            return new GeniusNerd(n.Name, n.IQ, n.Diopter);
        }
    }

    // Base type with bad encapsulation
    public class Mammal
    {
        protected int vertebrae;
        protected bool isMoonWalking;

        public virtual Mammal Clone()
        {
            return new Mammal { vertebrae = this.vertebrae };
        }

        public virtual void OnMouseDown()
        {
            isMoonWalking = true;
        }

        public virtual void OnMouseUp()
        {
            if (isMoonWalking)
            {
                Console.WriteLine("Stopping moonwalk.");
                isMoonWalking = false;
            }
        }
    }

    public class GeniusNerd : Mammal
    {
        public string Name { get; private set; }
        public int IQ { get; private set; }
        public double Diopter { get; private set; }

        public GeniusNerd(string name, int iq, double diopter)
        {
            Name = name;
            IQ = iq;
            Diopter = diopter;
            vertebrae = 33;
        }

        // Tries to "hide" base clone instead of proper polymorphism
        public new GeniusNerd Clone()
        {
            return new GeniusNerd(Name, IQ, Diopter);
        }

        // could randomly change isMoonWalking and break base assumptions
        public override void OnMouseDown()
        {
            base.OnMouseDown();
            if (IQ > 150)
            {
                isMoonWalking = false; // destroys invariant
            }
        }
    }

    public class ChildNerd : Mammal
    {
        public string Nick;
    }

    // Fat, concrete "service" without abstraction
    public class EmailSender
    {
        public void Send(string body, string to)
        {
            Console.WriteLine("Sending email to " + to + " => " + body);
        }
    }

    // Another bloated interface with multiple responsibilities
    public interface INerdRepository
    {
        // data access
        IList<NerdManager.Nerd> LoadAll();
        void SaveAll(IList<NerdManager.Nerd> nerds);

        // UI stuff (bad idea)
        void HighlightOnScreen(NerdManager.Nerd nerd);

        // lifecycle
        void Initialize();
        void Cleanup();
    }

    public class Program
    {
        static void Main(string[] args)
        {
            var manager = new NerdManager();
            manager.Load();
            manager.DrawAllNerds(debugMode: true, plotter: new object());

            Console.WriteLine("Press D to show dance instructions, E to email, S to save");
            var key = Console.ReadKey();
            Console.WriteLine();
            if (key.Key == ConsoleKey.D)
            {
                // just pick one nerd
                var genius = new GeniusNerd("ConsoleGenius", 160, 2.5);
                manager.PrintSpecialDanceInstructions(new NerdManager.Nerd { Name = genius.ToString() });
            }
            else if (key.Key == ConsoleKey.E)
            {
                manager.EmailAllNerds();
            }
            else if (key.Key == ConsoleKey.S)
            {
                manager.Save();
            }

            Console.WriteLine("Done.");
        }
    }
}
```

***

## Exercise 1 – Single Responsibility (SRP)

Focus only on **SRP** in this step.[5][1]

1. In comments above `NerdManager`, list at least **four different responsibilities** this class currently has.
2. Identify which logic belongs to:
   - **Domain/model** (nerd properties, IQ, radius).
   - **Persistence** (loading/saving XML, file name).
   - **Rendering/selection/UI** (drawing, color, debug plotting).
   - **Communication** (sending emails).
3. Create **at least three new classes** to separate responsibilities (for example: `Nerd`, `NerdStorage`, `NerdRenderer`, `NerdNotifier`), and move methods/fields accordingly.
4. Ensure `Main` still runs after the split.

Goal: each **class** should have a clear, **single responsibility**, and `Nerd` should not mix geometry, storage, and UI concerns.[1][4]

***

## Exercise 2 – Open/Closed Principle (OCP)

Now address **OCP** around drawing and dancing behaviour.[6][1]

1. Why does `PrintSpecialDanceInstructions` violate OCP?  
   - Explain in comments how `if (n is GeniusNerd)` and `else if (n is ChildNerd)` will hurt extensibility when new nerd types appear.
2. Refactor dance behaviour so you can add new nerd types **without modifying** the method that prints instructions:
   - Option A: give `Mammal` or a new interface (e.g. `INerdDance`) a virtual/abstract `GetDanceInstructions()` and override in each subtype.
   - Option B: introduce strategy objects, e.g. `IDanceStrategy`.
3. Do the same with drawing:
   - Replace the hardcoded IQ-based UI color and selection logic with pluggable **renderer(s)** or **rules** that can be extended by adding new implementations (e.g. `INerdRenderer` list invoked in a `Draw` method, similar to the renderer approach in the article).[4]

Goal: to add new dance or drawing options, you add new implementations, not change existing high-level code.[7][6]

***

## Exercise 3 – Liskov Substitution Principle (LSP)

Now fix **LSP** issues in `Mammal` / `GeniusNerd`.[8][1]

1. In comments above `GeniusNerd.Clone`, explain why `public new GeniusNerd Clone()` breaks substitutability when the object is referenced as `Mammal`.  
2. Replace the `new` clone with a proper **polymorphic** pattern:
   - Make `Clone` in `Mammal` abstract or virtual returning `Mammal`.
   - Override in `GeniusNerd` and other subclasses to return the correct runtime type while preserving the contract.
3. Fix the `isMoonWalking` field:
   - Explain why overriding `OnMouseDown` and arbitrarily flipping `isMoonWalking` can break assumptions from the base class.[3]
   - Make `isMoonWalking` **private** in `Mammal`, and expose only safe behaviour to inheritors (e.g. `protected bool IsMoonWalking { get; }`).
   - Optionally **seal** the mouse handlers and provide safe extension points (events or hook methods) that do not require calling base methods in a fragile way.

Goal: any subclass of `Mammal` should be usable wherever a `Mammal` is expected without surprising behaviour or broken invariants.[8][1]

***

## Exercise 4 – Interface Segregation Principle (ISP)

Now clean up **INerdRepository** and similar “fat” contracts.[9][1]

1. In comments above `INerdRepository`, list which methods belong to:
   - Data access.
   - UI.
   - Lifecycle/bootstrapping.
2. Split `INerdRepository` into **smaller, focused interfaces**, for example:
   - `INerdReader` (read-only).
   - `INerdWriter` (write/save).
   - `INerdHighlighter` (UI-only, if it even belongs).
   - `INerdLifecycle` (if really needed, often moved to composition root/IoC).
3. Update any class that would implement `INerdRepository` so that:
   - It only implements the interfaces it actually needs.
   - UI responsibilities are not forced on data-only implementations.
4. In `NerdManager` (or the class that coordinates everything after your refactor), depend only on the **minimal** interfaces needed for its job.

Goal: no class should be forced to implement methods it cannot logically support; each interface has a **single purpose**.[9][1]

***

## Exercise 5 – Dependency Inversion Principle (DIP)

Finally, fix **DIP** issues in storage and email sending.[6][1]

1. Identify **concrete dependencies** that violate DIP:
   - Direct use of `File`, `XmlSerializer`, `StreamReader/Writer`.
   - Direct `new EmailSender()` in `EmailAllNerds`.
   - Direct `string _storageFile = "nerds.xml"` and platform-specific details.
2. Introduce abstractions:
   - `INerdStorage` for saving/loading nerds (the storage implementation can use XML/file, DB, etc.).
   - `IEmailSender` for sending emails.
   - Optional: an abstraction for storage **format** vs **medium**, as in the article (e.g. `INerdSerializer` vs `INerdStore`).[4]
3. Inject abstractions:
   - Give your coordinating class a constructor like `public NerdManager(INerdStorage storage, IEmailSender emailSender, INerdRenderer renderer, ...)`.
   - In `Main`, create concrete implementations and pass them in.
4. Make your logic testable:
   - Ensure `NerdManager` can be unit-tested with **fake** implementations of `INerdStorage` and `IEmailSender` without touching disk or console.

Goal: high-level policy depends on **interfaces**, not on specific file formats or platforms; platform-specific code is pushed to the edges.[1][6]

***

solid-principles-c-simple-examples-ghadi-%E0%A4%B8%E0%A4%9A-%E0%A4%A8-%E0%A4%98-%E0%A4%A1--yzhwf)
[16](https://www.reddit.com/r/programming/comments/maa1dt/why_every_single_element_of_solid_is_wrong/)
[17](https://stackoverflow.com/questions/74488175/refactoring-code-using-solid-principles-and-design-patterns)
[18](https://www.c-sharpcorner.com/forums/refactor-using-solid-principles)
[19](https://www.telerik.com/blogs/10-awesome-csharp-refactoring-tips)
[20](https://www.infoq.com/SOLID/news/)
