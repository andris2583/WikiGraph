using System.Text;
using Data;

public static class SQLParser
{
  public static void ParseGraph()
  {
    string connectionString = "Server=localhost;Port=3306;Database=wiki;uid=root;pwd=rootpassword;Connection Timeout=60";
    long startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
    List<PageLink> pageLinks = Reader.LoadPageLinks(connectionString);
    Util.ShuffleList(pageLinks);
    Console.WriteLine($"{pageLinks.Count} PageLinks loaded in {Util.GetTimeElapsed(startTime)} ms");
    startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

    List<LinkTarget> linkTargets = Reader.LoadLinkTargets(connectionString);
    Console.WriteLine($"{linkTargets.Count} LinkTargets loaded in {Util.GetTimeElapsed(startTime)} ms");
    startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

    List<Page> pages = Reader.LoadPages(connectionString);
    Console.WriteLine($"{pages.Count} Pages loaded in {Util.GetTimeElapsed(startTime)} ms");
    startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

    startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

    string outputFolder = "output";

    // Ensure the output directory exists
    Directory.CreateDirectory(outputFolder);

    // Write data to CSV file
    ProcessData(pageLinks, linkTargets, pages, startTime);

    Console.WriteLine("Generation done");
    Console.WriteLine(Util.GetTimeElapsed(startTime) + " ms elapsed!");

  }

  public static void ProcessData(List<PageLink> pageLinks, List<LinkTarget> linkTargets, List<Page> pages, long startTime)
  {
    var csv = new StringBuilder();
    string[] names = new string[1200000];
    int[] sizes = new int[1200000];
    csv.AppendLine("source;target"); // Header

    var entryCount = 0;
    var linkTargetDict = linkTargets.ToDictionary(linkTarget => linkTarget.it_id, linkTarget => linkTarget.it_title);
    var pageDict = pages.ToDictionary(page => page.page_id, page => page.page_title);
    var pageTitleDict = pages.ToDictionary(page => page.page_title, page => page.page_id);

    object lockObject = new object();

    Parallel.ForEach(pageLinks, new ParallelOptions { MaxDegreeOfParallelism = 12 }, (link, state) =>
    {
      try
      {
        if (linkTargetDict.TryGetValue(link.pl_target_id, out string linkTargetTitle) &&
            pageDict.TryGetValue(link.pl_from, out string sourcePageTitle) &&
            pageTitleDict.TryGetValue(linkTargetTitle, out uint targetPageId))
        {
          var newLine = $"{link.pl_from};{targetPageId}";

          lock (lockObject)
          {
            names[link.pl_from] = sourcePageTitle;
            names[targetPageId] = linkTargetTitle;
            sizes[link.pl_from]++;
            csv.AppendLine(newLine);
            entryCount++;

            if (entryCount % 500 == 0)
            {
              Console.WriteLine($"{entryCount} lines written in {Util.GetTimeElapsed(startTime)} ms! Pagelink: {pageLinks.IndexOf(link)} / {pageLinks.Count()}");
            }
          }
        }
      }
      catch
      {
        // Handle exceptions if necessary
      }
    });

    File.WriteAllText(Path.Combine("output", "Links.csv"), csv.ToString());

    csv.Clear();
    csv.AppendLine("id;title;out");

    Parallel.For(0, names.Length, new ParallelOptions { MaxDegreeOfParallelism = 12 }, i =>
    {
      if (names[i] != null)
      {
        lock (lockObject)
        {
          csv.AppendLine($"{i};{names[i]};{sizes[i]}");
        }
      }
    });

    File.WriteAllText(Path.Combine("output", "Nodes.csv"), csv.ToString());
  }

  public static void ParseCategoryVectors()
  {
    string connectionString = "Server=localhost;Port=3306;Database=wiki;uid=root;pwd=rootpassword;Connection Timeout=60";
    var categories = Reader.LoadCategories("Server=localhost;Port=3306;Database=wiki;uid=root;pwd=rootpassword;Connection Timeout=60");
    var categoryLinks = Reader.LoadCategoryLinks("Server=localhost;Port=3306;Database=wiki;uid=root;pwd=rootpassword;Connection Timeout=60");
    long startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
    List<PageLink> pageLinks = Reader.LoadPageLinks(connectionString);
    Util.ShuffleList(pageLinks);
    Console.WriteLine($"{pageLinks.Count} PageLinks loaded in {Util.GetTimeElapsed(startTime)} ms");
    startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

    List<LinkTarget> linkTargets = Reader.LoadLinkTargets(connectionString);
    Console.WriteLine($"{linkTargets.Count} LinkTargets loaded in {Util.GetTimeElapsed(startTime)} ms");
    startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

    List<Page> pages = Reader.LoadPages(connectionString);
    Console.WriteLine($"{pages.Count} Pages loaded in {Util.GetTimeElapsed(startTime)} ms");
    startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

    startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

    string outputFolder = "output";

    // Ensure the output directory exists
    Directory.CreateDirectory(outputFolder);

    // Write data to CSV file
    ProcessData(pageLinks, linkTargets, pages, startTime);

    Console.WriteLine("Generation done");
    Console.WriteLine(Util.GetTimeElapsed(startTime) + " ms elapsed!");

  }
}