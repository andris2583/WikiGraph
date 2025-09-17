import csv
from graph_tool.all import Graph, sfdp_layout
import time
import sys

# Increase the CSV field size limit
csv.field_size_limit(sys.maxsize)

# Initialize the graph
G = Graph(directed=False)
node_map = {}

# Step 1: Reading Nodes.csv
print("Reading Nodes.csv...")
start_time = time.time()

with open('../output/Nodes.csv', 'r') as nodes_file:
    reader = csv.DictReader(nodes_file, delimiter=';')
    for row in reader:
        try:
            node_id = int(row['id'].strip())
            v = G.add_vertex()
            node_map[node_id] = v
        except ValueError as e:
            print(f"Skipping invalid node entry: {row} ({e})")

print(f"Nodes loaded: {len(node_map)}")
print(f"Time taken: {time.time() - start_time:.2f} seconds")

# Step 2: Reading Links.csv
print("Reading Links.csv...")
start_time = time.time()
edge_count = 0

with open('../output/Links.csv', 'r') as links_file:
    reader = csv.DictReader(links_file, delimiter=';')
    for row in reader:
        try:
            source = int(row['source'].strip())
            target = int(row['target'].strip())
            if source in node_map and target in node_map:
                G.add_edge(node_map[source], node_map[target])
                edge_count += 1
            else:
                print(f"Skipping edge with missing nodes: {row}")
        except ValueError as e:
            print(f"Skipping invalid edge entry: {row} ({e})")

print(f"Edges loaded: {edge_count}")
print(f"Time taken: {time.time() - start_time:.2f} seconds")

# Step 3: Calculating the layout positions (2D coordinates)
print("Calculating node positions using SFDP layout...")
start_time = time.time()

pos = sfdp_layout(G)

print("Node positions calculated.")
print(f"Time taken: {time.time() - start_time:.2f} seconds")

# Step 4: Writing NodeCoordinates.csv
print("Writing NodeCoordinates.csv...")
start_time = time.time()

with open('../output/NodeCoordinates.csv', 'w', newline='') as coord_file:
    fieldnames = ['id', 'title', 'out', 'x', 'y']
    writer = csv.DictWriter(coord_file, fieldnames=fieldnames, delimiter=';')
    writer.writeheader()

    with open('../output/Nodes.csv', 'r') as nodes_file:
        reader = csv.DictReader(nodes_file, delimiter=';')
        for row in reader:
            try:
                node_id = int(row['id'].strip())
                v = node_map[node_id]
                x, y = pos[v]
                writer.writerow({
                    'id': row['id'],
                    'title': row['title'].strip(),
                    'out': row['out'].strip(),
                    'x': x,
                    'y': y
                })
            except ValueError as e:
                print(f"Skipping invalid node entry in output: {row} ({e})")

print("NodeCoordinates.csv has been written.")
print(f"Time taken: {time.time() - start_time:.2f} seconds")
