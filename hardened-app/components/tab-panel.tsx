'use client'

import { useState } from 'react'
import { Box, Tab, Tabs } from '@mui/material'

export interface ITabPage {
  label: string
  component: JSX.Element
}

interface Props {
  pages: ITabPage[]
}

export default function TabPanel({ pages }: Props) {
  const [value, setValue] = useState(0)

  const handleChange = (event: React.SyntheticEvent, newValue: number) => {
    setValue(newValue)
  }

  return (
    <Box sx={{ width: '100%' }}>
      <Tabs value={value} onChange={handleChange} aria-label="Tab component">
        {pages.map((page, index) => (
          <Tab
            key={index}
            label={page.label}
            style={{ textTransform: 'none' }}
          />
        ))}
      </Tabs>
      {pages[value].component}
    </Box>
  )
}
