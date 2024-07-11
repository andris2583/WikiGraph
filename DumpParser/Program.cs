class Program
{
  static void Main(string[] args)
  {
    switch (args[0])
    {
      case "parse_graph":
        SQLParser.ParseGraph();
        break;
      case "parse_categories":
        SQLParser.ParseCategoryVectors();
        break;
      default:
        break;
    }
  }

}
