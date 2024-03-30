'use client'

import { Alert, AlertColor, AlertProps, Snackbar } from '@mui/material'
import React from 'react'

const AppAlert = React.forwardRef<HTMLDivElement, AlertProps>(function AppAlert(
  props,
  ref
) {
  return <Alert elevation={6} ref={ref} variant="filled" {...props} />
})

interface Props {
  open: boolean
  handleClose: any //function
  severity: AlertColor
  message: string
}

export default function Notification({
  open,
  handleClose,
  severity,
  message,
}: Props) {
  return (
    <Snackbar open={open} autoHideDuration={5000} onClose={handleClose}>
      <AppAlert
        onClose={handleClose}
        severity={severity}
        sx={{ width: '100%' }}
      >
        {message}
      </AppAlert>
    </Snackbar>
  )
}
