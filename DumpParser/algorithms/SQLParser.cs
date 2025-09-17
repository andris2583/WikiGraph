using System.Collections.Concurrent;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Data;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;

public static class SQLParser
{
  private const int NumberOfClusters = 20;

  public static void ParseGraph(string targetDatabase)
  {
    string connectionString = $"Server=localhost;Port=3306;Database={targetDatabase};uid=root;pwd=rootpassword;default command timeout=60000";
    long startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

    // Open file streams for writing
    using (var linksWriter = new StreamWriter(Path.Combine("output", "Links.csv")))
    using (var nodesWriter = new StreamWriter(Path.Combine("output", "Nodes.csv")))
    {
      // Write headers
      linksWriter.WriteLine("source;target");
      nodesWriter.WriteLine("id;title;out");

      // Process Pages in chunks
      var pageDict = new Dictionary<uint, string>();
      var pageTitleDict = new Dictionary<string, uint>();
      Reader.LoadPages(connectionString, pages =>
      {
        foreach (var page in pages)
        {
          pageDict[page.page_id] = page.page_title;
          pageTitleDict[page.page_title] = page.page_id;

          // Write to Nodes CSV immediately
          nodesWriter.WriteLine($"{page.page_id};{page.page_title.Replace(";", "").Replace(",", "").Replace("\n", "").Replace("\r", "")};0");
        }
        Console.WriteLine($"{pageDict.Count} Pages processed so far in {Util.GetTimeElapsed(startTime)} ms");
      });

      var linkTargetDict = new Dictionary<ulong, string>();

      // Process LinkTargets in chunks
      Reader.LoadLinkTargets(connectionString, linkTargets =>
      {
        foreach (var linkTarget in linkTargets)
        {
          linkTargetDict[linkTarget.it_id] = linkTarget.it_title;
        }
        Console.WriteLine($"{linkTargetDict.Count} LinkTargets processed so far in {Util.GetTimeElapsed(startTime)} ms");
      });

      var entryCount = 0;
      int[] sizes = new int[80000000];

      // Process PageLinks in chunks and write to CSV
      Reader.LoadPageLinks(connectionString, pageLinks =>
      {
        Parallel.ForEach(pageLinks, new ParallelOptions { MaxDegreeOfParallelism = 12 }, (link) =>
              {
                try
                {
                  if (linkTargetDict.TryGetValue(link.pl_target_id, out string linkTargetTitle) &&
                            pageDict.TryGetValue(link.pl_from, out string sourcePageTitle) &&
                            pageTitleDict.TryGetValue(linkTargetTitle.Replace(";", "").Replace(",", "").Replace("\n", "").Replace("\r", ""), out uint targetPageId))
                  {
                    var newLine = $"{link.pl_from};{targetPageId}";

                    lock (linksWriter)
                    {
                      sizes[link.pl_from]++;
                      linksWriter.WriteLine(newLine);
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
      });

      // Update the "out" column in the nodes CSV file
      nodesWriter.BaseStream.Seek(0, SeekOrigin.Begin); // Reset the stream to the beginning
      nodesWriter.WriteLine("id;title;out"); // Rewrite header

      foreach (var page in pageDict)
      {
        if (sizes[page.Key] > 0)
        {
          nodesWriter.WriteLine($"{page.Key};{page.Value};{sizes[page.Key]}");
        }
      }
    }

    Console.WriteLine("Generation done");
    Console.WriteLine(Util.GetTimeElapsed(startTime) + " ms elapsed!");
  }

  public static void ParseCategories(string targetDatabase)
  {
    string connectionString = $"Server=localhost;Port=3306;Database={targetDatabase};uid=root;pwd=rootpassword;default command timeout=60000";
    long startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

    var categories = Reader.LoadCategories(connectionString);
    Console.WriteLine($"{categories.Count} Categories loaded in {Util.GetTimeElapsed(startTime)} ms");
    startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

    var categoryLinks = Reader.LoadCategoryLinks(connectionString);
    Console.WriteLine($"{categoryLinks.Count} Category links loaded in {Util.GetTimeElapsed(startTime)} ms");
    startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

    List<Page> pages = new List<Page>();
    Reader.LoadPages(connectionString, pages =>
      {
        pages.AddRange(pages);
        Console.WriteLine($"{pages.Count} Pages processed so far in {Util.GetTimeElapsed(startTime)} ms");
      });
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
    KMeans.PerformKMeansClustering(Path.Combine("output", "CategoryVectors.csv"), Path.Combine("output", "NodeCoordinates.csv"), NumberOfClusters, startTime);
  }

  public static void AnalyzeCommunities(string targetDatabase)
  {
    var nodes = File.ReadAllLines(Path.Combine("output", "NodeCoordinates.csv"))
      .Select(line => line.Split(';'))
      .ToList();


  }

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