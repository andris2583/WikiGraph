
using System.Net.Sockets;
using System.Text;
using Data;
using MySql.Data.MySqlClient;

class Program
{
    const int READ_LIMIT = 10000;
    static void Main()
    {
        string connectionString = "Server=localhost;Port=3306;Database=wiki;uid=root;pwd=rootpassword;Connection Timeout=60";
        long startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        List<PageLink> pageLinks = LoadPageLinks(connectionString);
        Console.WriteLine($"PageLinks loaded in {GetTimeElapsed(startTime)} ms!");
        startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

        List<LinkTarget> linkTargets = LoadLinkTargets(connectionString);
        Console.WriteLine($"LinkTargets loaded in {GetTimeElapsed(startTime)} ms!");
        startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

        List<Page> pages = LoadPages(connectionString);
        Console.WriteLine($"Pages loaded in {GetTimeElapsed(startTime)} ms!");
        startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

        startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

        string outputFolder = "output";

        // Ensure the output directory exists
        Directory.CreateDirectory(outputFolder);

        // Write data to CSV file
        WriteToCsv(pageLinks, linkTargets, pages, startTime);

        Console.WriteLine("Generation done");
        Console.WriteLine(GetTimeElapsed(startTime) + " ms elapsed!");
    }

    private static long GetTimeElapsed(long startTime)
    {
        return (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - startTime;
    }

    public static List<PageLink> LoadPageLinks(string connectionString)
    {
        List<PageLink> pageLinks = new List<PageLink>();

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();

            string query = $"SELECT pl_from, pl_from_namespace, pl_target_id FROM pagelinks WHERE pl_from_namespace = 0 ORDER BY pl_from ASC, pl_target_id ASC LIMIT {READ_LIMIT}";
            // string query = $"SELECT pl_from, pl_from_namespace, pl_target_id FROM pagelinks WHERE pl_from_namespace = 0 ORDER BY pl_from ASC, pl_target_id ASC";

            MySqlCommand cmd = new MySqlCommand(query, conn);

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    PageLink link = new PageLink
                    {
                        pl_from = reader.GetUInt32("pl_from"),
                        pl_from_namespace = reader.GetInt32("pl_from_namespace"),
                        pl_target_id = reader.GetUInt64("pl_target_id")
                    };

                    pageLinks.Add(link);
                }
            }
        }

        return pageLinks;
    }


    public static List<LinkTarget> LoadLinkTargets(string connectionString)
    {
        List<LinkTarget> linkTargets = new List<LinkTarget>();

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();

            string query = $"SELECT it_id,it_title FROM linktarget WHERE it_namespace = 0 ORDER BY it_id ASC";

            MySqlCommand cmd = new MySqlCommand(query, conn);

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    LinkTarget link = new LinkTarget
                    {
                        it_id = reader.GetUInt32("it_id"),
                        it_title = Encoding.UTF8.GetString((byte[])reader["it_title"])
                    };

                    linkTargets.Add(link);
                }
            }
        }

        return linkTargets;
    }

    public static List<Page> LoadPages(string connectionString)
    {
        List<Page> pages = new List<Page>();

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            conn.Open();

            string query = $"SELECT page_id,page_title FROM page WHERE page_namespace = 0 ORDER BY page_id ASC";

            MySqlCommand cmd = new MySqlCommand(query, conn);

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Page page = new Page
                    {
                        page_id = reader.GetUInt32("page_id"),
                        page_title = Encoding.UTF8.GetString((byte[])reader["page_title"]),
                    };

                    pages.Add(page);
                }
            }
        }

        return pages;
    }

    public static void WriteToCsv(List<PageLink> pageLinks, List<LinkTarget> linkTargets, List<Page> pages, long startTime)
    {
        var csv = new StringBuilder();
        string[] names = new string[1200000];
        int[] sizes = Enumerable.Repeat(0, 1200000).ToArray();
        csv.AppendLine("source;target"); // Header
        var entryCount = 0;
        foreach (var link in pageLinks)
        {
            try
            {
                var linkTargetTitle = linkTargets.Find(linkTarget => linkTarget.it_id == link.pl_target_id)?.it_title;
                var sourcePage = pages.Find(page => page.page_id == link.pl_from);
                var targetPage = pages.Find(page => page.page_title == linkTargetTitle);
                var newLine = $"{link.pl_from};{targetPage!.page_id}";
                names[sourcePage!.page_id] = sourcePage.page_title;
                names[targetPage.page_id] = targetPage.page_title;
                sizes[sourcePage.page_id]++;
                csv.AppendLine(newLine);
                entryCount++;
                if (entryCount % 500 == 0)
                {
                    Console.WriteLine($"${entryCount} lines written in {GetTimeElapsed(startTime)} ms! Pagelink: {pageLinks.IndexOf(link)} / {pageLinks.Count()}");
                }
            }
            catch { }
        }

        File.WriteAllText(Path.Combine("output", "Links.csv"), csv.ToString());

        csv = new StringBuilder();
        csv.AppendLine("id;title;out");
        for (int index = 0; index != names.Length; index++)
        {
            if (names[index] != null)
            {
                csv.AppendLine($"{index};{names[index]};{sizes[index]}");
            }
        }
        File.WriteAllText(Path.Combine("output", "PageNames.csv"), csv.ToString());
    }
}