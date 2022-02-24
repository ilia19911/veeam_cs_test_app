using System.Diagnostics;
using CommandLine;
using System.Text.RegularExpressions;

namespace process_monitor;

internal static class ProcessMonitor
{
    private enum State
    {
        MainMenu,
        NewProcessMenu,
        ProcessMenu,
        Stop
    }

    private static State _programState;
    private static readonly List<ProcessOptions?> MyProcesses = new List<ProcessOptions?>();
    private static ProcessOptions? _selectedProcess;
    private static readonly List<(string,ConsoleColor)> MessageList = new();
    private static bool _force;
    private static bool AskParam(string message, out double result)
    {
        while (true)
        {
            Console.WriteLine(message);
            var str = Console.ReadLine()?.ToLower();
            if (str == "exit")
            {
                result = 0;
                return false;
            }

            if (double.TryParse(str, out result))
            {
                return true;
            }
            else
            {
                Console.WriteLine("Can't parse param");
            }
        }
    }

    private static void AddProcess(Process? myProcess, double frequency, double liveTime, string searchName)
    {
        if (frequency!=0 || AskParam("Please enter check frequency for this process, or \"exit\" for  cancel", out frequency))
        {
            if (liveTime != 0 || AskParam("Please enter live time for this process, or \"exit\" for  cancel", out liveTime))
            {
                if (myProcess != null)
                {
                    var processOpt = new ProcessOptions(myProcess.ProcessName, frequency, liveTime,  myProcess);
                    MyProcesses.Add(processOpt);
                    MessageList.Add((
                        "Process " + myProcess.ProcessName + ", id " + myProcess.Id + " added.",
                        ConsoleColor.Black));   
                }
                else
                {
                    var processOpt = new ProcessOptions(searchName, frequency, liveTime,  myProcess);
                    MyProcesses.Add(processOpt);
                    MessageList.Add((
                        "Process search name " + searchName +  " added.",
                        ConsoleColor.Black));
                }
                _programState = State.MainMenu;
            }
        }
    }
    private static void AddProcessMenuProcessing(string searchName = "",double frequency = 0, double liveTime = 0)
    {
        Console.Clear();
        Console.BackgroundColor = ConsoleColor.Magenta;
        Console.WriteLine("Process monitor program. \n Add process menu");
        Console.BackgroundColor = ConsoleColor.Black;
        Console.WriteLine("Press -h and enter for help, type exit to go to the main menu ");
        Process? myProcess = null;
        foreach (var message in MessageList)
        {
            Console.BackgroundColor = message.Item2;
            Console.WriteLine(message.Item1);
            Console.BackgroundColor = ConsoleColor.Black;
        }
        MessageList.Clear();
        if (searchName == "")
        {
            var str = Console.ReadLine()?.ToLower();
            searchName = str ?? "";
        }

        if (searchName == null)
        {
            throw new Exception("search name can't be null");
        }
        switch (searchName)
        {
            case "":
                MessageList.Add(("Please, type valid name.", ConsoleColor.Yellow));
                break;
            case "-h":
                MessageList.Add(("Press l and enter  to get processes list.", ConsoleColor.Black));
                MessageList.Add(("Press -f and enter  to set or reset force flag.", ConsoleColor.Black));
                MessageList.Add(("Type process id / name (or regex) to select process.", ConsoleColor.Black));
                MessageList.Add(("Type exit to close to main menu.", ConsoleColor.Black));
                break;
            case "-f":
                _force ^= true;
                MessageList.Add(("Force flag is " + _force,  ConsoleColor.Black));
                break;
            case "-l":
                MessageList.Add(("Process list", ConsoleColor.Magenta));
                MessageList.Add(("id               name ", ConsoleColor.Black));
                Process?[] processCollection = Process.GetProcesses();
                foreach (Process? p in processCollection)
                {
                    if (p != null) MessageList.Add((p.Id + "    " + p.ProcessName, ConsoleColor.Black));
                }
                break;
            case "exit":
                _programState = State.MainMenu;
                break;
            default:
                Process?[] processCollection2 = Process.GetProcesses();
                if (int.TryParse(searchName, out var id))
                {
                    myProcess = Array.Find(processCollection2, x => x != null && x.Id == id);
                    if (myProcess != null)
                    {
                        if (MyProcesses.Find(x => x?.MyProcess?.Id == id) == null)
                        {
                            MessageList.Add(("You selected process " + myProcess.ProcessName, ConsoleColor.Black));
                            AddProcess(myProcess, frequency, liveTime, searchName);
                        }
                        else
                        {
                            MessageList.Add(("Process already added", ConsoleColor.Red));
                        }   
                    }
                }
                else
                {
                    MessageList.Add(("Search by name", ConsoleColor.Black));
                    MessageList.Add(("id    name ", ConsoleColor.Black));
                    var matchesProcesses = new List<Process?>();
                    foreach (var p in processCollection2)
                    {
                        if (searchName != null)
                        {
                            var regex = new Regex(searchName);
                            if (p?.ProcessName == null || !regex.IsMatch(p.ProcessName.ToLower())) continue;
                        }

                        matchesProcesses.Add(p);
                        if (p != null) MessageList.Add((p.Id + "    " + p.ProcessName, ConsoleColor.Black));
                    }

                    if (matchesProcesses.Count == 1)
                    {
                        myProcess = matchesProcesses[0];
                        if (myProcess != null)
                        {
                            if (MyProcesses.Find(x => x?.MyProcess?.ProcessName == myProcess.ProcessName) == null)
                            {
                                MessageList.Add(("You selected process " + myProcess.ProcessName, ConsoleColor.Black));
                                Debug.Assert(searchName != null, nameof(searchName) + " != null");
                                AddProcess(myProcess, frequency, liveTime, searchName);
                            }
                            else
                            {
                                MessageList.Add(("Process already added", ConsoleColor.Red));
                            }   
                        }
                    }
                    else if (_force)
                    {
                        if (MyProcesses.Find(x => x?.SearchName == searchName) == null)
                        {
                            MessageList.Add(("Process not founded, but was added cause force flag ", ConsoleColor.Yellow));
                            MessageList.Add(("Process search name " + searchName, ConsoleColor.Yellow));
                            Debug.Assert(searchName != null, nameof(searchName) + " != null");
                            AddProcess(myProcess, frequency, liveTime, searchName);
                        }
                        else
                        {
                            MessageList.Add(("Process already added", ConsoleColor.Red));
                        }
                    }
                }
                break;
        }
    }
    private static void MainMenuProcessing()
    {
        Console.Clear();
        Console.BackgroundColor = ConsoleColor.Green;
        Console.WriteLine("Process monitor program. \n Main menu");
        Console.BackgroundColor = ConsoleColor.Black;
        Console.WriteLine("Press H and enter for help");
        foreach (var (item1, consoleColor) in MessageList)
        {
            Console.BackgroundColor = consoleColor;
            Console.WriteLine(item1);
            Console.BackgroundColor = ConsoleColor.Black;
        }
        MessageList.Clear();
        var input = Console.ReadLine()?.ToLower();
        switch (input)
        {
            case "h":
                MessageList.Add(("This is main menu of process monitor program.", ConsoleColor.Black));
                MessageList.Add(("Press A and enter to add new process.", ConsoleColor.Black));
                MessageList.Add(("Press L and enter to show all added processes.",ConsoleColor.Black));
                MessageList.Add(("Enter process name(regex) or id to config process options.", ConsoleColor.Black));
                break;
            case "a":
                _programState = State.NewProcessMenu;
                break;
            case "l":
                foreach (var process in MyProcesses)
                {
                    var values = Enum.GetValues(typeof(ConsoleColor));
                    var random = new Random();
                    var randomBar = (ConsoleColor)(values.GetValue(random.Next(values.Length)) ?? ConsoleColor.Black);
                    if (randomBar == ConsoleColor.Gray)
                    {
                        randomBar = ConsoleColor.Black;
                    }
                    if (process?.MyProcess != null)
                    {
                        MessageList.Add(("Process " + process.MyProcess?.ProcessName, randomBar));
                        if (process.MyProcess != null) MessageList.Add(("id " + process.MyProcess.Id, randomBar));
                    }
                    else
                    {
                        MessageList.Add(("Process search name " + process?.SearchName, randomBar));
                    }

                    if (process == null) continue;
                    MessageList.Add(("Check frequency " + process.Frequency + "p.m.", randomBar));
                    MessageList.Add(("Max live time " + process.LiveTime + "sec.", randomBar));
                }
                break;
            default:
                if (int.TryParse(input, out var id))
                {
                    var process =  MyProcesses.Find(x => x?.MyProcess != null && x.MyProcess.Id == id) ;
                    _selectedProcess = process;
                }
                else if(!string.IsNullOrEmpty(input))
                {
                    var matchesProcesses = new List<ProcessOptions?>();
                    foreach (var p in MyProcesses)
                    {
                        var regex = new Regex(input);
                        if (p?.SearchName != null && regex.IsMatch(p.SearchName))
                        {
                            if(p.MyProcess != null && regex.IsMatch(p.MyProcess.ProcessName))
                            {
                                matchesProcesses.Add(p);   
                            }   
                            else if (regex.IsMatch(p.SearchName))
                            {
                                matchesProcesses.Add(p); 
                            }
                        }
                    }
                    if (matchesProcesses.Count == 1)
                    {
                        _selectedProcess = matchesProcesses[0];
                    }
                }

                if (_selectedProcess != null)
                {
                    _programState = State.ProcessMenu;
                }
                break;
                
        }
    }
    private static void ProcessMenuProcessing()
    {
        if (_selectedProcess == null)
        {
            throw new Exception("Selected process can't be null. Program error");
        }
        Console.Clear();
        Console.BackgroundColor = ConsoleColor.Green;
        Console.WriteLine("Process monitor program.");
        Console.BackgroundColor = ConsoleColor.Blue;
        Console.WriteLine("Process {0} menu",
            _selectedProcess.MyProcess == null ? _selectedProcess.SearchName : _selectedProcess.MyProcess.ProcessName);
        Console.BackgroundColor = ConsoleColor.Black;
        Console.WriteLine("Press H and enter for help, type exit to go to the main menu ");
        foreach (var message in MessageList)
        {
            Console.BackgroundColor = message.Item2;
            Console.WriteLine(message.Item1);
            Console.BackgroundColor = ConsoleColor.Black;
        }
        MessageList.Clear();
        var input = Console.ReadLine()?.ToLower();
        switch (input)
        {
            case "h":
                MessageList.Add(("Press F to set process check frequency.", ConsoleColor.Black));
                MessageList.Add(("Press T and enter to set process live time.", ConsoleColor.Black));
                MessageList.Add(("Press S and enter to show status of process monitor.", ConsoleColor.Black));
                MessageList.Add(("Press D and enter to delete process from list.", ConsoleColor.Black));
                MessageList.Add(("Enter exit for close.", ConsoleColor.Black));
                break;
            case "f":
                Console.WriteLine("Please, enter check frequency for this process");
                input = Console.ReadLine()?.ToLower();
                if (double.TryParse(input, out var frequency))
                {
                    _selectedProcess.Frequency = frequency;
                    MessageList.Add(("frequency set as " + frequency, ConsoleColor.Black));
                }
                break;
            case "t":
                Console.WriteLine("Please, enter max live time");
                input = Console.ReadLine()?.ToLower();
                if (double.TryParse(input, out var lifetime))
                {
                    _selectedProcess.LiveTime = lifetime;
                    MessageList.Add(("Max live time set as " + lifetime, ConsoleColor.Black));
                }
                break;
            case "d":
                if (MyProcesses.Remove(_selectedProcess))
                {
                    MessageList.Add(_selectedProcess.MyProcess is null
                        ? ("process " + _selectedProcess.SearchName + " deleted from list", ConsoleColor.Black)
                        : ("process " + _selectedProcess.MyProcess.ProcessName + " deleted from list",
                            ConsoleColor.Black));

                    _selectedProcess.Stop();
                    _selectedProcess = null;
                    _programState = State.MainMenu;
                }
                else
                {
                    MessageList.Add(("Can't remove process. Program error", ConsoleColor.Red));
                }
                break;
            case "s":
                MessageList.Add(("Name for process search = " +_selectedProcess.SearchName, ConsoleColor.Black));
                MessageList.Add(("Check frequency = " + _selectedProcess.Frequency, ConsoleColor.Black));
                MessageList.Add(("Max live time =     " +  TimeSpan.FromMinutes( _selectedProcess.LiveTime).ToString("dd\\.hh\\:mm\\:ss") + "(d.h.m.s)", ConsoleColor.Black));
                if (_selectedProcess.MyProcess != null)
                {
                    MessageList.Add(("Time after start = "+(DateTime.Now - _selectedProcess.MyProcess.StartTime).ToString(@"dd\.hh\:mm\:ss") + "(d.h.m.s)", ConsoleColor.Black));
                    MessageList.Add(("Process name = " + _selectedProcess.MyProcess.ProcessName, ConsoleColor.Black));
                }
                break;
            case "exit":
                _selectedProcess = null;
                _programState = State.MainMenu;
                break;
            default:
                MessageList.Add(("Can't response, pass H and enter for help", ConsoleColor.Yellow));
                break;
        }
    }
    private static int Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args)
            .WithParsed(o =>
            {
                Console.WriteLine("Start process monitor program");
                _force = true;
                if (o.ProcessName != null)
                    AddProcessMenuProcessing(searchName: o.ProcessName, frequency: o.Frequency,
                        liveTime: o.MaxProcessTime);
                _force = false;
                _programState = State.MainMenu;
                while (_programState != State.Stop)
                {
                    switch (_programState)
                    {
                        case State.MainMenu:
                            MainMenuProcessing();
                            break;
                        case State.NewProcessMenu:
                            AddProcessMenuProcessing();
                            break;
                        case State.ProcessMenu:
                            ProcessMenuProcessing();
                            break;
                        case State.Stop:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                Console.WriteLine("Stop process monitor program");
            });
        return 0;
    }
}