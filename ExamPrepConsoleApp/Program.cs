//#define TERSE
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
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Data.SqlClient;
using System.Xml;

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

    class ImageOfDay
    {
        public string date { get; set; }
        public string explanation { get; set; }
        public string hdurl { get; set; }
        public string media_type { get; set; }
        public string service_version { get; set; }
        public string title { get; set; }
        public string url { get; set; }
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
        static PerformanceCounter TotalImageCounter;
        static PerformanceCounter ImagesPerSecondCounter;
        static EventLog imageEventLog;

        enum CreationResult
        {
            CreatedCounters,
            LoadedCounters,
            CreatedLog,
            LoadedLog
        };

        // static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        // static CancellationToken cancellationToken = new CancellationToken();

        static void Main(string[] args)
        {
            

            EndProgram();
        }

        private static void ReadXMLElements() // Listing 4-26
        {
            string XMLDocument = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
                                             "<MusicTrack xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"> " +
                                             "xmlns:xsd=\"http://www.w2.org/2001/XMLSchema\">  " +
                                             "<Artist>Rob Miles</Artist>  " +
                                             "<Title>My Way</Title>  " +
                                             "<Length>150</Length>" +
                                             "</MusicTrack>";

            using (StringReader stringReader = new StringReader(XMLDocument))
            {
                XmlTextReader reader = new XmlTextReader(stringReader);

                while (reader.Read())
                {
                    string description = string.Format($"Type: {reader.NodeType.ToString()} Name: {reader.Name} Value: {reader.Value}");
                    Console.WriteLine(description);
                }
            }
        }

        async Task<ImageOfDay> GetImageOfDay(string imageURL) // Listing 4-25
        {
            string NASAJson = await ReadWebpage(imageURL);

            ImageOfDay result = JsonConvert.DeserializeObject<ImageOfDay>(NASAJson);

            return result;
        }

        private static void ReadWithSQL() // Listing 4-19 
        {
            string connectionString = "Server=(localdb)\\mssqllocaldb;" +
                                                  "Database=MusicTracksContext-e0f0cd0d-38fe-44a4-add2-359310ff8b5d;" +
                                                  "Trusted_Connection=True;MultipleActiveResultSets=true"; // NEVER DO THIS

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SELECT * FROM MusicTrack", connection);

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string artist = reader["Artist"].ToString();
                    string title = reader["Title"].ToString();

                    Console.WriteLine($"Artist: {artist} Title {title}");
                }
            }
        }

        async Task<string> ReadHttpWebpage(string uri) // Listing 4-16
        {
            HttpClient client = new HttpClient();
            return await client.GetStringAsync(uri);
        }

        async Task<string> ReadWebpage(string uri) // Listing 4-15
        {
            WebClient client = new WebClient();
            return await client.DownloadStringTaskAsync(uri);
        }

        private static void UseWebClient() // Listing 4-14
        {
            WebClient webClient = new WebClient();
            string siteText = webClient.DownloadString("http://www.microsoft.com");
            Console.WriteLine(siteText);
        }

        private static void UsingHttpWebRequest() // Listing 4-13
        {
            WebRequest webRequest = WebRequest.Create("https://www.microsoft.com");
            WebResponse webResponse = webRequest.GetResponse();

            using (StreamReader responseReader = new StreamReader(webResponse.GetResponseStream()))
            {
                string siteText = responseReader.ReadToEnd();
                Console.WriteLine(siteText);
            }
        }

        private static void UseFindFiles() // Listing 4-12
        {
            DirectoryInfo startDir = new DirectoryInfo(@"..\..\..\");
            string searchString = "*.cs";

            FindFiles(startDir, searchString);
        }

        static void FindFiles(DirectoryInfo dir, string searchPattern) // Listing 4-12
        {
            foreach (DirectoryInfo directory in dir.GetDirectories())
            {
                FindFiles(directory, searchPattern);
            }

            FileInfo[] matchingFiles = dir.GetFiles(searchPattern);
            foreach (FileInfo fileInfo in matchingFiles)
            {
                Console.WriteLine(fileInfo.FullName);
            }
        }

        private static void UseFileClass2() // Listing 4-11
        {
            string fullName = @"c:\users\bnoel\Documents\test.txt";

            string dirName = Path.GetDirectoryName(fullName);
            string fileName = Path.GetFileName(fullName);
            string fileExtension = Path.GetExtension(fullName);
            string lisName = Path.ChangeExtension(fullName, ".lis");
            string newTest = Path.Combine(dirName, "newtest.txt");

            Console.WriteLine($"Full name: {fullName}");
            Console.WriteLine($"File directory: {dirName}");
            Console.WriteLine($"File name: {fileName}");
            Console.WriteLine($"File extension: {fileExtension}");
            Console.WriteLine($"File with lis extension: {lisName}");
            Console.WriteLine($"New test: {newTest}");
        }

        private static void UseDirectoryInfo() // Listing 4-10
        {
            DirectoryInfo localDir = new DirectoryInfo("TestDir");

            localDir.Create();

            if (localDir.Exists)
                Console.WriteLine("Directory created successfully");

            localDir.Delete();

            Console.WriteLine("Directory deleted successfully");
        }

        private static void UseDirectoryClass() // Listing 4-9
        {
            Directory.CreateDirectory("TestDir");
            if (Directory.Exists("TestDir"))
                Console.WriteLine("Directory created successfully");

            Directory.Delete("TestDir");
            Console.WriteLine("Directory deleted successfully");
        }

        private static void UsingFileInfo() // Listing 4-8
        {
            string filePath = "TextFile.txt";

            File.WriteAllText(path: filePath, contents: "This text goes in the file");
            FileInfo info = new FileInfo(filePath);
            Console.WriteLine($"Name: {info.Name}");
            Console.WriteLine($"Full Path: {info.FullName}");
            Console.WriteLine($"Last Access: {info.LastAccessTime}");
            Console.WriteLine($"Length: {info.Length}");
            Console.WriteLine($"Attributes: {info.Attributes}");
            Console.WriteLine("Make the file read only");
            info.Attributes |= FileAttributes.ReadOnly;
            Console.WriteLine($"Attributes: {info.Attributes}");
            Console.WriteLine($"Remove the read only attribute");
            info.Attributes &= ~FileAttributes.ReadOnly;
            Console.WriteLine($"Attributes: {info.Attributes}");
        }

        private static void DriveInformation() // Listing 4-7
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo drive in drives)
            {
                Console.Write($"Name: {drive.Name} ");
                if (drive.IsReady)
                {
                    Console.Write($"  Type:{drive.DriveType}");
                    Console.Write($"  Format:{drive.DriveFormat}");
                    Console.Write($"  Free space:{drive.TotalFreeSpace}");
                }
                else
                {
                    Console.Write(" Drive is not ready");
                }
            }
        }

        private static void FileNotFoundExceptionHandling() // Listing 4-6
        {
            try
            {
                string contents = File.ReadAllText(path: "TestFile.txt");
                Console.WriteLine(contents);
            }
            catch (FileNotFoundException notFoundEx)
            {
                // File not found
                Console.WriteLine(notFoundEx.Message);
            }
            catch (Exception ex)
            {
                // Any other exception
                Console.WriteLine(ex.Message);
            }
        }

        private static void UseFileClass() // Listing 4-5
        {
            File.WriteAllText(path: "TextFile.txt", contents: "This text goes in the file");

            File.AppendAllText(path: "TextFile.txt", contents: " - This goes on the end");

            if (File.Exists("TextFile.txt"))
                Console.WriteLine("Text File exists");

            try // Listing 4-6
            {
                string contents = File.ReadAllText(path: "TextFile.txt");
                Console.WriteLine($"File contents: {contents}");
            }
            catch (FileNotFoundException notFoundEx)
            {
                // File not found
                Console.WriteLine(notFoundEx.Message);
            }
            catch (Exception ex)
            {
                // Any other exception
                Console.WriteLine(ex.Message);
            }

            File.Copy(sourceFileName: "TextFile.txt", destFileName: "Copy-TextFile.txt");

            using (TextReader reader = File.OpenText(path: "Copy-TextFile.txt"))
            {
                string text = reader.ReadToEnd();
                Console.WriteLine($"Copied text: {text}");
            }
        }

        private static void StoreCompressedFile() // Listing 4-4
        {
            using (FileStream writeFile = new FileStream("CompText.zip", FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (GZipStream writeFileZip = new GZipStream(writeFile, CompressionMode.Compress))
                {
                    using (StreamWriter writeFileText = new StreamWriter(writeFileZip))
                    {
                        writeFileText.Write("Hello world");
                    }
                }
            }

            using (FileStream readFile = new FileStream("CompText.zip", FileMode.Open, FileAccess.Read))
            {
                using (GZipStream readFileZip = new GZipStream(readFile, CompressionMode.Decompress))
                {
                    using (StreamReader readFileText = new StreamReader(readFileZip))
                    {
                        string message = readFileText.ReadToEnd();
                        Console.WriteLine($"Read text: {message}");
                    }
                }
            }
        }

        private static void UseStreamWriterStreamReader() // Listing 4-3
        {
            using (StreamWriter writeStream = new StreamWriter("OutputText.txt"))
            {
                writeStream.Write("Hello world");
            }
            using (StreamReader readStream = new StreamReader("OutputText.txt"))
            {
                string readString = readStream.ReadToEnd();
                Console.WriteLine($"Text read: {readString}");
            }
        }

        private static void UseFileStream() // Listing 4-2
        {
            using (FileStream outputStream = new FileStream("OutputText.txt", FileMode.OpenOrCreate, FileAccess.Write))
            {
                string outputMessageString = "Hello world";
                byte[] outputMessageBytes = Encoding.UTF8.GetBytes(outputMessageString);
                outputStream.Write(outputMessageBytes, 0, outputMessageBytes.Length);
            }
        }

        private static void WriteToFile() // Listing 4-1
        {
            // Writing to a file
            FileStream outputStream = new FileStream("OutputText.txt", FileMode.OpenOrCreate, FileAccess.Write);
            string outputMessageString = "Hello World";
            byte[] outputMessageBytes = Encoding.UTF8.GetBytes(outputMessageString);
            outputStream.Write(outputMessageBytes, 0, outputMessageBytes.Length);
            outputStream.Close();
            FileStream inputStream = new FileStream("OutputText.txt", FileMode.Open, FileAccess.Read);
            long fileLength = inputStream.Length;
            byte[] readBytes = new byte[fileLength];
            inputStream.Read(readBytes, 0, (int)fileLength);
            string readString = Encoding.UTF8.GetString(readBytes);
            inputStream.Close();
            Console.WriteLine($"Read message: {readString}");
        }

        private static void BindToEventLog() // Listing 3-46
        {
            string categoryName = "Image Processing";

            EventLog imageEventLog = new EventLog();
            imageEventLog.Source = categoryName;
            imageEventLog.EntryWritten += ImageEventLog_EntryWritten;
            imageEventLog.EnableRaisingEvents = true;

            Console.WriteLine("Listening for log events");
        }

        private static void ImageEventLog_EntryWritten(object sender, EntryWrittenEventArgs e) // Listing 3-46
        {
            Console.WriteLine(e.Entry.Message);
        }

        private static void ReadFromEventLog() // Listing 3-45
        {
            string categoryName = "Image Processing";

            if (!EventLog.SourceExists(categoryName))
                Console.WriteLine("Event log not present");
            else
            {
                EventLog imageEventLog = new EventLog();
                imageEventLog.Source = categoryName;
                foreach (EventLogEntry entry in imageEventLog.Entries)
                {
                    Console.WriteLine($"Source: {entry.Source} Type: {entry.TimeWritten} Message: {entry.Message}");
                }
            }
        }

        private static void WriteToEventLog() // Listing 3-44
        {
            SetupLog();

            if (SetupLog() == CreationResult.CreatedLog)
            {
                Console.WriteLine("Log created");
                Console.WriteLine("Restart the program");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Processing started");

            imageEventLog.WriteEntry("Image processing started");

            // process images

            imageEventLog.WriteEntry("Image processing ended");

            Console.WriteLine("Processing complete.");
        }

        private static CreationResult SetupLog() // Listing 3-44
        {
            string categoryName = "Image Processing";

            if (EventLog.SourceExists(categoryName))
            {
                imageEventLog = new EventLog();
                imageEventLog.Source = categoryName;
                return CreationResult.LoadedLog;
            }

            EventLog.CreateEventSource(source: categoryName, logName: categoryName + " log");

            return CreationResult.CreatedLog;
        }

        private static CreationResult SetupPerformanceCounters() // Listing 3-43
        {
            string categoryName = "Image Processing";
            if (PerformanceCounterCategory.Exists(categoryName))
            {
                // production code should use using
                TotalImageCounter = new PerformanceCounter(categoryName: categoryName, counterName: "# of images processed", readOnly: false);
                // production code should use using
                ImagesPerSecondCounter = new PerformanceCounter(categoryName: categoryName, counterName: "# images processed per second", readOnly: false);
                return CreationResult.LoadedCounters;
            }

            CounterCreationData[] counters = new CounterCreationData[] {
                new CounterCreationData(counterName: "# of images processed",
                                        counterHelp: "number of images resized",
                                        counterType: PerformanceCounterType.NumberOfItems64),
                new CounterCreationData(counterName: "# images processed per second",
                                        counterHelp: "number of images processed per second",
                                        counterType: PerformanceCounterType.RateOfCountsPerSecond32)
            };

            CounterCreationDataCollection counterCollection = new CounterCreationDataCollection(counters);

            PerformanceCounterCategory.Create(categoryName: categoryName,
                                              categoryHelp: "Image processing information",
                                              categoryType: PerformanceCounterCategoryType.SingleInstance,
                                              counterData: counterCollection);
            return CreationResult.CreatedCounters;
        }

        private static void ReadPerformanceCounters() // Listing 3-42
        {
            PerformanceCounter processor = new PerformanceCounter(
                categoryName: "Processor Information",
                counterName: "% Processor Time",
                instanceName: "_Total"
            );

            Console.WriteLine("Press any key to stop");

            while (true)
            {
                System.Console.WriteLine($"Processor time {processor.NextValue()}");
                Thread.Sleep(500);
                if (Console.KeyAvailable)
                    break;
            }
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