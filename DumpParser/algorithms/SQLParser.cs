using System.Collections.Concurrent;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Data;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;

public static class SQLParser
{
  public static void ParseGraph(string targetDatabase)
  {
    string connectionString = $"Server=localhost;Port=3306;Database={targetDatabase};uid=root;pwd=rootpassword;default command timeout=60000";
    long startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
    List<Page> pages = Reader.LoadPages(connectionString);
    Console.WriteLine($"{pages.Count} Pages loaded in {Util.GetTimeElapsed(startTime)} ms");
    startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

    List<PageLink> pageLinks = Reader.LoadPageLinks(connectionString);
    Console.WriteLine($"{pageLinks.Count} PageLinks loaded in {Util.GetTimeElapsed(startTime)} ms");
    startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

    List<LinkTarget> linkTargets = Reader.LoadLinkTargets(connectionString);
    Console.WriteLine($"{linkTargets.Count} LinkTargets loaded in {Util.GetTimeElapsed(startTime)} ms");
    startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

    startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

    string outputFolder = "output";

    // Ensure the output directory exists
    Directory.CreateDirectory(outputFolder);

    ProcessGraphData(pageLinks, linkTargets, pages, startTime);

    Console.WriteLine("Generation done");
    Console.WriteLine(Util.GetTimeElapsed(startTime) + " ms elapsed!");

  }

  public static void ProcessGraphData(List<PageLink> pageLinks, List<LinkTarget> linkTargets, List<Page> pages, long startTime)
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

    Parallel.For(0, pages.Count, new ParallelOptions { MaxDegreeOfParallelism = 12 }, i =>
    {
      var page = pages[i];
      if (names[page.page_id] != null)
      {
        lock (lockObject)
        {
          csv.AppendLine($"{page.page_id};{page.page_title};{sizes[page.page_id]}");
        }
      }
    });

    File.WriteAllText(Path.Combine("output", "Nodes.csv"), csv.ToString());
  }

  public static void ParseCategoryVectors(string targetDatabase)
  {
    string connectionString = $"Server=localhost;Port=3306;Database={targetDatabase};uid=root;pwd=rootpassword;default command timeout=60000";
    long startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

    var categories = Reader.LoadCategories(connectionString);
    Console.WriteLine($"{categories.Count} Categories loaded in {Util.GetTimeElapsed(startTime)} ms");
    startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

    var categoryLinks = Reader.LoadCategoryLinks(connectionString);
    Console.WriteLine($"{categoryLinks.Count} Category links loaded in {Util.GetTimeElapsed(startTime)} ms");
    startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

    List<Page> pages = Reader.LoadPages(connectionString);
    Console.WriteLine($"{pages.Count} Pages loaded in {Util.GetTimeElapsed(startTime)} ms");
    startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

    startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

    string outputFolder = "output";

    // Ensure the output directory exists
    Directory.CreateDirectory(outputFolder);

    ProcessCategoryData(categories, categoryLinks, pages, startTime);

    Console.WriteLine("Generation done");
    Console.WriteLine(Util.GetTimeElapsed(startTime) + " ms elapsed!");
  }

  private static void ProcessCategoryData(List<string> categories, List<CategoryLink> categoryLinks, List<Page> pages, long startTime)
  {
    // Create a lookup dictionary for quick categoryLink lookups
    var categoryLinkLookup = categoryLinks
        .GroupBy(cl => cl.cl_from)
        .ToDictionary(g => g.Key, g => g.Select(cl => cl.cl_to).ToHashSet());

    Console.WriteLine("Category links have been grouped by page_id.");

    string csvFilePath = Path.Combine("output", "CategoryVectors.csv");
    using (var csvWriter = new StreamWriter(csvFilePath, false, Encoding.UTF8))
    {
      // Write the header
      csvWriter.WriteLine("page_id;page_title;" + string.Join(";", categories));

      var entryCount = 0;

      Parallel.ForEach(pages, new ParallelOptions { MaxDegreeOfParallelism = 12 }, (page) =>
      {
        byte[] categoryVector = new byte[categories.Count];
        if (categoryLinkLookup.TryGetValue(page.page_id, out var linkedCategories))
        {
          for (int i = 0; i < categories.Count; i++)
          {
            if (linkedCategories.Contains(categories[i]))
            {
              categoryVector[i] = 1;
            }
          }
        }

        var line = new StringBuilder();
        line.Append(page.page_id);
        line.Append(';');
        line.Append(page.page_title);
        line.Append(';');
        line.Append(string.Join(";", categoryVector));

        lock (csvWriter)
        {
          csvWriter.WriteLine(line.ToString());
          entryCount++;

          if (entryCount % 500 == 0)
          {
            Console.WriteLine($"{entryCount} lines written in {Util.GetTimeElapsed(startTime)} ms!");
          }
        }
      });
    }

    Console.WriteLine($"CSV file written with {pages.Count} entries.");

    // Perform K-Means clustering on the resulting CSV file
    KMeans.PerformKMeansClustering(Path.Combine("output", "CategoryVectors.csv"), Path.Combine("output", "NodeCoordinates.csv"), 20, startTime);
    // KMeans.PerformBalancedKMeansClustering(Path.Combine("output", "CategoryVectors.csv"), Path.Combine("output", "ClusteredPages.csv"), 50, startTime);
  }

  // Classes for ML.NET data loading and processing
  public class InputData
  {
    [LoadColumn(0)]
    public float PageId { get; set; }

    [LoadColumn(1)]
    public string PageTitle { get; set; }

    [LoadColumn(2, 2 + 50 - 1)] // Assuming categoriesCount is the number of category columns
    public float[] Features { get; set; }
  }

  public class ClusteredData
  {
    [ColumnName("PredictedLabel")]
    public uint PredictedLabel { get; set; }

    [LoadColumn(0)]
    public float PageId { get; set; }

    [LoadColumn(1)]
    public string PageTitle { get; set; }
  }
}