using System;

namespace Crypnostic
{
  public struct LastTrade
  {
    public readonly decimal price;

    // TODO what do we do about the fact not all exchanges get this at the same time? e.g. idex
    public readonly decimal volume;

    public readonly DateTime dateCreated;

    public LastTrade(
      decimal price,
      decimal volume)
    {
      this.price = price;
      this.volume = volume;
      this.dateCreated = DateTime.Now;
    }
  }
}
