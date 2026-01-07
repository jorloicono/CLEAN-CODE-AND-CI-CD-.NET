using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;

namespace NerdLab
{
    // EXERCISE 1 – SRP
    // Original NerdManager had at least these responsibilities:
    // 1) Domain/model management (holding list of nerds, selection state, geometry, IQ, etc.).
    // 2) Persistence (loading/saving to XML file, knowing "nerds.xml" path).
    // 3) Rendering / UI logic (drawing nerds, choosing colors, debug plotting, console output).
    // 4) Communication (emailing all nerds).
    // 5) Utility / math (FitNerdsIntoPaddedRoom).
    //
    // Logic classification:
    // - Domain/model: Nerd properties (Name, IQ, Radius, SuspenderTension, Position, Heading, etc.).
    // - Persistence: XmlSerializer usage, File.Exists, StreamReader/Writer, _storageFile name.
    // - Rendering/selection/UI: DrawAllNerds, selection based on IQ, UiColorHex, plotter, console drawing.
    // - Communication: EmailAllNerds and EmailSender.
    //
    // After refactor, NerdManager becomes an orchestration/service class with a single
    // responsibility: coordinate operations on nerds using injected abstractions (storage, email, rendering).

    #region Domain model

    // Domain entity: only domain-related data here (no persistence path, no UI color, no image)
    public class Nerd
    {
        public string Name { get; set; }
        public int IQ { get; set; }
        public double SuspenderTension { get; set; }
        public double Radius { get; set; }

        // Geometry belongs to domain (nerds have a position and heading in the room)
        public Point Position { get; set; }
        public double Heading { get; set; }

        // Selection flag is domain-ish (used by higher-level rules)
        public bool IsSelected { get; set; }
    }

    #endregion

    #region Persistence abstractions (DIP, ISP)

    // EXERCISE 5 – DIP
    // Identify concrete dependencies:
    // - Direct File, XmlSerializer, StreamReader/Writer, fixed "nerds.xml" string.
    // - Direct new EmailSender().
    // Introduce abstractions and inject them.

    public interface INerdStorage
    {
        IList<Nerd> Load();
        void Save(IList<Nerd> nerds);
    }

    // Optional split for format vs medium could be added (INerdSerializer / INerdStore),
    // but for brevity a single INerdStorage is used.

    public class XmlFileNerdStorage : INerdStorage
    {
        private readonly string _filePath;

        public XmlFileNerdStorage(string filePath)
        {
            _filePath = filePath;
        }

        public IList<Nerd> Load()
        {
            if (!File.Exists(_filePath))
            {
                // Seed default nerds if no file.
                return new List<Nerd>
                {
                    new Nerd { Name = "Alice", IQ = 130, Radius = 10, SuspenderTension = 1.2 },
                    new Nerd { Name = "Bob", IQ = 90, Radius = 15, SuspenderTension = 0.8 }
                };
            }

            var serializer = new XmlSerializer(typeof(List<Nerd>));
            using (var reader = new StreamReader(_filePath))
            {
                var result = (List<Nerd>)serializer.Deserialize(reader);
                return result;
            }
        }

        public void Save(IList<Nerd> nerds)
        {
            var serializer = new XmlSerializer(typeof(List<Nerd>));
            using (var writer = new StreamWriter(_filePath))
            {
                serializer.Serialize(writer, nerds);
            }
        }
    }

    #endregion

    #region Email abstractions (DIP)

    public interface IEmailSender
    {
        void Send(string body, string to);
    }

    public class ConsoleEmailSender : IEmailSender
    {
        public void Send(string body, string to)
        {
            Console.WriteLine("Sending email to " + to + " => " + body);
        }
    }

    #endregion

    #region Rendering and drawing (SRP, OCP)

    // EXERCISE 2 – OCP (drawing)
    // Original DrawAllNerds violated OCP:
    // - It had hardcoded rules: selection (IQ > 100), color rules, debug plotter, console drawing.
    // - To change drawing rules or add new ones, we needed to modify DrawAllNerds, not extend it.
    //
    // Here, drawing is delegated to pluggable renderers and rules.

    public interface INerdDrawingContext
    {
        bool DebugMode { get; }
        object Plotter { get; }
    }

    public class ConsoleDrawingContext : INerdDrawingContext
    {
        public bool DebugMode { get; }
        public object Plotter { get; }

        public ConsoleDrawingContext(bool debugMode, object plotter)
        {
            DebugMode = debugMode;
            Plotter = plotter;
        }
    }

    public interface INerdRule
    {
        void Apply(Nerd nerd, INerdDrawingContext context);
    }

    // Selection rule based on IQ.
    public class IqSelectionRule : INerdRule
    {
        public void Apply(Nerd nerd, INerdDrawingContext context)
        {
            nerd.IsSelected = nerd.IQ > 100;
        }
    }

    // Rendering rule: console drawing and optional plotter debug.
    public class ConsoleRenderRule : INerdRule
    {
        public void Apply(Nerd nerd, INerdDrawingContext context)
        {
            Console.WriteLine("Drawing nerd " + nerd.Name + " at " + nerd.Position);

            if (context.Plotter != null && context.DebugMode)
            {
                Console.WriteLine("Plotter debug circle PeachPuff radius=" + nerd.Radius);
            }
        }
    }

    // Color rule: maps IQ to UI color (decoupled from Nerd entity).
    public interface INerdColorStrategy
    {
        string GetColor(Nerd nerd);
    }

    public class IqBasedColorStrategy : INerdColorStrategy
    {
        public string GetColor(Nerd nerd)
        {
            if (nerd.IQ > 120)
            {
                return "#FFD700"; // gold
            }
            if (nerd.IQ < 95)
            {
                return "#808080"; // gray
            }
            return "#FFFFFF"; // default white
        }
    }

    public class ColorRenderRule : INerdRule
    {
        private readonly INerdColorStrategy _colorStrategy;

        public ColorRenderRule(INerdColorStrategy colorStrategy)
        {
            _colorStrategy = colorStrategy;
        }

        public void Apply(Nerd nerd, INerdDrawingContext context)
        {
            var color = _colorStrategy.GetColor(nerd);
            Console.WriteLine("Applying UI color " + color + " to " + nerd.Name);
        }
    }

    public interface INerdRenderer
    {
        void DrawAll(IList<Nerd> nerds, INerdDrawingContext context);
    }

    public class CompositeNerdRenderer : INerdRenderer
    {
        private readonly IList<INerdRule> _rules;

        public CompositeNerdRenderer(IList<INerdRule> rules)
        {
            _rules = rules;
        }

        public void DrawAll(IList<Nerd> nerds, INerdDrawingContext context)
        {
            Console.WriteLine("Drawing nerds (debug=" + context.DebugMode + ")");
            foreach (var nerd in nerds)
            {
                foreach (var rule in _rules)
                {
                    rule.Apply(nerd, context);
                }
            }
        }
    }

    #endregion

    #region Dance behaviour (OCP, LSP)

    // EXERCISE 3 – LSP
    // EXERCISE 2 – OCP (dance)

    public abstract class Mammal
    {
        // EXERCISE 3 – LSP: make state private and expose via protected property
        // so subclasses cannot arbitrarily break invariants.
        private bool _isMoonWalking;

        protected bool IsMoonWalking
        {
            get => _isMoonWalking;
            private set => _isMoonWalking = value;
        }

        protected int Vertebrae { get; set; }

        // EXERCISE 3 – LSP: Clone should be polymorphic.
        // Subclasses must override it to return the correct runtime type,
        // but callers use Mammal as the static type.
        public abstract Mammal Clone();

        // Base contract: OnMouseDown starts moonwalk, OnMouseUp stops it once.
        // To keep this invariant, these methods are sealed so subclasses
        // cannot arbitrarily flip IsMoonWalking and break expectations.
        public void OnMouseDown()
        {
            IsMoonWalking = true;
            OnMouseDownCore();
        }

        public void OnMouseUp()
        {
            if (IsMoonWalking)
            {
                Console.WriteLine("Stopping moonwalk.");
                IsMoonWalking = false;
            }
            OnMouseUpCore();
        }

        // Extension points that allow additional behaviour without breaking invariants.
        protected virtual void OnMouseDownCore() { }
        protected virtual void OnMouseUpCore() { }

        // EXERCISE 2 – OCP (dance):
        // Instead of type checks (if GeniusNerd / ChildNerd), each subtype provides its
        // own dance instructions. Adding a new nerd type does not change higher-level code.
        public abstract string GetDanceInstructions();
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
            Vertebrae = 33;
        }

        // EXERCISE 3 – LSP:
        // Original code had: public new GeniusNerd Clone()
        // This breaks substitutability because:
        // - If a GeniusNerd is referenced as Mammal, calling mammal.Clone() would call
        //   Mammal.Clone (returning a Mammal) instead of the "new" GeniusNerd.Clone,
        //   so the behaviour changes depending on the static type of the reference.
        // - Clients expecting polymorphic clone on Mammal would not get a proper GeniusNerd clone.
        //
        // Correct version: override abstract Clone and return Mammal, but actually
        // construct and return a GeniusNerd instance, preserving runtime type.
        public override Mammal Clone()
        {
            return new GeniusNerd(Name, IQ, Diopter);
        }

        protected override void OnMouseDownCore()
        {
            // Additional safe behaviour; base has already set IsMoonWalking = true.
            if (IQ > 150)
            {
                Console.WriteLine(Name + " starts moonwalk with extra style.");
            }
        }

        public override string GetDanceInstructions()
        {
            return "Do complex disco with moonwalk.";
        }
    }

    public class ChildNerd : Mammal
    {
        public string Nick { get; set; }

        public override Mammal Clone()
        {
            return new ChildNerd { Nick = this.Nick };
        }

        protected override void OnMouseDownCore()
        {
            Console.WriteLine(Nick + " is happily jumping.");
        }

        public override string GetDanceInstructions()
        {
            // Child can dance in a safe, appropriate way instead of throwing.
            return "Do simple, child-friendly dance.";
        }
    }

    // EXERCISE 2 – OCP
    // Original PrintSpecialDanceInstructions violated OCP because:
    // - It used explicit type checks: if (n is GeniusNerd) / else if (n is ChildNerd).
    // - Adding a new nerd type (e.g. SeniorNerd) would force modification of this method
    //   to add another branch, instead of being closed for modification.
    //
    // Now this class just relies on the polymorphic GetDanceInstructions implementation.
    public interface INerdDancePrinter
    {
        void PrintDanceInstructions(Mammal nerd);
    }

    public class ConsoleNerdDancePrinter : INerdDancePrinter
    {
        public void PrintDanceInstructions(Mammal nerd)
        {
            Console.WriteLine("Dance instructions:");
            Console.WriteLine(nerd.GetDanceInstructions());
        }
    }

    #endregion

    #region Room fitting utility (SRP)

    public interface INerdRoomFitter
    {
        IList<Nerd> FitIntoRoom(IList<Nerd> nerds, IList<Point> boundary);
    }

    public class SimpleNerdRoomFitter : INerdRoomFitter
    {
        public IList<Nerd> FitIntoRoom(IList<Nerd> nerds, IList<Point> boundary)
        {
            Console.WriteLine("Fitting nerds into padded room with " + boundary.Count + " points");
            foreach (var n in nerds)
            {
                if (boundary.Count > 0)
                {
                    n.Position = boundary[0];
                }
            }
            return nerds;
        }
    }

    #endregion

    #region Interface Segregation (ISP)

    // EXERCISE 4 – ISP
    // Original INerdRepository mixed:
    // - Data access: LoadAll, SaveAll
    // - UI: HighlightOnScreen
    // - Lifecycle: Initialize, Cleanup
    //
    // Split into focused interfaces so implementations are not forced to support
    // unrelated responsibilities.

    public interface INerdReader
    {
        IList<Nerd> LoadAll();
    }

    public interface INerdWriter
    {
        void SaveAll(IList<Nerd> nerds);
    }

    public interface INerdHighlighter
    {
        void HighlightOnScreen(Nerd nerd);
    }

    public interface INerdLifecycle
    {
        void Initialize();
        void Cleanup();
    }

    // Example concrete type that only deals with data and lifecycle, not UI.
    public class NerdRepository :
        INerdReader,
        INerdWriter,
        INerdLifecycle
    {
        private readonly INerdStorage _storage;
        private IList<Nerd> _cache;

        public NerdRepository(INerdStorage storage)
        {
            _storage = storage;
        }

        public void Initialize()
        {
            _cache = _storage.Load();
        }

        public void Cleanup()
        {
            _storage.Save(_cache);
        }

        public IList<Nerd> LoadAll()
        {
            return _cache;
        }

        public void SaveAll(IList<Nerd> nerds)
        {
            _cache = nerds;
            _storage.Save(_cache);
        }
    }

    // UI-only implementation, e.g. for highlighting in some graphical UI.
    public class ConsoleNerdHighlighter : INerdHighlighter
    {
        public void HighlightOnScreen(Nerd nerd)
        {
            Console.WriteLine("Highlighting nerd on screen: " + nerd.Name);
        }
    }

    #endregion

    #region Coordinator / NerdManager (now SRP + DIP- friendly)

    public class NerdManager
    {
        private readonly INerdReader _reader;
        private readonly INerdWriter _writer;
        private readonly INerdRenderer _renderer;
        private readonly IEmailSender _emailSender;
        private readonly INerdRoomFitter _roomFitter;

        private readonly List<Nerd> _nerds = new List<Nerd>();

        public NerdManager(
            INerdReader reader,
            INerdWriter writer,
            INerdRenderer renderer,
            IEmailSender emailSender,
            INerdRoomFitter roomFitter)
        {
            _reader = reader;
            _writer = writer;
            _renderer = renderer;
            _emailSender = emailSender;
            _roomFitter = roomFitter;
        }

        public void Load()
        {
            Console.WriteLine("Loading nerds...");
            _nerds.Clear();
            _nerds.AddRange(_reader.LoadAll());
        }

        public void Save()
        {
            Console.WriteLine("Saving nerds...");
            _writer.SaveAll(_nerds);
        }

        public void DrawAllNerds(bool debugMode, object plotter = null)
        {
            var context = new ConsoleDrawingContext(debugMode, plotter);
            _renderer.DrawAll(_nerds, context);
        }

        public void EmailAllNerds()
        {
            Console.WriteLine("Emailing nerds...");
            foreach (var n in _nerds)
            {
                var address = n.Name + "@example.com";
                _emailSender.Send("Hello " + n.Name + ", welcome to NerdLab!", address);
            }
        }

        public void FitNerdsIntoPaddedRoom(IList<Point> boundary)
        {
            _roomFitter.FitIntoRoom(_nerds, boundary);
        }

        public IList<Nerd> GetNerds()
        {
            return _nerds;
        }
    }

    #endregion

    #region Program

    public class Program
    {
        static void Main(string[] args)
        {
            // Wiring (Composition Root) – here we create concrete implementations
            // and pass them into NerdManager (DIP).
            var storage = new XmlFileNerdStorage("nerds.xml");
            var repository = new NerdRepository(storage);

            var rules = new List<INerdRule>
            {
                new IqSelectionRule(),
                new ConsoleRenderRule(),
                new ColorRenderRule(new IqBasedColorStrategy())
            };
            var renderer = new CompositeNerdRenderer(rules);

            var emailSender = new ConsoleEmailSender();
            var fitter = new SimpleNerdRoomFitter();

            var manager = new NerdManager(
                reader: repository,
                writer: repository,
                renderer: renderer,
                emailSender: emailSender,
                roomFitter: fitter);

            // Initialize repository (ISP – lifecycle separated).
            repository.Initialize();

            manager.Load();
            manager.DrawAllNerds(debugMode: true, plotter: new object());

            Console.WriteLine("Press D to show dance instructions, E to email, S to save");
            var key = Console.ReadKey();
            Console.WriteLine();
            if (key.Key == ConsoleKey.D)
            {
                // Use polymorphic dance behaviour (no type checks).
                var dancePrinter = new ConsoleNerdDancePrinter();
                Mammal genius = new GeniusNerd("ConsoleGenius", 160, 2.5);
                dancePrinter.PrintDanceInstructions(genius);
            }
            else if (key.Key == ConsoleKey.E)
            {
                manager.EmailAllNerds();
            }
            else if (key.Key == ConsoleKey.S)
            {
                manager.Save();
            }

            // Example of room fitting.
            manager.FitNerdsIntoPaddedRoom(new List<Point> { new Point(10, 10) });

            // Cleanup repository if needed.
            repository.Cleanup();

            Console.WriteLine("Done.");
        }
    }

    #endregion
}
