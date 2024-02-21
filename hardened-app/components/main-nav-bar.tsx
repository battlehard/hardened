'use client'

import React from 'react'

import { NEO_LINE_LOGO, ONEGATE_LOGO } from './constant'
import {
  AppBar,
  Button,
  Drawer,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Toolbar,
} from '@mui/material'
import DeleteIcon from '@mui/icons-material/Delete'

import { NEO_LINE, ONEGATE } from '@/utils/neo/constant'
import { useWallet } from '@/context/wallet-provider'
import { AddressShorten } from '@/utils/app-helpers'

const MainNavBar = () => {
  const {
    connectWallet,
    disConnectWallet,
    connectedWallet,
    list,
    isWalletModalActive,
    openWalletModal,
    closeWalletModal,
    isNeoLineAvailable,
    isOneGateAvailable,
  } = useWallet()

  return (
    <AppBar position="static">
      <Toolbar
        style={{
          display: 'flex',
          flexDirection: 'column',
          flexWrap: 'wrap',
          alignContent: 'flex-end',
          justifyContent: 'center',
          height: '64px',
        }}
      >
        {connectedWallet && (
          <Button
            style={{ borderRadius: '15px', textTransform: 'none' }}
            variant="contained"
            onClick={disConnectWallet}
            endIcon={<DeleteIcon />}
            color="secondary"
          >
            {AddressShorten(connectedWallet?.account.address)}
          </Button>
        )}
        {!connectedWallet && (
          <Button
            style={{
              textTransform: 'none',
              width: '150px',
              fontSize: '1rem',
              margin: '0px 15px',
            }}
            variant="contained"
            onClick={openWalletModal}
          >
            Connect Wallet
          </Button>
        )}
      </Toolbar>
      <Drawer
        anchor="right"
        open={isWalletModalActive}
        onClose={closeWalletModal}
        style={{
          width: '300px',
        }}
      >
        {!connectedWallet && (
          <List>
            {list.map((_wallet) => {
              const walletKey = _wallet.key
              let isDisable = true
              let walletName = ''
              let imgSrc = ''
              if (walletKey == NEO_LINE) {
                isDisable = !isNeoLineAvailable
                walletName = 'NeoLine (Extension)'
                imgSrc = NEO_LINE_LOGO
              } else if (walletKey == ONEGATE) {
                isDisable = !isOneGateAvailable
                walletName = 'OneGate'
                imgSrc = ONEGATE_LOGO
              }

              return (
                <ListItem key={walletKey} disablePadding>
                  <ListItemButton
                    disabled={isDisable}
                    onClick={() => {
                      connectWallet(walletKey)
                    }}
                  >
                    <ListItemIcon>
                      <img src={imgSrc} style={{ marginRight: '10px' }} />
                    </ListItemIcon>
                    <ListItemText primary={walletName} />
                  </ListItemButton>
                </ListItem>
              )
            })}
          </List>
        )}
      </Drawer>
    </AppBar>
  )
}

export default MainNavBar
