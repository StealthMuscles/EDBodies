using System;
using System.IO;
using System.Threading;


namespace EDBodies
{
    class Program
    {
        static void Main(string[] args)
        {
            Run();
        }

        private static void Run()
        {
            string[] args = Environment.GetCommandLineArgs();

            // If a directory is not specified, exit program.
            if (args.Length != 2)
            {
                // Display the proper way to call the program.
                Console.WriteLine("Usage: dotnet .\\EDBodies.dll <directory>");
                return;
            }

            LogitechArx.logiArxCbContext contextCallback;
            contextCallback.arxCallBack = new LogitechArx.logiArxCB(SDKCallback);
            contextCallback.arxContext = System.IntPtr.Zero;

            if (!LogitechArx.LogiArxInit("org.duckdns.stealthmuscles.EDBodies", "EDBodies", ref contextCallback))
            {
                Console.WriteLine($"loading sdk failed: {LogitechArx.LogiArxGetLastError()}");
                return;
            }

            // Create a new FileSystemWatcher and set its properties.
            using (FileSystemWatcher watcher = new FileSystemWatcher())
            {
                watcher.Path = args[1];

                // Watch for changes in LastAccess and LastWrite times
                watcher.NotifyFilter = NotifyFilters.LastAccess
                                     | NotifyFilters.LastWrite;

                // Only watch our text file
                watcher.Filter = "EstimatedValues.txt";

                // Add event handlers.
                watcher.Changed += OnChanged;
                watcher.Created += OnChanged;
                watcher.Deleted += OnChanged;
                watcher.Renamed += OnRenamed;

                // Begin watching.
                watcher.EnableRaisingEvents = true;

                // Wait for the user to quit the program.
                Console.WriteLine("Type 'q' to quit:");
                while (Console.Read() != 'q') ;
                LogitechArx.LogiArxShutdown();
            }
        }

        // Define the event handlers.
        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            Console.WriteLine($"File: {e.FullPath} {e.ChangeType}");
            Thread.Sleep(2000);
            string[] args = Environment.GetCommandLineArgs();
            string data = File.ReadAllText($"{args[1]}\\EstimatedValues.txt");
            //Console.WriteLine(data);
            
            if (!LogitechArx.LogiArxSetTagContentById("bodies-data", data.Replace(System.Environment.NewLine, "&").Replace(@"\n", @"\\n")))
            {
                Console.WriteLine($"LogiArxSetTagContentById(bodies-data) failed: {LogitechArx.LogiArxGetLastError()}");
            }

            if (!LogitechArx.LogiArxSetTagContentById("systemName", "Needs Update"))
            {
                Console.WriteLine($"LogiArxSetTagContentById(systemName) failed: {LogitechArx.LogiArxGetLastError()}");
            }
        }

        private static void OnRenamed(object source, RenamedEventArgs e) =>
            // Specify what is done when a file is renamed.
            Console.WriteLine($"File: {e.OldFullPath} renamed to {e.FullPath}");

        private static void SDKCallback(int eventType, int eventValue, System.String eventArg, System.IntPtr context)
        {
            Console.WriteLine($"SDKCallback: eventType={eventType}, eventValue={eventValue}, eventArg={eventArg}");
            
            if (eventType == LogitechArx.LOGI_ARX_EVENT_MOBILEDEVICE_ARRIVAL)
            {
                if (!LogitechArx.LogiArxAddFileAs("./html/index.html", "index.html", "text/html"))
                {
                    Console.WriteLine($"LogiArxAddFileAs (index.html) failed: {LogitechArx.LogiArxGetLastError()}");
                    return;
                }

                if (!LogitechArx.LogiArxAddFileAs("./html/d3.min.js", "d3.min.js", "application/javascript"))
                {
                    Console.WriteLine($"LogiArxAddFileAs (d3.min.js) failed: {LogitechArx.LogiArxGetLastError()}");
                    return;
                }

                if (!LogitechArx.LogiArxAddFileAs("./html/tabulator.min.js", "tabulator.min.js", "application/javascript"))
                {
                    Console.WriteLine($"LogiArxAddFileAs (tabulator.min.js) failed: {LogitechArx.LogiArxGetLastError()}");
                    return;
                }

                if (!LogitechArx.LogiArxAddFileAs("./html/tabulator_midnight.min.css", "tabulator_midnight.min.css", "text/css"))
                {
                    Console.WriteLine($"LogiArxAddFileAs (tabulator_midnight.min.css) failed: {LogitechArx.LogiArxGetLastError()}");
                    return;
                }

                string[] args = Environment.GetCommandLineArgs();
                string data = File.ReadAllText($"{args[1]}\\EstimatedValues.txt");
                string script = $"function EDBodies() {{ return `{data.Replace(System.Environment.NewLine, "&").Replace(@"\n", @"\\n")}` }};";
                //Console.WriteLine(script);
                if (!LogitechArx.LogiArxAddUTF8StringAs(script, "bodies.js", "application/javascript"))
                {
                    Console.WriteLine($"LogiArxAddUTF8StringAs failed: {LogitechArx.LogiArxGetLastError()}");
                    return;
                }

                if (!LogitechArx.LogiArxSetIndex("index.html"))
                {
                    Console.WriteLine($"LogiArxSetIndex failed: {LogitechArx.LogiArxGetLastError()}");
                }
            }
            /*
            else if (eventType == LogitechArx.LOGI_ARX_EVENT_MOBILEDEVICE_REMOVAL)
            {    //Device disconnected   
            }
            else if (eventType == LogitechArx.LOGI_ARX_EVENT_TAP_ON_TAG)
            {
                if (eventArg == "myBtn")
                {     //Do something on this input    
                }
            }
            */
        }
    }  
}
