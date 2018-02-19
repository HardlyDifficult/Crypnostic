using System;

namespace Crypnostic.Exchanges.AEX
{
  public class AexCoinJson
  {
    public AexTickerJson ticker { get; set; }
  }

  public class AexTickerJson
  {
    public decimal high { get; set; }
    public decimal low { get; set; }
    public decimal last { get; set; }
    public decimal vol { get; set; }
    public decimal buy { get; set; }
    public decimal sell { get; set; }
  }


  public class AexDepthJson
  {
    public decimal[][] bids { get; set; }
    public decimal[][] asks { get; set; }
  }

}
