import { Head, Html, Main, NextScript } from 'next/document';

export default function Document() {
  return (
    <Html lang="en">
      <Head />
      <body
        style={{ overflow: 'hidden' }}
        className="dark text-foreground bg-background"
      >
        <Main />
        <NextScript />
      </body>
    </Html>
  );
}
