using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class GraphClustering
{
  public static void PerformCommunityDetection(string nodesFilePath, string edgesFilePath)
  {
    var nodes = LoadNodes(nodesFilePath);
    var edges = LoadEdges(edgesFilePath);
    var clusters = DetectCommunities(nodes, edges);

    UpdateNodeClusters(nodesFilePath, clusters.ToDictionary());
  }

  private static List<Node> LoadNodes(string filePath)
  {
    return File.ReadAllLines(filePath)
        .Skip(1) // Skip header
        .Select(line =>
        {
          var parts = line.Split(';');
          return new Node
          {
            Id = long.Parse(parts[0]),
            Title = parts[1],
            Out = int.Parse(parts[2]),
            X = double.Parse(parts[3]),
            Y = double.Parse(parts[4]),
            Cluster = int.Parse(parts[5])
          };
        })
        .ToList();
  }

  private static List<Edge> LoadEdges(string filePath)
  {
    return File.ReadAllLines(filePath)
        .Skip(1) // Skip header
        .Select(line =>
        {
          var parts = line.Split(';');
          return new Edge
          {
            Source = long.Parse(parts[0]),
            Target = long.Parse(parts[1])
          };
        })
        .ToList();
  }

  private static ConcurrentDictionary<long, long> DetectCommunities(List<Node> nodes, List<Edge> edges)
  {
    // Initialize clusters (each node is its own community initially)
    var clusters = new ConcurrentDictionary<long, long>(nodes.ToDictionary(node => node.Id, node => node.Id));

    bool changed;
    int iteration = 0;

    do
    {
      changed = false;
      iteration++;

      Parallel.ForEach(nodes, node =>
      {
        var bestCluster = clusters[node.Id];
        var bestScore = ModularityGain(clusters, edges, node.Id, bestCluster);

        foreach (var neighbor in edges.Where(e => e.Source == node.Id || e.Target == node.Id))
        {
          var neighborId = neighbor.Source == node.Id ? neighbor.Target : neighbor.Source;
          var score = ModularityGain(clusters, edges, node.Id, clusters[neighborId]);
          if (score > bestScore)
          {
            bestCluster = clusters[neighborId];
            bestScore = score;
            changed = true;
          }
        }

        clusters[node.Id] = bestCluster;
      });

      Console.WriteLine($"Iteration {iteration} completed.");
    } while (changed);

    return clusters;
  }

  private static double ModularityGain(ConcurrentDictionary<long, long> clusters, List<Edge> edges, long nodeId, long newCluster)
  {
    // A simple modularity gain calculation (not the full modularity, just an approximation)
    double gain = 0.0;
    foreach (var edge in edges)
    {
      if (clusters[edge.Source] == clusters[nodeId] && clusters[edge.Target] == clusters[newCluster])
      {
        gain += 1.0;
      }
      else if (clusters[edge.Source] == clusters[newCluster] && clusters[edge.Target] == clusters[nodeId])
      {
        gain += 1.0;
      }
    }
    return gain;
  }

  private static void UpdateNodeClusters(string nodesFilePath, Dictionary<long, long> clusters)
  {
    var nodes = File.ReadAllLines(nodesFilePath).ToList();
    var headers = nodes[0];
    var clusterIndex = headers.Split(';').Length - 1;

    for (int i = 1; i < nodes.Count; i++)
    {
      var parts = nodes[i].Split(';');
      var nodeId = long.Parse(parts[0]);
      parts[clusterIndex] = clusters[nodeId].ToString();
      nodes[i] = string.Join(";", parts);
    }

    File.WriteAllLines("output/test.csv", nodes);
    Console.WriteLine($"Clustered data written to {nodesFilePath}");
  }
}

public class Node
{
  public long Id { get; set; }
  public string Title { get; set; }
  public int Out { get; set; }
  public double X { get; set; }
  public double Y { get; set; }
  public int Cluster { get; set; }
}

public class Edge
{
  public long Source { get; set; }
  public long Target { get; set; }
}
