class Program
{
  static void Main(string[] args)
  {
    switch (args[0])
    {
      case "parse":
        SQLParser.ParseDump(int.Parse(args[1]));
        break;
      case "calccoords":
        // ForceDirectedGraph.Calculate();
        break;
      default:
        break;
    }
  }

}
