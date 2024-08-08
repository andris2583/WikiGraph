import Layout from '@/components/layout';

export default function Embedding() {
  return <div>Embedding</div>;
}

Embedding.getLayout = function getLayout(page) {
  return <Layout>{page}</Layout>;
};
