using Mysqlx;

class Program
{
  static void Main(string[] args)
  {
    if (args.Length < 2)
    {
      Console.Error.WriteLine("No valid argument was provided for what process to run, please provide on of the following: parse_graph, parse_categories");
      return;
    }
    switch (args[0])
    {
      case "parse_graph":
        SQLParser.ParseGraph(args[1]);
        break;
      case "parse_categories":
        SQLParser.ParseCategoryVectors(args[1]);
        break;
      case "community_detection":
        long startTime = DateTime.Now.Ticks;
        GraphClustering.PerformCommunityDetection("output/NodeCoordinates.csv", "output/Links.csv");
        break;
      default:
        break;
    }
  }

}
