using System;
using System.Diagnostics;

namespace Crypnostic
{
  /// <summary>
  /// The last price and the time it was last refreshed.
  /// 
  /// Different exchanges will update this value at different rates.
  /// </summary>
  // TODO make class with auto-update option
  public struct LastTrade
  {
    /// <summary>
    /// Price of 0 means no recent trade
    /// (different than an unknown state)
    /// </summary>
    public readonly decimal price;

    public readonly DateTime dateCreated;

    public LastTrade(
      decimal price)
    {
      this.price = price;
      this.dateCreated = DateTime.Now;
    }
  }
}
