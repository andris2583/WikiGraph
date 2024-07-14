using static SQLParser;
using System.Text;
using Microsoft.ML;

public static class KMeans
{
  public static void PerformKMeansClustering(string inputFilePath, string nodesFilePath, int numberOfClusters, long startTime)
  {
    var context = new MLContext();

    // Load the data
    var data = context.Data.LoadFromTextFile<InputData>(
        path: inputFilePath,
        hasHeader: true,
        separatorChar: ';'
    );

    // Define the training pipeline
    var pipeline = context.Transforms.Concatenate("Features", nameof(InputData.Features))
        .Append(context.Transforms.NormalizeMinMax("Features")) // Feature scaling  
        .Append(context.Clustering.Trainers.KMeans(
            featureColumnName: "Features",
            numberOfClusters: numberOfClusters));

    // Train the model
    var model = pipeline.Fit(data);

    // Make predictions
    var predictions = model.Transform(data);

    // Extract the clustered data
    var clusteredData = context.Data.CreateEnumerable<ClusteredData>(predictions, reuseRowObject: false).ToList();

    var nodes = File.ReadAllLines(nodesFilePath)
    .Select(line => line.Split(';'))
    .ToList();

    // Get headers and find the index of the "cluster" column
    var headers = nodes[0];
    int clusterIndex = Array.IndexOf(headers, "cluster");

    // If "cluster" column does not exist, add it
    if (clusterIndex == -1)
    {
      headers = headers.Append("cluster").ToArray();
      clusterIndex = headers.Length - 1;
      nodes[0] = headers;
    }

    // Create a dictionary for quick look-up of cluster results
    var clusterDict = clusteredData.ToDictionary(cd => cd.PageId, cd => cd.PredictedLabel);

    // Update nodes with cluster information
    for (int i = 1; i < nodes.Count; i++)
    {
      var node = nodes[i];
      float pageId = float.Parse(node[0]);
      if (clusterDict.TryGetValue(pageId, out uint clusterId))
      {
        if (node.Length <= clusterIndex)
        {
          node = node.Append(clusterId.ToString()).ToArray();
        }
        else
        {
          node[clusterIndex] = clusterId.ToString();
        }
      }
      nodes[i] = node;
    }

    // Write the updated nodes back to Nodes.csv
    File.WriteAllLines(nodesFilePath, nodes.Select(line => string.Join(";", line)));

    Console.WriteLine($"Clustered data written to {nodesFilePath}");
  }

  public static void PerformBalancedKMeansClustering(string inputFilePath, string outputFilePath, int numberOfClusters, long startTime)
  {
    var context = new MLContext();

    // Load the data
    var data = context.Data.LoadFromTextFile<InputData>(
        path: inputFilePath,
        hasHeader: true,
        separatorChar: ';'
    );

    // Define the training pipeline
    var pipeline = context.Transforms.Concatenate("Features", nameof(InputData.Features))
        .Append(context.Transforms.NormalizeMinMax("Features")) // Feature scaling  
        .Append(context.Clustering.Trainers.KMeans(
            featureColumnName: "Features",
            numberOfClusters: numberOfClusters));

    // Train the model
    var model = pipeline.Fit(data);

    // Make predictions
    var predictions = model.Transform(data);

    // Extract the clustered data
    var clusteredData = context.Data.CreateEnumerable<ClusteredData>(predictions, reuseRowObject: false).ToList();

    // Balance the clusters
    var balancedClusters = BalanceClusters(clusteredData, numberOfClusters);

    // Write the results to a new CSV file
    using (var csvWriter = new StreamWriter(outputFilePath, false, Encoding.UTF8))
    {
      csvWriter.WriteLine("page_id;page_title;cluster_id");

      var entryCount = 0;

      foreach (var item in balancedClusters)
      {
        csvWriter.WriteLine($"{item.PageId};{item.PageTitle};{item.PredictedLabel}");
        entryCount++;

        if (entryCount % 500 == 0)
        {
          Console.WriteLine($"{entryCount} lines written in {Util.GetTimeElapsed(startTime)} ms!");
        }
      }
    }

    Console.WriteLine($"Clustered data written to {outputFilePath}");
  }

  private static List<ClusteredData> BalanceClusters(List<ClusteredData> clusteredData, int numberOfClusters)
  {
    var clusters = clusteredData.GroupBy(c => c.PredictedLabel)
                                .OrderBy(g => g.Count())
                                .ToList();

    var balancedClusters = new List<ClusteredData>();

    int targetSize = clusteredData.Count / numberOfClusters;

    foreach (var cluster in clusters)
    {
      if (cluster.Count() > targetSize)
      {
        balancedClusters.AddRange(cluster.Take(targetSize));
      }
      else
      {
        balancedClusters.AddRange(cluster);
      }
    }

    return balancedClusters;
  }


}