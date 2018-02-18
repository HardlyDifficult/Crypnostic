using System;
using System.Collections.Generic;

namespace Crypnostic
{
  public struct Order
  {
    public readonly decimal price;
    public readonly decimal volume;

    public Order(
      decimal price,
      decimal volume)
    {
      this.price = price;
      this.volume = volume;
    }
  }
}
