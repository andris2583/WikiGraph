import CustomNavbar from './navbar/customNavbar';
import { Providers } from './providers';

export default function Layout({ children }) {
  return (
    <Providers>
      <div
        style={{ overflowY: 'auto' }}
        className="dark text-foreground bg-background"
      >
        <CustomNavbar />
        <div className="w-screen h-screen p-8 flex items-start justify-center">
          <main>{children}</main>
        </div>
      </div>
    </Providers>
  );
}
