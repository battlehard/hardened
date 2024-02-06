using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using static Hardened.Helpers;

namespace Hardened
{
  public partial class Hardened
  {
    private static HardenedState ValidateOwnership(string bhNftId)
    {
      HardenedState nftState = GetState(bhNftId);
      Assert(Runtime.CheckWitness(nftState.Owner), E_04);
      return nftState;
    }
    private static void UpdateState(string bhNftId, HardenedState nftState)
    {
      StorageMap tokenMap = new(Storage.CurrentContext, Prefix_Token);
      tokenMap.Put(bhNftId, StdLib.Serialize(nftState));
    }
  }
}
