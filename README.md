# WikiGraph

- Wikipedia page and link visualizer in the form of a directed graph.
- The DumpParser project is responsible for generating the graph, calculating node positions, and classifying nodes into communities, based on wikipedia database dumps.
- The wikigraph nextjs project visualizes the generated data.

## TODO

### Graph visualization

- Click on node -> highlight connected nodes

### Category links

- Color nodes based on communities, by categories of page
- Only page categories
- Ignore hidden categories, maybe also look into tracker, container categories
  Get categories that are hidden: select cl_from from categorylinks c WHERE c.cl_to ='Hidden_categories'
  Transform these into their page names from id
  Only get category links that's cl_to are not in the queried list of hidden ones
- Minimum size for categories to be used
- Filter out categories related to chronology
- Problem
  99% percent of pages are categorized in group 1, for no discernible reason
  The other groups are very samll(less than 200) and hyper specific, like X year births, X year cars, and cathedrals

### Node coordinates

- A large multiplier value is need to upscale the coordinate values, not too much of a problem, but them working out of the box would be preferable
