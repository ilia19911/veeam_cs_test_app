using System.Diagnostics;
using System.Text.RegularExpressions;

namespace process_monitor;

/// <summary>
/// Process superstructure.
/// </summary>
public class ProcessOptions
{
    /// <summary>
    /// Stop check thread. Call it before remove ProcessOptions
    /// </summary>
    public void Stop()
    {
        _success = false;
        _mre.Set();
    }
    /// <summary>
    /// Start check thread. It calls automatically in constructor
    /// </summary>
    private void Start()
    {
        if (_processOptTask is not null && _processOptTask.IsAlive) return;
        _success = true;
        _mre.Set();
        _processOptTask = new Thread(ProcessTask);
        _processOptTask.Start();
    }
    /// <summary>
    /// Constructor of ProcessOptions
    /// </summary>
    /// <param name="searchName"> Process search name </param>
    /// <param name="frequency">Process check frequency</param>
    /// <param name="maxLiveTime">Process max live time</param>
    /// <param name="process">Process to be monitored</param>
    public ProcessOptions(string searchName, double frequency , double maxLiveTime , Process? process = null!)
    {
        SearchName = searchName;
        Frequency = frequency;
        MaxLiveTime = maxLiveTime;
        MyProcess = process;
        Start();
    }

    /// <summary>
    /// The name of the search process. This is necessary to find the process if it killed
    /// </summary>
    public readonly string SearchName;
    /// <summary>
    /// Process check frequency. 
    /// </summary>
    public double Frequency;
    /// <summary>
    /// This interval is used to wait for the next iteration of the check.
    /// </summary>
    private TimeSpan Interval
    {
        get
        {
            TimeSpan interval = TimeSpan.FromMinutes(1 / Frequency);
            return interval;
        }
    }
    /// <summary>
    /// Maximum lifetime. If the process lifetime is greater than the maximum time, the process will be killed.
    /// </summary>
    public double MaxLiveTime;
    
    /// <summary>
    /// Process object. Can be null, if the process is killed or user set a force flag but the process is not found
    /// </summary>
    public Process? MyProcess;
    /// <summary>
    /// flag that is true if ProcessOption is alive 
    /// </summary>
    private bool _success;
    /// <summary>
    /// ManualResetEvent for waiting of next check iteration 
    /// </summary>
    private readonly ManualResetEvent _mre = new ManualResetEvent(false);
    /// <summary>
    /// Thread for check process
    /// </summary>
    private Thread? _processOptTask;
    /// <summary>
    /// Check process task
    /// </summary>
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
                try
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
                    if (DateTime.Now - MyProcess.StartTime > TimeSpan.FromMinutes(MaxLiveTime))
                    {
                        MyProcess.Kill();
                        Console.WriteLine("Process {0}, id[{1}] killed.",
                            MyProcess?.ProcessName, MyProcess?.Id);
                        MyProcess = null;
                    }
                }
                catch ( Exception e)
                {
                    _success = false;
                    Console.WriteLine("Can't kill this process, not enough access ");
                    Console.WriteLine(e);
                }
            }
            _mre.WaitOne(Interval);
        }
    }
}