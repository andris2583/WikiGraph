public static class Util
{
  public static void ShuffleList<T>(List<T> list)
  {
    Random rng = new Random();
    int n = list.Count;
    while (n > 1)
    {
      n--;
      int k = rng.Next(n + 1);
      T value = list[k];
      list[k] = list[n];
      list[n] = value;
    }
  }

  public static long GetTimeElapsed(long startTime)
  {
    return (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - startTime;
  }

}