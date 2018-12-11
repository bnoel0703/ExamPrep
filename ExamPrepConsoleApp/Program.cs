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
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;

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

        public MusicTrack() {}
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

            EndProgram();
        }

        private static void EncryptWithRSA() // Listing 3-16
        {
            string plainText = "This is my super secret data";
            Console.WriteLine($"Plain text: {plainText}");

            // RSA works on byte arrays, not strings of text
            // This will convert our input string into bytes and back

            ASCIIEncoding converter = new ASCIIEncoding();

            // Convert the plain text into a byte array
            byte[] plainBytes = converter.GetBytes(plainText);

            DumpBytes("Plain bytes:", plainBytes);

            byte[] encryptedBytes;
            byte[] decryptedBytes;

            // Create a new RSA to encrypt the data
            // should be wrapped in using for production code
            RSACryptoServiceProvider rsaEncrypt = new RSACryptoServiceProvider();

            // get the keys out of the encryptor
            string publicKey = rsaEncrypt.ToXmlString(includePrivateParameters: false);
            Console.WriteLine($"Public key: {publicKey}");
            string privateKey = rsaEncrypt.ToXmlString(includePrivateParameters: true);
            Console.WriteLine($"Private key: {privateKey}");

            // Now tell the encryptor to use the public key to encrypt the data
            rsaEncrypt.FromXmlString(privateKey);

            // Use the encryptor to encrypt the data. the fOAEP parameter
            // specifies how the output is "padded" with extra bytes
            // For maximum compatibility with receiving systems, set this as false
            encryptedBytes = rsaEncrypt.Encrypt(plainBytes, fOAEP: false);

            DumpBytes("Encrypted bytes: ", encryptedBytes);

            // Now do the decode - use the private key for this
            // We have sent someone our public key and they
            // have used this to encrypt data that they are sending to us

            // Create a new RSA to decrypt the data
            // should be wrapped in using for production code
            RSACryptoServiceProvider rsaDecrypt = new RSACryptoServiceProvider();

            // Configure the decryptor from the XML in the private key
            rsaDecrypt.FromXmlString(privateKey);

            decryptedBytes = rsaDecrypt.Decrypt(encryptedBytes, fOAEP: false);

            DumpBytes("Decrypted bytes: ", decryptedBytes);
            Console.WriteLine($"Decrypted string: {converter.GetString(decryptedBytes)}");
        }

        private static string DecryptWithAes(byte[] cipherText, byte[] key, byte[] initializationVector)
        {
            string decryptedText;
            using (Aes aes = Aes.Create())
            {
                // Configure the aes instances with the key and
                // initialization vector to use for the decryption
                aes.Key = key;
                aes.IV = initializationVector;

                // Create the decryptor from the aes
                // should be wrapped in using for production code
                ICryptoTransform decryptor = aes.CreateDecryptor();

                using (MemoryStream decryptStream = new MemoryStream(cipherText))
                {
                    using (CryptoStream decryptCryptoStream = new CryptoStream(decryptStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(decryptCryptoStream))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            decryptedText = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return decryptedText;
        }

        private static void EncryptWithAes(string plainText, out byte[] cipherText, out byte[] key, out byte[] initializationVector)
        {
            using (Aes aes = Aes.Create())
            {
                // copy the key and the initialization vector
                key = aes.Key;
                initializationVector = aes.IV;

                // create an encryptor to encrypt some data
                // should be wrapped in using for production code
                ICryptoTransform encryptor = aes.CreateEncryptor();


                // Create a new memory stream to receive the encrypted data
                using (MemoryStream encryptMemoryStream = new MemoryStream())
                {
                    // create a CryptoStream, tell it the stream to write to
                    // and the encryptor to use. Also set the mode
                    using (CryptoStream encryptCryptoStream = new CryptoStream(encryptMemoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        // make a stream writer from the cryptostream
                        using (StreamWriter swEncrypt = new StreamWriter(encryptCryptoStream))
                        {
                            // Write the secret message to the stream.
                            swEncrypt.Write(plainText);
                        }
                        // get the encrypted message from the stream
                        cipherText = encryptMemoryStream.ToArray();
                    }
                }
            }
        }

        static void DumpBytes(string title, byte[] bytes)
        {
            Console.Write(title);
            foreach(var b in bytes)
                Console.WriteLine($"{b:X}");
            Console.WriteLine();
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