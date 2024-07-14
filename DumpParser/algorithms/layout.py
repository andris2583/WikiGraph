import graph_tool.all as gt
import csv
import time

def load_graph_from_edge_file(edge_file):
    G = gt.Graph(directed=False)
    vertex_map = {}

    with open(edge_file, 'r') as f:
        reader = csv.DictReader(f, delimiter=';')
        for row in reader:
            source = int(row['source'])
            target = int(row['target'])

            if source not in vertex_map:
                vertex_map[source] = G.add_vertex()
            if target not in vertex_map:
                vertex_map[target] = G.add_vertex()

            G.add_edge(vertex_map[source], vertex_map[target])
    
    return G, vertex_map

def load_node_info(node_file):
    node_info = {}
    with open(node_file, 'r') as f:
        reader = csv.DictReader(f, delimiter=';')
        for row in reader:
            node_id = int(row['id'])
            title = row['title']
            out = int(row['out'])
            node_info[node_id] = {'title': title, 'out': out}
    return node_info

def main():
    edge_file = '../output/Links.csv'  # Change this to your edge file path
    node_file = '../output/Nodes.csv'  # Change this to your node file path
    output_file = '../output/NodeCoordinates.csv'

    print("Loading node information...")
    node_info = load_node_info(node_file)

    print("Loading graph from edge file...")
    start_time = time.time()
    G, vertex_map = load_graph_from_edge_file(edge_file)
    print(f"Graph loaded in {time.time() - start_time:.2f} seconds.")
    print(f"Number of nodes: {G.num_vertices()}, Number of edges: {G.num_edges()}")

    print("Computing layout...")
    start_time = time.time()
    pos = gt.sfdp_layout(
      G,
      verbose = True,
      C=0.1,
      p=0.8,
      K=0.5,
      gamma=1.5,
      epsilon=0.001,
      theta=0.7,
      max_iter=1500
  )
    print(f"Layout computed in {time.time() - start_time:.2f} seconds.")

    print("Saving coordinates to file...")
    with open(output_file, 'w') as f:
        f.write("id;title;out;x;y\n")
        for node_id, v in vertex_map.items():
            x, y = pos[v]
            info = node_info.get(node_id, {'title': 'Unknown', 'out': 0})
            title = info['title']
            out = info['out']
            f.write(f"{node_id};{title};{out};{x};{y}\n")
    print(f"Coordinates saved to {output_file}")

if __name__ == "__main__":
    main()
