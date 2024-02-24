import { GAS_SCRIPT_HASH, MAINNET, TESTNET } from '../constant'
import {
  IConnectedWallet,
  IInvokeScriptJson,
  INetworkType,
} from '../interfaces'

import { WalletAPI } from '../wallet'
import { stackJsonToObject } from '../helpers'
import { Network } from '../network'
import { wallet as NeonWallet, tx } from '@cityofzion/neon-core'
import { MAX_PAGE_LIMIT } from '@/components/constant'

export enum ManageAdminAction {
  SET = 'Set',
  DELETE = 'Delete',
  GET = 'Get',
}

export enum ReadMethod {
  GET_ADMIN = 'getAdmin',
  GET_BLUEPRINT_IMAGE_URL = 'getBlueprintImageUrl',
}

export const HARDENED_SCRIPT_HASH = {
  [TESTNET]: '0xefb1125ce1cf90476b1b1e049be07d81e6be3420',
  [MAINNET]: '',
}

export interface IPendingProperties {
  clientPubKey: string
  contractPubKey: string
  userWalletHash: string
  payTokenHash: string
  payTokenAmount: number
  gasAmount: number
  bhNftId: string
  slotNftHashes: string[]
  slotNftIds: string[]
}

export interface IPendingListPagination {
  totalPages: number
  totalPending: number
  pendingList: IPendingProperties[]
}

export interface IFeeStructure {
  bTokenMintCost: number
  bTokenUpdateCost: number
  gasMintCost: number
  gasUpdateCost: number
  walletPoolHash: string
}

export class HardenedContract {
  network: INetworkType
  contractHash: string

  constructor(networkType: INetworkType) {
    this.network = networkType
    this.contractHash = HARDENED_SCRIPT_HASH[networkType]
  }

  ManageAdmin = async (
    connectedWallet: IConnectedWallet,
    adminWalletHash: string,
    action: ManageAdminAction
  ): Promise<string> => {
    const invokeScript: IInvokeScriptJson = {
      operation: `${action.toLowerCase()}Admin`,
      scriptHash: this.contractHash,
      args: [
        {
          type: 'Hash160',
          value: adminWalletHash,
        },
      ],
    }

    return this.invoke(connectedWallet, invokeScript)
  }

  SetBlueprintImageUrl = async (
    connectedWallet: IConnectedWallet,
    blueprintImageUrl: string
  ): Promise<string> => {
    const invokeScript: IInvokeScriptJson = {
      operation: 'setBlueprintImageUrl',
      scriptHash: this.contractHash,
      args: [
        {
          type: 'String',
          value: blueprintImageUrl,
        },
      ],
    }

    return this.invoke(connectedWallet, invokeScript)
  }

  FeeUpdate = async (
    connectedWallet: IConnectedWallet,
    feeStructure: IFeeStructure
  ): Promise<string> => {
    const invokeScript: IInvokeScriptJson = {
      operation: 'feeUpdate',
      scriptHash: this.contractHash,
      args: [
        {
          type: 'Integer',
          value: feeStructure.bTokenMintCost,
        },
        {
          type: 'Integer',
          value: feeStructure.bTokenUpdateCost,
        },
        {
          type: 'Integer',
          value: feeStructure.gasMintCost,
        },
        {
          type: 'Integer',
          value: feeStructure.gasUpdateCost,
        },
        {
          type: 'Hash160',
          value: feeStructure.walletPoolHash,
        },
      ],
    }

    return this.invoke(connectedWallet, invokeScript)
  }

  Read = async (readMethod: ReadMethod, args = []): Promise<any> => {
    const invokeScript: IInvokeScriptJson = {
      operation: readMethod,
      scriptHash: this.contractHash,
      args: args,
    }

    const res = await Network.read(this.network, [invokeScript])
    console.log(res)
    return stackJsonToObject(res.stack[0])
  }

  private invoke(
    connectedWallet: IConnectedWallet,
    invokeScript: IInvokeScriptJson
  ): string | PromiseLike<string> {
    return new WalletAPI(connectedWallet.key).invoke(
      connectedWallet.account.address,
      invokeScript
    )
  }
}