using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypnostic.Internal
{

  public class EtherDeltaMainConfigJson
  {
    public string contractEtherDelta { get; set; }
    public string contractToken { get; set; }
    public string contractReserveToken { get; set; }
    public Contractetherdeltaaddr[] contractEtherDeltaAddrs { get; set; }
    public bool ethTestnet { get; set; }
    public int ethChainId { get; set; }
    public string ethProvider { get; set; }
    public long ethGasPrice { get; set; }
    public string ethAddr { get; set; }
    public string ethAddrPrivateKey { get; set; }
    public int gasApprove { get; set; }
    public int gasDeposit { get; set; }
    public int gasWithdraw { get; set; }
    public int gasTrade { get; set; }
    public int gasOrder { get; set; }
    public decimal minOrderSize { get; set; }
    public string socketServer { get; set; }
    public string userCookie { get; set; }
    public string eventsCacheCookie { get; set; }
    public string ordersCacheCookie { get; set; }
    public string etherscanAPIKey { get; set; }
    public string ledgerPath { get; set; }
    public Token[] bases { get; set; }
    public Token[] tokens { get; set; }
    //public Defaultpair defaultPair { get; set; }
  }

  //public class Defaultpair
  //{
  //  public string token { get; set; }
  //  public string _base { get; set; }
  //}

  public class Contractetherdeltaaddr
  {
    public string addr { get; set; }
    public string info { get; set; }
  }
  
  public class Token
  {
    public string addr { get; set; }
    public string name { get; set; }
    public int decimals { get; set; }
  }

}
