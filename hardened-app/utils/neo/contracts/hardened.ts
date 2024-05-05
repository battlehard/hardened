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
  PENDING_INFUSION = 'pendingInfusion',
}

export enum ArgumentType {
  STRING = 'String',
  INTEGER = 'Integer',
  HASH160 = 'Hash160',
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

export interface IPreInfusionObject {
  clientPubKey: string
  payTokenHash: string
  payTokenAmount: number
  bhNftId: string | null
  slotNftHashes: string[]
  slotNftIds: string[]
}

export interface IInfusionMintObject {
  clientPubKey: string
  contractPubKey: string
  contractHash: string
  payTokenHash: string
  payTokenAmount: number
  base58Properties: string
}

export interface IInfusionUpdateObject {
  clientPubKey: string
  contractPubKey: string
  userWalletHash: string
  payTokenHash: string
  payTokenAmount: number
  contractHash: string
  base58Properties: string
}

export interface IPendingInfusionProperties {
  clientPubKey: string
  contractPubKey: string
  userWalletAddress: string
  bhNftId: string
  slotNftHashes: string[]
  slotNftIds: string[]
}

export interface IPendingInfusionListPagination {
  totalPages: number
  totalPending: number
  pendingList: IPendingInfusionProperties[]
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
      ],
    }

    return this.invoke(connectedWallet, invokeScript)
  }

  PreInfusion = async (
    connectedWallet: IConnectedWallet,
    preInfusionObject: IPreInfusionObject
  ): Promise<string> => {
    preInfusionObject.slotNftHashes = preInfusionObject.slotNftHashes.filter(
      (hash) => hash !== ''
    )
    preInfusionObject.slotNftIds = preInfusionObject.slotNftIds.filter(
      (id) => id !== ''
    )
    const invokeScript: IInvokeScriptJson = {
      operation: 'preInfusion',
      scriptHash: this.contractHash,
      args: [
        {
          type: 'String',
          value: preInfusionObject.clientPubKey,
        },
        {
          type: 'Hash160',
          value: preInfusionObject.payTokenHash,
        },
        {
          type: 'Integer',
          value: preInfusionObject.payTokenAmount,
        },
        {
          type: 'String',
          value:
            preInfusionObject.bhNftId == null ? '' : preInfusionObject.bhNftId,
        },
        {
          type: 'Array',
          value: this.getArray(
            ArgumentType.HASH160,
            preInfusionObject.slotNftHashes
          ),
        },
        {
          type: 'Array',
          value: this.getArray(
            ArgumentType.STRING,
            preInfusionObject.slotNftIds
          ),
        },
      ],
    }

    // Custom permission
    invokeScript.signers = [
      {
        account: NeonWallet.getScriptHashFromAddress(
          connectedWallet.account.address
        ),
        scopes: tx.WitnessScope.CustomContracts,
        allowedContracts: [
          this.contractHash,
          GAS_SCRIPT_HASH,
          preInfusionObject.payTokenHash,
          ...preInfusionObject.slotNftHashes,
        ],
      },
    ]

    return this.invoke(connectedWallet, invokeScript)
  }

  CancelInfusion = async (
    connectedWallet: IConnectedWallet,
    clientPubKey: string,
    contractPubKey: string
  ): Promise<string> => {
    const invokeScript: IInvokeScriptJson = {
      operation: 'cancelInfusion',
      scriptHash: this.contractHash,
      args: [
        {
          type: 'String',
          value: clientPubKey,
        },
        {
          type: 'String',
          value: contractPubKey,
        },
      ],
    }
    return this.invoke(connectedWallet, invokeScript)
  }

  Unfuse = async (
    connectedWallet: IConnectedWallet,
    bhNftId: string
  ): Promise<string> => {
    const invokeScript: IInvokeScriptJson = {
      operation: 'unfuse',
      scriptHash: this.contractHash,
      args: [
        {
          type: 'String',
          value: bhNftId,
        },
      ],
    }
    return this.invoke(connectedWallet, invokeScript)
  }

  BurnInfusion = async (
    connectedWallet: IConnectedWallet,
    bhNftId: string
  ): Promise<string> => {
    const invokeScript: IInvokeScriptJson = {
      operation: 'burnInfusion',
      scriptHash: this.contractHash,
      args: [
        {
          type: 'String',
          value: bhNftId,
        },
      ],
    }
    return this.invoke(connectedWallet, invokeScript)
  }

  PendingInfusion = async (
    walletHashesList: string[],
    pageNumber: number = 1,
    pageSize: number = MAX_PAGE_LIMIT
  ): Promise<IPendingInfusionListPagination> => {
    const args: any = [
      {
        type: 'Integer',
        value: pageNumber,
      },
      {
        type: 'Integer',
        value: pageSize,
      },
      {
        type: 'Array',
        value: this.getArray(ArgumentType.HASH160, walletHashesList),
      },
    ]

    return this.Read(ReadMethod.PENDING_INFUSION, args)
  }

  InfusionMint = async (
    connectedWallet: IConnectedWallet,
    infusionMintObject: IInfusionMintObject
  ): Promise<string> => {
    const invokeScript: IInvokeScriptJson = {
      operation: 'infusionMint',
      scriptHash: this.contractHash,
      args: [
        {
          type: 'String',
          value: infusionMintObject.clientPubKey,
        },
        {
          type: 'String',
          value: infusionMintObject.contractPubKey,
        },
        {
          type: 'Hash160',
          value: infusionMintObject.contractHash,
        },
        {
          type: 'Hash160',
          value: infusionMintObject.payTokenHash,
        },
        {
          type: 'Integer',
          value: infusionMintObject.payTokenAmount,
        },
        {
          type: 'String',
          value: infusionMintObject.base58Properties,
        },
      ],
    }
    return this.invoke(connectedWallet, invokeScript)
  }

  InfusionUpdate = async (
    connectedWallet: IConnectedWallet,
    infusionUpdateObject: IInfusionUpdateObject
  ): Promise<string> => {
    const invokeScript: IInvokeScriptJson = {
      operation: 'infusionUpdate',
      scriptHash: this.contractHash,
      args: [
        {
          type: 'String',
          value: infusionUpdateObject.clientPubKey,
        },
        {
          type: 'String',
          value: infusionUpdateObject.contractPubKey,
        },
        {
          type: 'Hash160',
          value: infusionUpdateObject.userWalletHash,
        },
        {
          type: 'Hash160',
          value: infusionUpdateObject.payTokenHash,
        },
        {
          type: 'Integer',
          value: infusionUpdateObject.payTokenAmount,
        },
        {
          type: 'Hash160',
          value: infusionUpdateObject.contractHash,
        },
        {
          type: 'String',
          value: infusionUpdateObject.base58Properties,
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
    if (process.env.NEXT_PUBLIC_IS_DEBUG) console.log(res)
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

  private getArray(type: ArgumentType, list: any[]) {
    return list.map((s) => {
      return { type, value: s }
    })
  }
}
