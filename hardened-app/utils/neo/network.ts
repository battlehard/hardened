import { rpc, sc, u, wallet } from '@cityofzion/neon-core'
import { MAINNET, MAINNET_CONFIG, TESTNET, TESTNET_CONFIG } from './constant'
import { InvokeResult, Query } from '@cityofzion/neon-core/lib/rpc'
import { MAX_RPC_RESULT } from './constant'
import { INetworkType } from './interfaces'

export class Network {
  static getRPCClient = (networkType: INetworkType) => {
    let config
    switch (networkType) {
      case TESTNET:
        config = TESTNET_CONFIG
        break
      case MAINNET:
        config = MAINNET_CONFIG
        break
    }
    return new rpc.RPCClient(config.url)
  }

  static read = async (
    network: INetworkType,
    scripts: sc.ContractCallJson[]
  ): Promise<InvokeResult> => {
    const rpcClient = Network.getRPCClient(network)
    const sb = new sc.ScriptBuilder()
    scripts.map((script) => {
      let params: unknown[] = []
      if (script.args) {
        params = script.args.map((arg) => Network.convertContractCallParam(arg))
      }
      sb.emitAppCall(script.scriptHash, script.operation, params)
    })
    return await rpcClient.invokeScript(u.HexString.fromHex(sb.build()))
  }

  private static convertContractCallParam = (param: any) => {
    switch (param.type) {
      case 'Address':
        return sc.ContractParam.hash160(
          wallet.getScriptHashFromAddress(param.value)
        )
      case 'Hash160':
        return sc.ContractParam.hash160(param.value)
      case 'String':
        return sc.ContractParam.string(param.value)
      case 'Integer':
        return sc.ContractParam.integer(param.value)
      case 'Array':
        return sc.ContractParam.array(
          ...param.value.map((i: any) => Network.convertContractCallParam(i))
        )
      case 'ByteArray':
        return sc.ContractParam.byteArray(
          u.hex2base64(u.str2hexstring(param.value))
        )
      default:
        throw new Error('No support param')
    }
  }
}
