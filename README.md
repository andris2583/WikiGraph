# WikiGraph

Click on node -> highlight connected nodes
Maybe filter out redirects from nodes (uncertain if target node of link can be redirect page, that would make the filtering harder to do without data loss)

## Category links

- Color nodes based on communities, by categories of page
- Only page categories
- Ignore hidden categories, maybe also look into tracker, container categories
  Get categories that are hidden: select cl_from from categorylinks c WHERE c.cl_to ='Hidden_categories'
  Transform these into their page names from id
  Only get category links that's cl_to are not in the queried list of hidden ones
  select \* from categorylinks c WHERE c.cl_to NOT IN (select page_title from page p join (select cl_from from categorylinks c WHERE c.cl_to ='Hidden_categories') as cat on cat.cl_from = page_id )
- Minimum size for categories to be used
