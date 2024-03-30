using System.ComponentModel;

#pragma warning disable CS8618 // Suppress known warning
namespace Hardened
{
  public partial class Hardened
  {
    [DisplayName("PreInfusion")]
    public static event OnPreInfusionDelegate OnPreInfusion;
    public delegate void OnPreInfusionDelegate(string clientPubKey, string contractPubKey);
  }
}
