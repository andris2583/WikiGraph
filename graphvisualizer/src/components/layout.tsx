import CustomNavbar from './navbar/customNavbar';

export default function Layout({ children }) {
  return (
    <div style={{ overflowY: 'auto' }}>
      <CustomNavbar />
      <div className="w-screen h-screen p-8 flex items-start justify-center">
        <main>{children}</main>
      </div>
    </div>
  );
}
