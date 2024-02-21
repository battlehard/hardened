import { sc } from '@cityofzion/neon-core'
import { NEO_LINE, ONEGATE, MAINNET, TESTNET } from './constant'

export type INetworkType = typeof MAINNET | typeof TESTNET

export interface IWalletStates {
  list: {
    key: IWalletType
    label: string
  }[]
  invokeScript?: sc.ContractCallJson
  network: INetworkType
  connectedWallet?: IConnectedWallet
  isWalletModalActive: boolean
  openWalletModal: () => void
  closeWalletModal: () => void
  connectWallet: (wallet: IWalletType) => void
  disConnectWallet: () => void
  doInvoke: (invokeScript: sc.ContractCallJson) => void
  closeInvoke: () => void
  isNeoLineAvailable: boolean
  isOneGateAvailable: boolean
}

export interface IWalletOptions {
  network: INetworkType
}

export type IWalletType = typeof NEO_LINE | typeof ONEGATE

export interface IConnectedWallet {
  key: IWalletType
  account: any
  provider: any
  balances: IBalance[]
  network: any
}

export interface IBalance {
  contract: string
  symbol: string
  amount: string
}

export interface ITransaction {
  network: INetworkType
  wallet: IWalletType
  status: 'PENDING' | 'SUBMITTED'
  txid: string
  contractHash: string
  method: string
  args: any
  createdAt: string
}

export interface IInvokeScriptJson {
  scriptHash: string
  operation: string
  args?: any[]
  attributes?: any[]
  signers?: any
  network?: string
  extraSystemFee?: string
  extraNetworkFee?: string
  broadcastOverride?: boolean
}
