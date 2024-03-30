'use client'

import UserPage from '@/components/pages/user'
import AdminPage from '@/components/pages/admin'
import TabPanel, { ITabPage } from '@/components/tab-panel'
import ContractDataPage from '@/components/pages/contract-data'

const pages: ITabPage[] = [
  { label: 'Admin', component: <AdminPage /> },
  { label: 'User', component: <UserPage /> },
  { label: 'Contract Data', component: <ContractDataPage /> },
]

export default function Home() {
  return (
    <main className="flex min-h-screen flex-col">
      <TabPanel pages={pages} />
    </main>
  )
}
