import './globals.css'
import { Inter } from 'next/font/google'
import WalletProvider from '../context/wallet-provider'
import { INetworkType } from '@/utils/neo/interfaces'
import MainNavBar from '@/components/main-nav-bar'

const inter = Inter({ subsets: ['latin'] })

export const metadata = {
  title: 'Hardened App',
  description: 'A portal to Battle Hard smart contract',
}

export default function RootLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <html lang="en">
      <body className={inter.className}>
        <WalletProvider
          options={{
            network: process.env.NEXT_PUBLIC_NETWORK as INetworkType,
          }}
        >
          <MainNavBar />
          {children}
        </WalletProvider>
      </body>
    </html>
  )
}
