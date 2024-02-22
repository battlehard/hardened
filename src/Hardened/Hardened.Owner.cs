using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using static Hardened.Helpers;

namespace Hardened
{
  public partial class Hardened
  {
    private static bool IsOwner()
    {
      var contractOwner = GetContractOwner();
      var tx = (Transaction)Runtime.ScriptContainer;
      return contractOwner.Equals(tx.Sender) && Runtime.CheckWitness(contractOwner);
    }

    private static void CheckOwner()
    {
      Assert(IsOwner(), $"{CONTRACT_NAME}: No owner authorization");
    }

    public static void _deploy(object data, bool update)
    {
      if (update) return;
      var tx = (Transaction)Runtime.ScriptContainer;
      Storage.Put(Storage.CurrentContext, Prefix_Owner, tx.Sender);
    }

    public static void Update(ByteString nefFile, string manifest)
    {
      CheckOwner();
      ContractManagement.Update(nefFile, manifest, null);
    }
  }
}