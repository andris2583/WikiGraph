import { FixedNode } from '@/data/FixedNode';
import Papa from 'papaparse';
import { Link } from '../data/Link';
import { Node } from '../data/Node';

export const parseLinks = (fileContent: string): Link[] => {
  const links: Link[] = [];

  Papa.parse(fileContent, {
    header: true,
    skipEmptyLines: true,
    delimiter: ';',
    complete: (results: any) => {
      results.data.forEach((row: any) => {
        links.push({ source: row.source, target: row.target });
      });
    },
  });

  return links;
};

export const parseNodes = (fileContent: string): Node[] => {
  const nodes: Node[] = [];

  Papa.parse(fileContent, {
    header: true,
    skipEmptyLines: true,
    delimiter: ';',
    complete: (results: any) => {
      results.data.forEach((row: any) => {
        nodes.push({ id: row.id, label: row.title, out: row.out ?? 0 });
      });
    },
  });

  return nodes;
};

export const parseFixedNodes = (fileContent: string): FixedNode[] => {
  const fixedNodes: FixedNode[] = [];

  Papa.parse(fileContent, {
    header: true,
    skipEmptyLines: true,
    delimiter: ';',
    complete: (results: any) => {
      results.data.forEach((row: any) => {
        if (String(row.title).length > 500) {
          console.log(row.id);
        }
        fixedNodes.push({
          id: row.id,
          label: row.title,
          out: row.out ?? 0,
          x: row.x * 10000,
          y: row.y * 10000,
        });
      });
    },
  });

  return fixedNodes;
};
