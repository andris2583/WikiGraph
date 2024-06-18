public class FixedNode
{
  public int Id { get; set; }
  public string Title { get; set; }
  public int Out { get; set; }
  public double X { get; set; }
  public double Y { get; set; }

  public FixedNode(int Id, string Title, int Out, int X = 0, int Y = 0)
  {
    this.Id = Id;
    this.Title = Title;
    this.Out = Out;
    this.X = X;
    this.Y = Y;
  }

}