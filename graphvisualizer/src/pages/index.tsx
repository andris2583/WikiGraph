import Image from "next/image";
import { Inter } from "next/font/google";
import { Link } from "@/data/Link";
import { Node } from "@/data/Node";
import { Cosmograph } from "@cosmograph/react";
import { GetStaticProps } from "next";
import path from "path";
import { parseLinks, parseNodes } from "@/utils/Parser";
import fs from "fs";
import { useEffect, useMemo } from "react";

export const getStaticProps: GetStaticProps = async () => {
  const linksResponse = await fetch(`http://localhost:3000/links.csv`);
  const linksFileContent = await linksResponse.text();
  const links = parseLinks(linksFileContent);
  const nodesResponse = await fetch(`http://localhost:3000/Nodes.csv`);
  const nodesFileContent = await nodesResponse.text();
  const nodes = parseNodes(nodesFileContent);
  return {
    props: {
      links,
      nodes,
    },
  };
};

export default function Home({
  links,
  nodes,
}: {
  links: Link[];
  nodes: Node[];
}) {
  const maxOut = useMemo(() => {
    return nodes.map((node) => Number(node.out)).sort((a, b) => a - b)[
      nodes.length - 1
    ];
  }, []);

  return (
    <Cosmograph
      nodes={nodes}
      links={links}
      nodeLabelAccessor={(node: Node) => node.label}
      simulationRepulsion={0.6}
      simulationLinkSpring={0.05}
      // disableSimulation={true}
      // spaceSize={8192}
      showFPSMonitor={true}
      randomSeed={Math.random()}
      nodeSize={(node) => Math.max(4, (node.out / maxOut) * 4 * 10)}
    />
  );
}
