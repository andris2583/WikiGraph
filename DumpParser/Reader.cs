using System.Text;
using Data;
using MySql.Data.MySqlClient;

public static class Reader
{
  public static List<PageLink> LoadPageLinks(string connectionString)
  {
    List<PageLink> pageLinks = new List<PageLink>();

    using (MySqlConnection conn = new MySqlConnection(connectionString))
    {
      conn.Open();

      string query = $"SELECT pl_from, pl_from_namespace, pl_target_id FROM pagelinks WHERE pl_from_namespace = 0 ORDER BY pl_from ASC, pl_target_id ASC";

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

}