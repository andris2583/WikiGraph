using System.Text;
using Data;
using MySql.Data.MySqlClient;

public static class Reader
{
  // public static List<PageLink> LoadPageLinks(string connectionString)
  // {
  //   List<PageLink> pageLinks = new List<PageLink>();
  //   int offset = 0;
  //   int limit = 1000000;

  //   using (MySqlConnection conn = new MySqlConnection(connectionString))
  //   {
  //     conn.Open();
  //     while (true)
  //     {
  //       string query = $"SELECT pl_from, pl_from_namespace, pl_target_id FROM pagelinks WHERE pl_from_namespace = 0 ORDER BY pl_from ASC, pl_target_id ASC LIMIT {limit} OFFSET {offset}";

  //       MySqlCommand cmd = new MySqlCommand(query, conn);

  //       using (MySqlDataReader reader = cmd.ExecuteReader())
  //       {
  //         if (!reader.HasRows) break;

  //         while (reader.Read())
  //         {
  //           PageLink link = new PageLink
  //           {
  //             pl_from = reader.GetUInt32("pl_from"),
  //             pl_from_namespace = reader.GetInt32("pl_from_namespace"),
  //             pl_target_id = reader.GetUInt64("pl_target_id")
  //           };

  //           pageLinks.Add(link);
  //         }
  //       }
  //       offset += limit;
  //     }
  //   }

  //   return pageLinks;
  // }

  // public static List<LinkTarget> LoadLinkTargets(string connectionString)
  // {
  //   List<LinkTarget> linkTargets = new List<LinkTarget>();
  //   int offset = 0;
  //   int limit = 1000000;

  //   using (MySqlConnection conn = new MySqlConnection(connectionString))
  //   {
  //     conn.Open();
  //     while (true)
  //     {
  //       string query = $"SELECT it_id, it_title FROM linktarget WHERE it_namespace = 0 ORDER BY it_id ASC LIMIT {limit} OFFSET {offset}";

  //       MySqlCommand cmd = new MySqlCommand(query, conn);

  //       using (MySqlDataReader reader = cmd.ExecuteReader())
  //       {
  //         if (!reader.HasRows) break;

  //         while (reader.Read())
  //         {
  //           LinkTarget link = new LinkTarget
  //           {
  //             it_id = reader.GetUInt32("it_id"),
  //             it_title = Encoding.UTF8.GetString((byte[])reader["it_title"])
  //           };

  //           linkTargets.Add(link);
  //         }
  //       }
  //       offset += limit;
  //     }
  //   }

  //   return linkTargets;
  // }

  // public static List<Page> LoadPages(string connectionString)
  // {
  //   List<Page> pages = new List<Page>();
  //   int offset = 0;
  //   int limit = 1000000;

  //   using (MySqlConnection conn = new MySqlConnection(connectionString))
  //   {
  //     conn.Open();
  //     while (true)
  //     {
  //       string query = $"SELECT page_id, page_title FROM page WHERE page_namespace = 0 AND page_is_redirect = 0 ORDER BY page_id ASC LIMIT {limit} OFFSET {offset}";

  //       MySqlCommand cmd = new MySqlCommand(query, conn);

  //       using (MySqlDataReader reader = cmd.ExecuteReader())
  //       {
  //         if (!reader.HasRows) break;

  //         while (reader.Read())
  //         {
  //           Page page = new Page
  //           {
  //             page_id = reader.GetUInt32("page_id"),
  //             page_title = Encoding.UTF8.GetString((byte[])reader["page_title"]),
  //           };

  //           pages.Add(page);
  //         }
  //       }
  //       offset += limit;
  //     }
  //   }

  //   return pages;
  // }
  public static void LoadPageLinks(string connectionString, Action<List<PageLink>> processChunk)
  {
    // int offset = 0;
    int offset = 148288209;
    int limit = 10000000;

    using (MySqlConnection conn = new MySqlConnection(connectionString))
    {
      conn.Open();
      while (true)
      {
        List<PageLink> pageLinks = new List<PageLink>();
        string query = $"SELECT pl_from, pl_from_namespace, pl_target_id FROM pagelinks WHERE pl_from_namespace = 0 LIMIT {limit} OFFSET {offset}";

        MySqlCommand cmd = new MySqlCommand(query, conn);

        using (MySqlDataReader reader = cmd.ExecuteReader())
        {
          if (!reader.HasRows) break;

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
        processChunk(pageLinks);
        offset += limit;
      }
    }
  }

  public static void LoadLinkTargets(string connectionString, Action<List<LinkTarget>> processChunk)
  {
    int offset = 0;
    int limit = 1000000;

    using (MySqlConnection conn = new MySqlConnection(connectionString))
    {
      conn.Open();
      while (true)
      {
        List<LinkTarget> linkTargets = new List<LinkTarget>();
        string query = $"SELECT it_id, it_title FROM linktarget WHERE it_namespace = 0 LIMIT {limit} OFFSET {offset}";

        MySqlCommand cmd = new MySqlCommand(query, conn);

        using (MySqlDataReader reader = cmd.ExecuteReader())
        {
          if (!reader.HasRows) break;

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
        processChunk(linkTargets);
        offset += limit;
      }
    }
  }

  public static void LoadPages(string connectionString, Action<List<Page>> processChunk)
  {
    int offset = 0;
    int limit = 1000000;

    using (MySqlConnection conn = new MySqlConnection(connectionString))
    {
      conn.Open();
      while (true)
      {
        List<Page> pages = new List<Page>();
        string query = $"SELECT page_id, page_title FROM page WHERE page_namespace = 0 AND page_is_redirect = 0 LIMIT {limit} OFFSET {offset}";

        MySqlCommand cmd = new MySqlCommand(query, conn);

        using (MySqlDataReader reader = cmd.ExecuteReader())
        {
          if (!reader.HasRows) break;

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
        processChunk(pages);
        offset += limit;
      }
    }
  }

  public static List<string> LoadCategories(string connectionString)
  {
    List<string> categories = new List<string>();

    using (MySqlConnection conn = new MySqlConnection(connectionString))
    {
      conn.Open();
      string query = "SELECT DISTINCT(cl_to) as category_name, COUNT(cl_to) as cn FROM categorylinks GROUP BY cl_to HAVING cn > 100 ORDER BY cn DESC";

      MySqlCommand cmd = new MySqlCommand(query, conn);

      using (MySqlDataReader reader = cmd.ExecuteReader())
      {
        while (reader.Read())
        {
          categories.Add(Encoding.UTF8.GetString((byte[])reader["category_name"]));
        }
      }
    }

    return categories;
  }

  public static List<CategoryLink> LoadCategoryLinks(string connectionString)
  {
    List<CategoryLink> categoryLinks = new List<CategoryLink>();
    int offset = 0;
    int limit = 1000000;

    using (MySqlConnection conn = new MySqlConnection(connectionString))
    {
      conn.Open();
      while (true)
      {
        string query = $"SELECT cl_from, cl_to FROM categorylinks c WHERE c.cl_type = 'page' LIMIT {limit} OFFSET {offset}";

        MySqlCommand cmd = new MySqlCommand(query, conn);

        using (MySqlDataReader reader = cmd.ExecuteReader())
        {
          if (!reader.HasRows) break;

          while (reader.Read())
          {
            categoryLinks.Add(new CategoryLink
            {
              cl_from = reader.GetUInt32("cl_from"),
              cl_to = Encoding.UTF8.GetString((byte[])reader["cl_to"])
            });
          }
        }
        offset += limit;
      }
    }

    return categoryLinks;
  }
}
