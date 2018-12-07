//#define TERSE
#define VERBOSE
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Newtonsoft.Json;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;

namespace ExamPrepConsoleApp
{
    public class Alarm
    {
        // Delegate for the alarm event
        public event EventHandler<AlarmEventArgs> OnAlarmRaised = delegate { };

        // Called to raise an alarm
        public void RaiseAlarm(string location)
        {
            var exceptionList = new List<Exception>();

            foreach (Delegate handler in OnAlarmRaised.GetInvocationList())
            {
                try
                {
                    handler.DynamicInvoke(this, new AlarmEventArgs(location));
                }
                catch (TargetInvocationException e)
                {
                    exceptionList.Add(e.InnerException);
                }
            }
            if (exceptionList.Count > 0)
                throw new AggregateException(exceptionList);
        }
    }

    public class AlarmEventArgs : EventArgs
    {
        public string Location { get; set; }

        public AlarmEventArgs(string location)
        {
            Location = location;
        }
    }

    public class Miles
    {
        public double Distance { get; }

        // Conversion operator for implicit conversation to Kilometers
        public static implicit operator Kilometers(Miles t)
        {
            Console.WriteLine("Implicit coversion from miles to kilometers");
            return new Kilometers(t.Distance * 1.6);
        }

        public static explicit operator int(Miles t)
        {
            Console.WriteLine("Explicit coversion from miles to int");
            return (int)(t.Distance + 0.5);
        }

        public Miles(double miles)
        {
            Distance = miles;
        }
    }

    public class Kilometers
    {
        public double Distance { get; }

        public Kilometers(double kilometers)
        {
            Distance = kilometers;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property)]
    class ProgrammerAttribute : Attribute
    {
        private readonly string programmerValue;

        public ProgrammerAttribute(string programmer)
        {
            programmerValue = programmer;
        }

        public string Programmer { get => programmerValue; }
    }

    [Serializable, Programmer(programmer: "Bryan")]
    public class Person
    {
        [Programmer(programmer: "Bryan")]
        public string Name { get; set; }
        public int Age { get; }

        [NonSerialized]
        // No need to save this
        private readonly int _screenPosition;

        public Person(string name, int age)
        {
            Name = name;
            Age = age;
            _screenPosition = 0;
        }
    }

    public class MultiplyToAdd : ExpressionVisitor
    {
        public Expression Modify(Expression expression) => Visit(expression);

        protected override Expression VisitBinary(BinaryExpression b)
        {
            if (b.NodeType == ExpressionType.Multiply)
            {
                Expression left = this.Visit(b.Left);
                Expression right = this.Visit(b.Right);

                // Make this binary expression an Add rather than a multiply operation.
                return Expression.Add(left, right);
            }

            return base.VisitBinary(b);
        }
    }

    public class Calculator
    {
        public int AddInt(int v1, int v2)
        {
            return v1 + v2;
        }
    }

    public class MusicTrack
    {
        public string Artist { get; set; }
        public string Title { get; set; }
        public int Length { get; set; }

        // ToString that overrides the behavior in the base class
        public override string ToString()
        {
            return $"{Artist} {Title} {Length.ToString()} seconds long";
        }

        public MusicTrack(string artist, string title, int length)
        {
            Artist = artist;
            Title = title;
            Length = length;
        }
    }

    class Program
    {
        //make an array that holds the values 0 to 50000000
        static int[] items = Enumerable.Range(0, 50000001).ToArray();
        static long sharedTotal;
        static object sharedTotalLock = new object();
        static object lock1 = new object();
        static object lock2 = new object();
        delegate int IntOperation(int a, int b);
        delegate int GetValue();
        static GetValue getLocalInt;

        // static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        // static CancellationToken cancellationToken = new CancellationToken();

        static void Main(string[] args)
        {
            CodeCompileUnit compileUnit = new CodeCompileUnit();

            // Create a namespace to hold the types we are going to create
            CodeNamespace personnelNameSpace = new CodeNamespace("Personnel");

            // Import the system namespace
            personnelNameSpace.Imports.Add(new CodeNamespaceImport("System"));
            // Create a Person class
            CodeTypeDeclaration personClass = new CodeTypeDeclaration("Person");
            personClass.IsClass = true;
            personClass.TypeAttributes = System.Reflection.TypeAttributes.Public;
            // Add the Person class to personnelNamespace
            personnelNameSpace.Types.Add(personClass);

            // Create a field to hold the name of a person
            CodeMemberField nameField = new CodeMemberField("String", "name");
            nameField.Attributes = MemberAttributes.Private;

            // Add the name field to the Person class
            personClass.Members.Add(nameField);

            // Add the namespace to the document
            compileUnit.Namespaces.Add(personnelNameSpace);

            // Create a provider to parse the document
            CodeDomProvider provider = CodeDomProvider.CreateProvider("C-Sharp");
            // Give the provider somewhere to send the parsed output
            StringWriter s = new StringWriter();
            // Set some options for the parse - we can use the defaults
            CodeGeneratorOptions options = new CodeGeneratorOptions();

            // Generate the C# source from the CodeDOM
            provider.GenerateCodeFromCompileUnit(compileUnit, s, options);
            s.Close();

            Console.WriteLine(s.ToString());

            EndProgram();
        }

        [Conditional("VERBOSE"), Conditional("TERSE")]
        static void reportHeader()
        {
            Console.WriteLine("This is the header for the report");
        }

        [Conditional("VERBOSE")]
        static void verboseReport()
        {
            Console.WriteLine("This is the output from the verbose report");
        }

        [Conditional("TERSE")]
        static void terseReport()
        {
            Console.WriteLine("This is the output from the terse report");
        }

        static void SetLocalInt()
        {
            // Local variable set to 99
            int localInt = 99;

            // Set delegate getLocalInt to a lambda expression that
            // returns the value of localInt
            getLocalInt = () => localInt;
        }

        public static int Add(int a, int b)
        {
            Console.WriteLine("Add called");
            return a + b;
        }

        public static int Subtract(int a, int b)
        {
            Console.WriteLine("Subtract called");
            return a - b;
        }

        // Method that must run when the alarm is raised
        // Only the sender is valid as this event doesn't have arguments
        private static void AlarmListener1(object sender, AlarmEventArgs args)
        {
            Console.WriteLine("Alarm listener 1 called");
            Console.WriteLine($"Alarm in {args.Location}");
            throw new Exception("Bang");
        }

        // Method that must run when the alarm is raised
        private static void AlarmListener2(object sender, AlarmEventArgs args)
        {
            Console.WriteLine("Alarm listener 2 called");
            Console.WriteLine($"Alarm in {args.Location}");
            throw new Exception("Boom");
        }

        static void Clock(CancellationToken cancellationToken)
        {
            int tickCount = 0;
            while (!cancellationToken.IsCancellationRequested && tickCount < 20)
            {
                tickCount++;
                Console.WriteLine("Tick");
                Thread.Sleep(500);
            }
            cancellationToken.ThrowIfCancellationRequested();
        }

        static void Method1()
        {
            lock (lock1)
            {
                Console.WriteLine("Method 1 got lock 1");
                Console.WriteLine("Method 1 waiting for lock 2");
                lock (lock2)
                {
                    Console.WriteLine("Method 1 got lock 2");
                }
                Console.WriteLine("Method 1 released lock 2");
            }
            Console.WriteLine("Method 1 released lock 1");
        }

        static void Method2()
        {
            lock (lock2)
            {
                Console.WriteLine("Method 2 got lock 2");
                Console.WriteLine("Method 2 waiting for lock 1");
                lock (lock1)
                {
                    Console.WriteLine("Method 2 got lock 1");
                }
                Console.WriteLine("Method 2 released lock 1");
            }
            Console.WriteLine("Method 2 released lock 2");
        }

        static void AddRangeOfValues(int start, int end)
        {
            long subTotal = 0;
            while (start < end)
            {
                subTotal = subTotal + items[start];
                start++;
            }

            Interlocked.Add(ref sharedTotal, subTotal);
            // Monitor.Enter(sharedTotalLock);
            // try
            // {
            //     sharedTotal = sharedTotal + subTotal;
            // }
            // finally
            // {
            //     Monitor.Exit(sharedTotalLock);
            // }
        }

        private static void EndProgram()
        {
            Console.WriteLine("Press a key to exit.");
            Console.ReadKey();
        }

        static void DoWork(object state)
        {
            Console.WriteLine($"Doing work: {state}");
            Thread.Sleep(500);
            Console.WriteLine($"Work finished: {state}");
        }

        static void DisplayThread(Thread t)
        {
            Console.WriteLine($"Name: {t.Name}");
            Console.WriteLine($"Culture: {t.CurrentCulture}");
            Console.WriteLine($"Priority: {t.Priority}");
            Console.WriteLine($"Context: {t.ExecutionContext}");
            Console.WriteLine($"IsBackground?: {t.IsBackground}");
            Console.WriteLine($"IsPool?: {t.IsThreadPoolThread}");
        }

        public static ThreadLocal<Random> RandomGenerator = new ThreadLocal<Random>(() => { return new Random(2); });

        static void ThreadHello()
        {
            Console.WriteLine("Hello from the thread");
            Thread.Sleep(2000);
        }

        static void WorkOnData(object data)
        {
            Console.WriteLine($"Working on: {data}");
            Thread.Sleep(1000);
        }
    }
}