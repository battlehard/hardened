import { CONST as NEON_CONST } from '@cityofzion/neon-core'
import { IWalletType } from './interfaces'

/* Wallets */
export const ONEGATE = 'ONEGATE'
export const NEO_LINE = 'NEO_LINE'

export const WALLET_LIST: {
  label: string
  key: IWalletType
}[] = [
  {
    label: 'NEO Line',
    key: NEO_LINE,
  },
  {
    label: 'ONEGATE',
    key: ONEGATE,
  },
]

/* Network types */
export const TESTNET = 'N3TestNet'
export const MAINNET = 'N3MainNet'

export const TESTNET_CONFIG = {
  label: 'N3TestNet',
  url: 'https://testnet1.neo.coz.io:443',
}

export const MAINNET_CONFIG = {
  label: 'N3MainNet',
  url: 'https://mainnet1.neo.coz.io:443',
}

/* Contract hashes */
export const GAS_SCRIPT_HASH = NEON_CONST.NATIVE_CONTRACT_HASH.GasToken
export const NEO_SCRIPT_HASH = NEON_CONST.NATIVE_CONTRACT_HASH.NeoToken

/* Network config */
export const MAX_RPC_RESULT = 10000
