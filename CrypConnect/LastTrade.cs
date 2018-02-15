using System;

namespace CrypConnect
{
  public struct LastTrade
  {
    public readonly decimal price;

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
