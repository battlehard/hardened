'use client'

import UserPage from '@/components/pages/user'
import AdminPage from '@/components/pages/admin'
import TabPanel, { ITabPage } from '@/components/tab-panel'

const pages: ITabPage[] = [
  { label: 'Admin', component: <AdminPage /> },
  { label: 'User', component: <UserPage /> },
]

export default function Home() {
  return (
    <main className="flex min-h-screen flex-col">
      <TabPanel pages={pages} />
    </main>
  )
}
