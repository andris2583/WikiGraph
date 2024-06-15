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
    const pages: Node[] = [];

    Papa.parse(fileContent, {
        header: true,
        skipEmptyLines: true,
        delimiter: ';',
        complete: (results: any) => {
            results.data.forEach((row: any) => {
                pages.push({ id: row.id, label: row.title, out: row.out });
            });
        },
    });

    return pages;
};
