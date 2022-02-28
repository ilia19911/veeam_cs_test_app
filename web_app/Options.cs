using CommandLine;

namespace web_app;

internal class Options
{
    /// <summary>
    /// process name for monitoring
    /// </summary>
    [Option('c', "count", Required = false, HelpText = "Projected number of vacancies")]
    public string? VacanciesCount { get; set; }
}