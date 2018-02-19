using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Crypnostic.Data
{
  /// <summary>
  /// Data about a coin from CoinMarketCap.
  /// 
  /// Note that this is not available at all for some coins, 
  /// and for others the data will be spotty.
  /// </summary>
  public class MarketCap
  {
    public readonly string ticker;
    public readonly int rank;
    public readonly decimal? priceBtc;
    public readonly decimal? priceUsd;
    public readonly decimal? volume24HrUsd;
    public readonly decimal? marketCapUsd;
    public readonly decimal? availableSupply;
    public readonly decimal? totalSupply;
    public readonly decimal? maxSupply;
    public readonly decimal? percentChange1HrUsd;
    public readonly decimal? percentChange24HrUsd;
    public readonly decimal? percentChange7dUsd;
    public readonly DateTime lastUpdated;

    public MarketCap(
      string ticker,
      int rank,
      decimal? priceBtc,
      decimal? priceUsd,
      decimal? volume24HrUsd,
      decimal? marketCapUsd,
      decimal? availableSupply,
      decimal? totalSupply,
      decimal? maxSupply,
      decimal? percentChange1HrUsd,
      decimal? percentChange24HrUsd,
      decimal? percentChange7dUsd,
      DateTime lastUpdated)
    {
      Debug.Assert(string.IsNullOrWhiteSpace(ticker) == false);
      Debug.Assert(rank > 0);
      Debug.Assert(rank < 1_000_000); // someday that won't be true ;)

      this.ticker = ticker;
      this.rank = rank;
      this.priceBtc = priceBtc;
      this.priceUsd = priceUsd;
      this.volume24HrUsd = volume24HrUsd;
      this.marketCapUsd = marketCapUsd;
      this.availableSupply = availableSupply;
      this.totalSupply = totalSupply;
      this.maxSupply = maxSupply;
      this.percentChange1HrUsd = percentChange1HrUsd;
      this.percentChange24HrUsd = percentChange24HrUsd;
      this.percentChange7dUsd = percentChange7dUsd;
      this.lastUpdated = lastUpdated;
    }
  }
}
