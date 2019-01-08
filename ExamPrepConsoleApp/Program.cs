﻿//#define TERSE
#define VERBOSE
//#undef DEBUG
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
using System.Security.Cryptography.X509Certificates;

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

    public class ResourceHolder : IDisposable
    {
        // Flag to indicate when the object has been disposed
        bool disposed = false;

        public void Dispose()
        {
            // Call dispose and tell it that it is being called from a Dispose call
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            // Give up if already disposed
            if (disposed)
                return;

            if (disposing)
            {
                // free any managed objects here
            }

            // Free any unmanaged objects here
        }

        ~ResourceHolder()
        {
            // Dispose only of unmanaged objects here
            Dispose(false);
        }
    }
    public class MusicTrack
    {
        public static bool DebugMode = false;
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

#if DIAGNOSTICS
            Console.WriteLine($"Music track created: {this.ToString()}");

#endif
        }

        public MusicTrack() { }
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

        private static void SourceSwitch() // Listing 3-40
        {
            TraceSource trace = new TraceSource("Tracer", SourceLevels.All);
            SourceSwitch control = new SourceSwitch("control", "Controls the tracing")
            {
                Level = SourceLevels.Information
            };
            trace.Switch = control;

            trace.TraceEvent(TraceEventType.Start, 10000);
            trace.TraceEvent(TraceEventType.Warning, 10001);
            trace.TraceEvent(TraceEventType.Verbose, 10002, "At the end of the program");
            trace.TraceEvent(TraceEventType.Information, 10003, "Some information",
            new object[] { "line1", "line2" });
            trace.Flush();
            trace.Close();
        }

        private static void TraceSwitch() // Listing 3-38
        {
            TraceSwitch control = new TraceSwitch("Control", "Control the trace output");
            control.Level = TraceLevel.Warning;

            if (control.TraceError)
                Console.WriteLine("An error has occured");
            Trace.WriteLineIf(control.TraceWarning, "A warning message");
        }

        private static void SimpleTraceSource() // Listing 3-37
        {
            TraceSource trace = new TraceSource("Tracer", SourceLevels.All);
            trace.TraceEvent(TraceEventType.Start, 10000);
            trace.TraceEvent(TraceEventType.Warning, 10001);
            trace.TraceEvent(TraceEventType.Verbose, 10002, "At the end of the program");
            trace.TraceData(TraceEventType.Information, 1003, new object[] { "Note 1", "Message 2" });
            trace.Flush();
            trace.Close();
        }

        private static void TraceListener() // Listing 3-36
        {
            TraceListener consoleListener = new ConsoleTraceListener();
            Trace.Listeners.Add(consoleListener);
            Trace.TraceInformation("This is an information message");
            Trace.TraceWarning("This is a warning message");
            Trace.TraceError("This is an error message");
        }

        private static void DebugAssertions() // Listing 3-35
        {
            string customerName = "Bryan";
            Debug.Assert(!string.IsNullOrWhiteSpace(customerName));

            customerName = "";
            Debug.Assert(!string.IsNullOrWhiteSpace(customerName));
        }

        private static void TraceCodeTracing() // Listing 3-34
        {
            Trace.WriteLine("Starting the program");
            Trace.TraceInformation("This is an information message");
            Trace.TraceWarning("This is a warning message");
            Trace.TraceError("This is an error message");
        }

        private static void DebugCodeTracing() // Listing 3-33
        {
            Debug.WriteLine("Starting the program");
            Debug.Indent();
            Debug.WriteLine("Inside a function");
            Debug.Unindent();
            Debug.WriteLine("Outside a function");
            string customerName = "Bryan";
            Debug.WriteLineIf(string.IsNullOrEmpty(customerName), "The name is empty");
        }

        [Conditional("DEBUG")]
        static void display(string message)
        {
            Console.WriteLine(message);
        }

        private static void StrongNames() // Listing 3-26
        {
            string assemblyName = typeof(MusicTrack).Assembly.FullName;
            Console.WriteLine(assemblyName);
        }

        private static void Assemblies() // Listing 3-25
        {
            MusicTrack m = new MusicTrack(artist: "Rob Miles", title: "My Way", length: 150);
            Console.WriteLine(m);
        }

        private static void StreamEncrypt() // Listing 3-24
        {
            string plainText = "This is my super super secret data";

            // byte array to hold the encrypted message
            byte[] encryptedText;

            // byte arrays to hold the key that was used for encryption
            byte[] key1;
            byte[] key2;

            // byte array to hold the initialization vector that was used for encryption
            byte[] initilizationVector1;
            byte[] initilizationVector2;

            using (Aes aes1 = Aes.Create())
            {
                // copy the key and the initializtion vector
                key1 = aes1.Key;
                initilizationVector1 = aes1.IV;

                // create an encryptor to encrypt some data
                ICryptoTransform encryptor1 = aes1.CreateEncryptor();

                // Create a new memory stream to receive the encrypted data.

                using (MemoryStream encryptMemoryStream = new MemoryStream())
                {
                    // create a CryptoStream, tell it the stream to write to and the encryptor to use
                    // also set the mode
                    using (CryptoStream cryptoStream1 = new CryptoStream(encryptMemoryStream, encryptor1, CryptoStreamMode.Write))
                    {
                        // Add another layer of encryption
                        using (Aes aes2 = Aes.Create())
                        {
                            // copy the key and the initlization vector
                            key2 = aes2.Key;
                            initilizationVector2 = aes2.IV;

                            ICryptoTransform encryptor2 = aes2.CreateEncryptor();

                            using (CryptoStream cryptostream2 = new CryptoStream(cryptoStream1, encryptor2, CryptoStreamMode.Write))
                            {
                                using (StreamWriter swEncrypt = new StreamWriter(cryptostream2))
                                {
                                    // Write the secret message to the stream.
                                    swEncrypt.Write(plainText);
                                }
                                // get the encrypted message from the stream
                                encryptedText = encryptMemoryStream.ToArray();
                            }
                        }
                    }
                }
            }
        }

        static byte[] CalculateHash(string source) // Listing 3-23
        {
            // This will convert our input string into bytes and back
            ASCIIEncoding converter = new ASCIIEncoding();
            byte[] sourceBytes = converter.GetBytes(source);

            HashAlgorithm hasher = SHA256.Create();
            byte[] hash = hasher.ComputeHash(sourceBytes);
            return hash;
        }

        static void ShowHash(string source) // Listing 3-23
        {
            Console.WriteLine($"Hash for {source} is: ");

            byte[] hash = CalculateHash(source);

            foreach (byte b in hash)
                Console.WriteLine($"{b:X}");
            Console.WriteLine();
        }

        static void ShowHash(object source) // Listing 3-22
        {
            Console.WriteLine($"Hash for {source} is: {source.GetHashCode():X}");
        }

        private static int CalculateChecksum(string source) // Listing 3-21
        {
            int total = 0;
            foreach (char ch in source)
            {
                total = total + (int)ch;
            }
            return total;
        }

        private static void ShowChecksum(string source) // Listing 3-21
        {
            Console.WriteLine($"Checksum for {source} is {CalculateChecksum(source)}");
        }

        private static void SignData() // Listing 3-20
        {
            // This will convert our input string into bytes and back

            ASCIIEncoding converter = new ASCIIEncoding();

            // Get a crypto provider out of the certificate store
            // should be wrapped in using for production code
            X509Store store = new X509Store("demoCertStore", StoreLocation.CurrentUser);

            store.Open(OpenFlags.ReadOnly);

            // should be wrapped in using for production code
            X509Certificate2 certificate = store.Certificates[0];

            // should be wrapped in using for production code
            RSACryptoServiceProvider encryptProvider = certificate.PrivateKey as RSACryptoServiceProvider;

            string messageToSign = "This is the message I want to sign";
            Console.WriteLine($"Message: {messageToSign}");

            byte[] messageToSignBytes = converter.GetBytes(messageToSign);
            DumpBytes("Message to sign in bytes: ", messageToSignBytes);

            // need to calculate a hash for this message - this will go into the
            // signature and be used to verify the message
            // Create an implementation of the hashing algorithm we are going to use
            // should be wrapped in using for production code
            HashAlgorithm hasher = new SHA1Managed();
            // Use the hasher to hash the message
            byte[] hash = hasher.ComputeHash(messageToSignBytes);
            DumpBytes("Hash for message: ", hash);

            // Now sign the hash to create a signature
            byte[] signature = encryptProvider.SignHash(hash, CryptoConfig.MapNameToOID("SHA1"));
            DumpBytes("Signature: ", messageToSignBytes);

            // We can send the signature along with the message to authenticate it
            // Create a decryptor that uses the public key 
            // should be wrapped in using for production code
            RSACryptoServiceProvider decryptProvider = certificate.PublicKey.Key as RSACryptoServiceProvider;

            // Now use the signature to perform a successful validation of the message
            bool validSignature = decryptProvider.VerifyHash(hash, CryptoConfig.MapNameToOID("SHA1"), signature);
            Console.WriteLine($"Correct signature validated OK: {validSignature}");

            // Change one byte of the signature
            signature[0] = 99;
            // Now try using the incorrect signautre to validate the message
            bool invalidSignature = decryptProvider.VerifyHash(hash, CryptoConfig.MapNameToOID("SHA1"), signature);
            Console.WriteLine($"Incorrect signature validated OK: {invalidSignature}");
        }

        private static void CSPKeyStore() // Listing 3-19
        {
            string containerName = "MyKeyStore";

            CspParameters csp = new CspParameters();
            csp.KeyContainerName = containerName;

            // Create a new RSA to encrypt the data
            RSACryptoServiceProvider rsaStore = new RSACryptoServiceProvider(csp);
            Console.WriteLine($"Stored keys: {rsaStore.ToXmlString(includePrivateParameters: true)}");

            RSACryptoServiceProvider rsaLoad = new RSACryptoServiceProvider(csp);
            Console.WriteLine($"Loaded keys: {rsaLoad.ToXmlString(includePrivateParameters: true)}");
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
            foreach (var b in bytes)
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
            Console.WriteLine($"Doing work : {state}");
            Thread.Sleep(500);
            Console.WriteLine($"Work finished : {state}");
        }

        static void DisplayThread(Thread t)
        {
            Console.WriteLine($"Name : {t.Name}");
            Console.WriteLine($"Culture : {t.CurrentCulture}");
            Console.WriteLine($"Priority : {t.Priority}");
            Console.WriteLine($"Context : {t.ExecutionContext}");
            Console.WriteLine($"IsBackground? : {t.IsBackground}");
            Console.WriteLine($"IsPool? : {t.IsThreadPoolThread}");
        }

        public static ThreadLocal<Random> RandomGenerator = new ThreadLocal<Random>(() => { return new Random(2); });

        static void ThreadHello()
        {
            Console.WriteLine("Hello from the thread");
            Thread.Sleep(2000);
        }

        [Obsolete("This method is obsolete. Call DoWork() instead.", false)]
        static void WorkOnData(object data)
        {
            Console.WriteLine($"Working on: {data}");
            Thread.Sleep(1000);
        }
    }
}