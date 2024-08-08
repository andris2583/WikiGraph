import { FixedNode } from '@/data/FixedNode';
import { Link } from '@/data/Link';
import { Node } from '@/data/Node';
import { colors } from '@/utils/Constants';
import { parseFixedNodes, parseLinks, parseNodes } from '@/utils/Parser';
import {
  Cosmograph,
  CosmographProvider,
  CosmographRef,
} from '@cosmograph/react';
import { Avatar, Image, Select, SelectItem, Slider } from '@nextui-org/react';
import { promises } from 'fs';
import { GetStaticProps } from 'next';
import NextLink from 'next/link';
import path from 'path';
import { useEffect, useMemo, useRef, useState } from 'react';

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
  }, [nodes]);
  const clusters = useMemo(() => {
    return Array.from({ length: 21 }, (_, index) => index);
  }, []);
  const cosmographRef = useRef<CosmographRef<Node>>(null);
  const [selectedClusters, setSelectedClusters] = useState(new Set<number>([]));
  const [nodeSizeVar, setNodeSizeVar] = useState(1);
  const [minNodeSizeVar, setMinNodeSizeVar] = useState(1);
  useEffect(() => {
    cosmographRef.current?.unselectNodes();
    if (selectedClusters.size > 0) {
      cosmographRef.current?.selectNodes(
        fixedNodes.filter((tempNode) => selectedClusters.has(tempNode.cluster))
      );
    } else {
      cosmographRef.current?.selectNodes(fixedNodes);
    }
  }, [selectedClusters, fixedNodes]);

  const onLabelClick = (node: Node | undefined) => {
    if (node && cosmographRef.current?.getSelectedNodes()?.includes(node)) {
      window
        .open(`https://simple.wikipedia.org/wiki/${node.label}`, '_blank')!
        .focus();
    }
  };

  const onNodeMouseOver = (node: Node | undefined) => {
    if (node) {
      cosmographRef.current?.selectNodes(
        fixedNodes.filter((tempNode) => tempNode.cluster == node.cluster)
      );
    }
  };

  const onNodeMouseOut = (node: Node | undefined) => {
    if (node) {
      cosmographRef.current?.unselectNodes();
    }
  };

  return (
    <div className="flex">
      <CosmographProvider nodes={fixedNodes}>
        <div
          style={{ width: '20vw', minWidth: '200px' }}
          className="flex flex-col items-start justify-start p-4 gap-4"
        >
          <div className="flex flex-row items-center justify-between gap-4">
            <NextLink href="/embedding">
              <Image src="/img/back.svg" alt="" width={25} />
            </NextLink>
            <h2>Simple english wikipedia</h2>
          </div>
          {/* <CosmographSearch
            className="p-2"
            style={{ zIndex: 11 }}
            accessors={[
              {
                label: 'label',
                accessor: (node: Node) => node.label?.replaceAll('_', ' '),
              },
            ]}
          /> */}
          <Select
            label="Select clusters"
            placeholder="Select clusters"
            selectionMode="multiple"
            className="max-w-xs"
            variant="flat"
            selectedKeys={selectedClusters}
            //@ts-ignore
            onSelectionChange={(keys) => setSelectedClusters(keys)}
          >
            {clusters.map((cluster) => (
              <SelectItem
                key={cluster}
                startContent={
                  <Avatar
                    src="/img/cluster.svg"
                    size="sm"
                    className="p-1.5"
                    style={{
                      backgroundColor: colors[cluster] ?? '#FFF',
                    }}
                  />
                }
              >
                {'Cluster ' + cluster + ' '}
              </SelectItem>
            ))}
          </Select>
          <Slider
            size="lg"
            label="Node size"
            maxValue={5}
            minValue={0.1}
            step={0.1}
            value={nodeSizeVar}
            //@ts-ignore
            onChange={setNodeSizeVar}
          ></Slider>
          <Slider
            size="lg"
            label="Minimum node size"
            maxValue={5}
            minValue={0.1}
            step={0.1}
            value={minNodeSizeVar}
            //@ts-ignore
            onChange={setMinNodeSizeVar}
          ></Slider>
        </div>
        <Cosmograph
          style={{ height: '100vh', width: '80vw' }}
          backgroundColor="#1e2428"
          ref={cosmographRef}
          nodeLabelAccessor={(node: Node) => node.label?.replaceAll('_', ' ')}
          spaceSize={8192}
          showFPSMonitor={false}
          onLabelClick={(node) => onLabelClick(node)}
          // onNodeMouseOver={(node) => onNodeMouseOver(node)}
          // onNodeMouseOut={(node) => onNodeMouseOut(node)}
          // @ts-ignore
          nodeColor={(node: FixedNode) => colors[node.cluster] ?? '#FFF'}
          nodeSize={(node) =>
            Math.max(
              4 * 10 * 10 * minNodeSizeVar,
              (node.out / maxOut) * 10 * 20 * 5 * nodeSizeVar
            )
          }
        />
      </CosmographProvider>
    </div>
  );
}
