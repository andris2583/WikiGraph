import { FixedNode } from '@/data/FixedNode';
import { Link } from '@/data/Link';
import { Node } from '@/data/Node';
import { colors } from '@/utils/Constants';
import { parseFixedNodes, parseLinks, parseNodes } from '@/utils/Parser';
import { Cosmograph } from '@cosmograph/react';
import { promises } from 'fs';
import { GetStaticProps } from 'next';
import path from 'path';
import { useMemo } from 'react';

export const getStaticProps: GetStaticProps = async () => {
  const linksFilePath = path.join(process.cwd(), 'public', '1mil', 'Links.csv');
  const nodesFilePath = path.join(process.cwd(), 'public', '1mil', 'Nodes.csv');
  const fixedNodesFilePath = path.join(
    process.cwd(),
    'public',
    'NodeCoordinates.csv'
  );

  // Read the file contents
  const linksFileContent = await promises.readFile(linksFilePath, 'utf-8');
  const nodesFileContent = await promises.readFile(nodesFilePath, 'utf-8');
  const fixedNodesFileContent = await promises.readFile(
    fixedNodesFilePath,
    'utf-8'
  );

  // Parse the CSV data into JavaScript objects
  const links = parseLinks(linksFileContent);
  const nodes = parseNodes(nodesFileContent);
  const fixedNodes = parseFixedNodes(fixedNodesFileContent);

  return {
    props: {
      links,
      nodes,
      fixedNodes,
    },
  };
};

export default function Simple({
  links,
  nodes,
  fixedNodes,
}: {
  links: Link[];
  nodes: Node[];
  fixedNodes: FixedNode[];
}) {
  const maxOut = useMemo(() => {
    return nodes.map((node) => Number(node.out)).sort((a, b) => a - b)[
      nodes.length - 1
    ];
  }, []);
  const onNodeClick = (node: FixedNode | undefined) => {
    window
      .open(`https://simple.wikipedia.org/wiki/${node!.label}`, '_blank')!
      .focus();
  };

  return (
    // <Cosmograph
    //   nodes={nodes}
    //   links={links}
    //   nodeLabelAccessor={(node: Node) => node.label}
    //   // simulationRepulsion={0.7}
    //   // simulationLinkSpring={0.05}
    //   // simulationGravity={0.1}
    //   spaceSize={8192}
    //   showFPSMonitor={true}
    //   randomSeed={Math.random()}
    // />
    <Cosmograph
      nodes={fixedNodes}
      nodeLabelAccessor={(node: Node) => node.label + ' - ' + node.out}
      spaceSize={8192}
      showFPSMonitor={true}
      onClick={(node) => onNodeClick(node)}
      // @ts-ignore
      nodeColor={(node) => colors[node.cluster] ?? '#FFF'}
      nodeSize={(node) =>
        Math.max(4 * 10 * 10, (node.out / maxOut) * 10 * 20 * 5)
      }
      // nodeSize={1}
    />
  );
}
