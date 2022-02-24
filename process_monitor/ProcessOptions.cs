using System.Diagnostics;
using System.Text.RegularExpressions;

namespace process_monitor;

public class ProcessOptions
{
    public void Stop()
    {
        _success = false;
        _mre.Set();
    }

    private void Start()
    {
        if (_processOptTask is not null && _processOptTask.IsAlive) return;
        _mre.Set();
        _processOptTask = new Thread(ProcessTask);
        _processOptTask.Start();
    }
    public ProcessOptions(string? input, double frequency , double liveTime , Process? process = null!)
    {
        SearchName = input;
        Frequency = frequency;
        LiveTime = liveTime;
        MyProcess = process;
        if (MyProcess != null)
        {
            Start();
        }
    }

    public readonly string? SearchName;
    public double Frequency;
    private TimeSpan Interval
    {
        get
        {
            TimeSpan interval = TimeSpan.FromMinutes(1 / Frequency);
            return interval;
        }
    }
    public double LiveTime;
    public Process? MyProcess;
    private bool _success;
    private readonly ManualResetEvent _mre = new ManualResetEvent(false);
    private Thread? _processOptTask;

    private void ProcessTask()
    {
        while (_success)
        {
            _mre.Reset();
            Process?[] processCollection = Process.GetProcesses();
            if (MyProcess == null)
            {
                var matchesProcesses = new List<Process?>();
                foreach (var p in processCollection)
                {
                    var regex = new Regex(SearchName);
                    if (p?.ProcessName == null || !regex.IsMatch(p.ProcessName)) continue;

                    matchesProcesses.Add(p);
                    Console.WriteLine("{0} {1}", p.Id, p.ProcessName);
                }

                if (matchesProcesses.Count == 1)
                {
                    MyProcess = matchesProcesses[0];
                    Console.WriteLine("Process " + matchesProcesses[0] +" automatically added");
                }
            }
            else
            {

                    MyProcess.EnableRaisingEvents = true;
                    MyProcess.Exited += (_, _) =>
                    {
                        if (MyProcess != null)
                        {
                            Console.WriteLine("Process {0} closed by third party", SearchName);
                            MyProcess = null;   
                        }
                    };
                    if (DateTime.Now - MyProcess.StartTime > TimeSpan.FromMinutes(LiveTime))
                    {
                        MyProcess.Kill();
                        Console.WriteLine("Process {0}, id[{1}] killed.",
                            MyProcess?.ProcessName, MyProcess?.Id);
                        MyProcess = null;
                    }
            }
            _mre.WaitOne(Interval);
        }
    }
}