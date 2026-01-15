using NCrontab; // To use CrontabSchedule and related classes

DateTime start = new(2024, 1, 1);
DateTime end = start.AddYears(1);
WriteLine($"Start at: {start:ddd, MMM yyyy HH:mm:ss}");
WriteLine($"End at:   {end:ddd, MMM yyyy HH:mm:ss}");
WriteLine();

string sec = "0,30";
string min = "*";
string hour = "*";
string dayOfMonth = "*";
string month = "*";
string expression = string.Format("{0,-3} {1,-3} {2,-3} {3,-3} {4,-3} {5,-3}",
    sec, min, hour, dayOfMonth, month, dayOfMonth);
WriteLine($"Expression: {expression}");

WriteLine(@"            \ / \ / \ / \ / \ / \ /");
WriteLine($"             -   -   -   -   -   -");
WriteLine($"             |   |   |   |   |   |");
WriteLine($"             |   |   |   |   |   +--- day of week (0 - 6) (Sunday=0)");
WriteLine($"             |   |   |   |   +------- month (1 - 12)");
WriteLine($"             |   |   |   +----------- day of month (1 - 31)");
WriteLine($"             |   |   +--------------- hour (0 - 23)");
WriteLine($"             |   +------------------- min (0 - 59)");
WriteLine($"             +----------------------- sec (0 - 59)");
WriteLine();

CrontabSchedule schedule = CrontabSchedule.Parse(expression, 
    new CrontabSchedule.ParseOptions { IncludingSeconds = true });

IEnumerable<DateTime> occurrences = schedule.GetNextOccurrences(start, end);

// Output the first 40 occurrences.

foreach (DateTime occurrence in occurrences.Take(20))
{
    WriteLine($"{occurrence:ddd, dd MMM yyyy HH:mm:ss}");
}