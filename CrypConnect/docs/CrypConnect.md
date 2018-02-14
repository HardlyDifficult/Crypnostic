<a name='contents'></a>
# Contents [#](#contents 'Go To Here')

- [BinanceExchange](#T-CryptoExchanges-BinanceExchange 'CryptoExchanges.BinanceExchange')
  - [#ctor()](#M-CryptoExchanges-BinanceExchange-#ctor-CryptoExchanges-ExchangeMonitor- 'CryptoExchanges.BinanceExchange.#ctor(CryptoExchanges.ExchangeMonitor)')
- [Coin](#T-CryptoExchanges-Coin 'CryptoExchanges.Coin')
  - [fullNameLower](#F-CryptoExchanges-Coin-fullNameLower 'CryptoExchanges.Coin.fullNameLower')
  - [bitcoin](#P-CryptoExchanges-Coin-bitcoin 'CryptoExchanges.Coin.bitcoin')
  - [ethereum](#P-CryptoExchanges-Coin-ethereum 'CryptoExchanges.Coin.ethereum')
  - [usd](#P-CryptoExchanges-Coin-usd 'CryptoExchanges.Coin.usd')
  - [Best(sellVsBuy,baseCoinFullName,exchangeName)](#M-CryptoExchanges-Coin-Best-CryptoExchanges-Coin,System-Boolean,System-Nullable{CryptoExchanges-ExchangeName}- 'CryptoExchanges.Coin.Best(CryptoExchanges.Coin,System.Boolean,System.Nullable{CryptoExchanges.ExchangeName})')
- [CoinMarketCapAPI](#T-CryptoExchanges-CoinMarketCap-CoinMarketCapAPI 'CryptoExchanges.CoinMarketCap.CoinMarketCapAPI')
- [CryptopiaExchange](#T-CryptoExchanges-CryptopiaExchange 'CryptoExchanges.CryptopiaExchange')
  - [#ctor(exchangeMonitor)](#M-CryptoExchanges-CryptopiaExchange-#ctor-CryptoExchanges-ExchangeMonitor- 'CryptoExchanges.CryptopiaExchange.#ctor(CryptoExchanges.ExchangeMonitor)')
- [EtherDeltaExchange](#T-CryptoExchanges-EtherDeltaExchange 'CryptoExchanges.EtherDeltaExchange')
  - [#ctor(exchangeMonitor)](#M-CryptoExchanges-EtherDeltaExchange-#ctor-CryptoExchanges-ExchangeMonitor- 'CryptoExchanges.EtherDeltaExchange.#ctor(CryptoExchanges.ExchangeMonitor)')
- [Exchange](#T-CryptoExchanges-Exchange 'CryptoExchanges.Exchange')
  - [supportsOverlappingBooks](#P-CryptoExchanges-Exchange-supportsOverlappingBooks 'CryptoExchanges.Exchange.supportsOverlappingBooks')
  - [AddTicker()](#M-CryptoExchanges-Exchange-AddTicker-System-String,CryptoExchanges-Coin,System-Boolean- 'CryptoExchanges.Exchange.AddTicker(System.String,CryptoExchanges.Coin,System.Boolean)')
  - [GetAllTradingPairs()](#M-CryptoExchanges-Exchange-GetAllTradingPairs 'CryptoExchanges.Exchange.GetAllTradingPairs')
  - [LoadTickerNames()](#M-CryptoExchanges-Exchange-LoadTickerNames 'CryptoExchanges.Exchange.LoadTickerNames')
- [ExchangeMonitor](#T-CryptoExchanges-ExchangeMonitor 'CryptoExchanges.ExchangeMonitor')
  - [aliasLowerToCoin](#F-CryptoExchanges-ExchangeMonitor-aliasLowerToCoin 'CryptoExchanges.ExchangeMonitor.aliasLowerToCoin')
  - [blacklistedFullNameLowerList](#F-CryptoExchanges-ExchangeMonitor-blacklistedFullNameLowerList 'CryptoExchanges.ExchangeMonitor.blacklistedFullNameLowerList')
  - [exchangeList](#F-CryptoExchanges-ExchangeMonitor-exchangeList 'CryptoExchanges.ExchangeMonitor.exchangeList')
  - [fullNameLowerToCoin](#F-CryptoExchanges-ExchangeMonitor-fullNameLowerToCoin 'CryptoExchanges.ExchangeMonitor.fullNameLowerToCoin')
- [ExchangeMonitorConfig](#T-CryptoExchanges-ExchangeMonitorConfig 'CryptoExchanges.ExchangeMonitorConfig')
  - [#ctor(supportedExchangeList)](#M-CryptoExchanges-ExchangeMonitorConfig-#ctor-CryptoExchanges-ExchangeName[]- 'CryptoExchanges.ExchangeMonitorConfig.#ctor(CryptoExchanges.ExchangeName[])')
  - [AddCoinMap(coinFullNameMapList)](#M-CryptoExchanges-ExchangeMonitorConfig-AddCoinMap-System-String[][]- 'CryptoExchanges.ExchangeMonitorConfig.AddCoinMap(System.String[][])')
  - [BlacklistCoins()](#M-CryptoExchanges-ExchangeMonitorConfig-BlacklistCoins-System-String[]- 'CryptoExchanges.ExchangeMonitorConfig.BlacklistCoins(System.String[])')
- [GDaxExchange](#T-CryptoExchanges-Exchanges-GDax-GDaxExchange 'CryptoExchanges.Exchanges.GDax.GDaxExchange')
- [KucoinExchange](#T-CryptoExchanges-Exchanges-KucoinExchange 'CryptoExchanges.Exchanges.KucoinExchange')
  - [#ctor()](#M-CryptoExchanges-Exchanges-KucoinExchange-#ctor-CryptoExchanges-ExchangeMonitor- 'CryptoExchanges.Exchanges.KucoinExchange.#ctor(CryptoExchanges.ExchangeMonitor)')
- [TradingPair](#T-CryptoExchanges-TradingPair 'CryptoExchanges.TradingPair')
  - [askPrice](#P-CryptoExchanges-TradingPair-askPrice 'CryptoExchanges.TradingPair.askPrice')
  - [bidPrice](#P-CryptoExchanges-TradingPair-bidPrice 'CryptoExchanges.TradingPair.bidPrice')

<a name='assembly'></a>
# CrypConnect [#](#assembly 'Go To Here') [=](#contents 'Back To Contents')

<a name='T-CryptoExchanges-BinanceExchange'></a>
## BinanceExchange [#](#T-CryptoExchanges-BinanceExchange 'Go To Here') [=](#contents 'Back To Contents')

##### Namespace

CryptoExchanges

##### Summary



##### Remarks

https://github.com/binance-exchange/binance-official-api-docs/blob/master/rest-api.md HTTP 429 return code is used when breaking a request rate limit.

<a name='M-CryptoExchanges-BinanceExchange-#ctor-CryptoExchanges-ExchangeMonitor-'></a>
### #ctor() `constructor` [#](#M-CryptoExchanges-BinanceExchange-#ctor-CryptoExchanges-ExchangeMonitor- 'Go To Here') [=](#contents 'Back To Contents')

##### Summary

Throttle: 1200 requests per minute is the stated max. Targeting half that to avoid issues.

##### Parameters

This constructor has no parameters.

<a name='T-CryptoExchanges-Coin'></a>
## Coin [#](#T-CryptoExchanges-Coin 'Go To Here') [=](#contents 'Back To Contents')

##### Namespace

CryptoExchanges

<a name='F-CryptoExchanges-Coin-fullNameLower'></a>
### fullNameLower `constants` [#](#F-CryptoExchanges-Coin-fullNameLower 'Go To Here') [=](#contents 'Back To Contents')

##### Summary

Cached in this form for performance.

<a name='P-CryptoExchanges-Coin-bitcoin'></a>
### bitcoin `property` [#](#P-CryptoExchanges-Coin-bitcoin 'Go To Here') [=](#contents 'Back To Contents')

##### Summary

Reference to Bitcoin, for convenience.

<a name='P-CryptoExchanges-Coin-ethereum'></a>
### ethereum `property` [#](#P-CryptoExchanges-Coin-ethereum 'Go To Here') [=](#contents 'Back To Contents')

##### Summary

Reference to Ethereum, for convenience.

<a name='P-CryptoExchanges-Coin-usd'></a>
### usd `property` [#](#P-CryptoExchanges-Coin-usd 'Go To Here') [=](#contents 'Back To Contents')

##### Summary

Reference to United States Dollar, for convenience.

<a name='M-CryptoExchanges-Coin-Best-CryptoExchanges-Coin,System-Boolean,System-Nullable{CryptoExchanges-ExchangeName}-'></a>
### Best(sellVsBuy,baseCoinFullName,exchangeName) `method` [#](#M-CryptoExchanges-Coin-Best-CryptoExchanges-Coin,System-Boolean,System-Nullable{CryptoExchanges-ExchangeName}- 'Go To Here') [=](#contents 'Back To Contents')

##### Summary



##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| sellVsBuy | [CryptoExchanges.Coin](#T-CryptoExchanges-Coin 'CryptoExchanges.Coin') | True: Sell this coin for baseCoin. False: Buy this coin with baseCoin. |
| baseCoinFullName | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') |  |
| exchangeName | [System.Nullable{CryptoExchanges.ExchangeName}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Nullable 'System.Nullable{CryptoExchanges.ExchangeName}') | If specified, only consider trades on this exchange |

<a name='T-CryptoExchanges-CoinMarketCap-CoinMarketCapAPI'></a>
## CoinMarketCapAPI [#](#T-CryptoExchanges-CoinMarketCap-CoinMarketCapAPI 'Go To Here') [=](#contents 'Back To Contents')

##### Namespace

CryptoExchanges.CoinMarketCap

##### Summary

https://coinmarketcap.com/api/

<a name='T-CryptoExchanges-CryptopiaExchange'></a>
## CryptopiaExchange [#](#T-CryptoExchanges-CryptopiaExchange 'Go To Here') [=](#contents 'Back To Contents')

##### Namespace

CryptoExchanges

##### Remarks

https://www.cryptopia.co.nz/Forum/Thread/255

<a name='M-CryptoExchanges-CryptopiaExchange-#ctor-CryptoExchanges-ExchangeMonitor-'></a>
### #ctor(exchangeMonitor) `constructor` [#](#M-CryptoExchanges-CryptopiaExchange-#ctor-CryptoExchanges-ExchangeMonitor- 'Go To Here') [=](#contents 'Back To Contents')

##### Summary

1,000 requests/minute 1,000,000 requests/day (smaller) (using half daily limit)

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| exchangeMonitor | [CryptoExchanges.ExchangeMonitor](#T-CryptoExchanges-ExchangeMonitor 'CryptoExchanges.ExchangeMonitor') |  |

<a name='T-CryptoExchanges-EtherDeltaExchange'></a>
## EtherDeltaExchange [#](#T-CryptoExchanges-EtherDeltaExchange 'Go To Here') [=](#contents 'Back To Contents')

##### Namespace

CryptoExchanges

##### Summary



##### Remarks

https://github.com/etherdelta/etherdelta.github.io/blob/master/docs/API_OLD.md

<a name='M-CryptoExchanges-EtherDeltaExchange-#ctor-CryptoExchanges-ExchangeMonitor-'></a>
### #ctor(exchangeMonitor) `constructor` [#](#M-CryptoExchanges-EtherDeltaExchange-#ctor-CryptoExchanges-ExchangeMonitor- 'Go To Here') [=](#contents 'Back To Contents')

##### Summary

No stated throttle limit, going with the same as Crytpopia

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| exchangeMonitor | [CryptoExchanges.ExchangeMonitor](#T-CryptoExchanges-ExchangeMonitor 'CryptoExchanges.ExchangeMonitor') |  |

<a name='T-CryptoExchanges-Exchange'></a>
## Exchange [#](#T-CryptoExchanges-Exchange 'Go To Here') [=](#contents 'Back To Contents')

##### Namespace

CryptoExchanges

<a name='P-CryptoExchanges-Exchange-supportsOverlappingBooks'></a>
### supportsOverlappingBooks `property` [#](#P-CryptoExchanges-Exchange-supportsOverlappingBooks 'Go To Here') [=](#contents 'Back To Contents')

##### Summary

True if the exchange allows a negative spread.

<a name='M-CryptoExchanges-Exchange-AddTicker-System-String,CryptoExchanges-Coin,System-Boolean-'></a>
### AddTicker() `method` [#](#M-CryptoExchanges-Exchange-AddTicker-System-String,CryptoExchanges-Coin,System-Boolean- 'Go To Here') [=](#contents 'Back To Contents')

##### Parameters

This method has no parameters.

<a name='M-CryptoExchanges-Exchange-GetAllTradingPairs'></a>
### GetAllTradingPairs() `method` [#](#M-CryptoExchanges-Exchange-GetAllTradingPairs 'Go To Here') [=](#contents 'Back To Contents')

##### Summary

This is called during init, after LoadTickerNames and then refreshed periodically. Call AddTradingPair for each pair supported.

##### Parameters

This method has no parameters.

<a name='M-CryptoExchanges-Exchange-LoadTickerNames'></a>
### LoadTickerNames() `method` [#](#M-CryptoExchanges-Exchange-LoadTickerNames 'Go To Here') [=](#contents 'Back To Contents')

##### Summary

This is called during init and then refreshed periodically. You can also call this anytime for a manual refresh (subject to throttling). It should call AddTicker for each coin. This may call UpdateTradingPair with status (unless that is done during GetAllTradingPairs)

##### Parameters

This method has no parameters.

<a name='T-CryptoExchanges-ExchangeMonitor'></a>
## ExchangeMonitor [#](#T-CryptoExchanges-ExchangeMonitor 'Go To Here') [=](#contents 'Back To Contents')

##### Namespace

CryptoExchanges

<a name='F-CryptoExchanges-ExchangeMonitor-aliasLowerToCoin'></a>
### aliasLowerToCoin `constants` [#](#F-CryptoExchanges-ExchangeMonitor-aliasLowerToCoin 'Go To Here') [=](#contents 'Back To Contents')

##### Summary

Populated from config on construction.

<a name='F-CryptoExchanges-ExchangeMonitor-blacklistedFullNameLowerList'></a>
### blacklistedFullNameLowerList `constants` [#](#F-CryptoExchanges-ExchangeMonitor-blacklistedFullNameLowerList 'Go To Here') [=](#contents 'Back To Contents')

##### Summary

Populated from config on construction. After consider aliases.

<a name='F-CryptoExchanges-ExchangeMonitor-exchangeList'></a>
### exchangeList `constants` [#](#F-CryptoExchanges-ExchangeMonitor-exchangeList 'Go To Here') [=](#contents 'Back To Contents')

##### Summary

In priority order, so first exchange is my most preferred trading platform.

<a name='F-CryptoExchanges-ExchangeMonitor-fullNameLowerToCoin'></a>
### fullNameLowerToCoin `constants` [#](#F-CryptoExchanges-ExchangeMonitor-fullNameLowerToCoin 'Go To Here') [=](#contents 'Back To Contents')

##### Summary

After considering aliases and blacklist.

<a name='T-CryptoExchanges-ExchangeMonitorConfig'></a>
## ExchangeMonitorConfig [#](#T-CryptoExchanges-ExchangeMonitorConfig 'Go To Here') [=](#contents 'Back To Contents')

##### Namespace

CryptoExchanges

<a name='M-CryptoExchanges-ExchangeMonitorConfig-#ctor-CryptoExchanges-ExchangeName[]-'></a>
### #ctor(supportedExchangeList) `constructor` [#](#M-CryptoExchanges-ExchangeMonitorConfig-#ctor-CryptoExchanges-ExchangeName[]- 'Go To Here') [=](#contents 'Back To Contents')

##### Summary



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| supportedExchangeList | [CryptoExchanges.ExchangeName[]](#T-CryptoExchanges-ExchangeName[] 'CryptoExchanges.ExchangeName[]') | List in priority order. i.e. the first exchange will be considered before others. Leave blank to support every exchange. |

<a name='M-CryptoExchanges-ExchangeMonitorConfig-AddCoinMap-System-String[][]-'></a>
### AddCoinMap(coinFullNameMapList) `method` [#](#M-CryptoExchanges-ExchangeMonitorConfig-AddCoinMap-System-String[][]- 'Go To Here') [=](#contents 'Back To Contents')

##### Summary

Defines aliases for a coin. e.g. TetherUS maps to Tether so it matches with other exchanges.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| coinFullNameMapList | [System.String[][]](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String[][] 'System.String[][]') | The first string is the coin's full name we will use going forward. The other strings are aliases which will map to the first. |

<a name='M-CryptoExchanges-ExchangeMonitorConfig-BlacklistCoins-System-String[]-'></a>
### BlacklistCoins() `method` [#](#M-CryptoExchanges-ExchangeMonitorConfig-BlacklistCoins-System-String[]- 'Go To Here') [=](#contents 'Back To Contents')

##### Summary

Completely removes these coins from consideration. To restore these entries, stop and restart the program.

##### Parameters

This method has no parameters.

<a name='T-CryptoExchanges-Exchanges-GDax-GDaxExchange'></a>
## GDaxExchange [#](#T-CryptoExchanges-Exchanges-GDax-GDaxExchange 'Go To Here') [=](#contents 'Back To Contents')

##### Namespace

CryptoExchanges.Exchanges.GDax

##### Remarks

https://docs.gdax.com/#introduction

<a name='T-CryptoExchanges-Exchanges-KucoinExchange'></a>
## KucoinExchange [#](#T-CryptoExchanges-Exchanges-KucoinExchange 'Go To Here') [=](#contents 'Back To Contents')

##### Namespace

CryptoExchanges.Exchanges

##### Remarks

https://kucoinapidocs.docs.apiary.io/#

<a name='M-CryptoExchanges-Exchanges-KucoinExchange-#ctor-CryptoExchanges-ExchangeMonitor-'></a>
### #ctor() `constructor` [#](#M-CryptoExchanges-Exchanges-KucoinExchange-#ctor-CryptoExchanges-ExchangeMonitor- 'Go To Here') [=](#contents 'Back To Contents')

##### Summary

No stated throttle limit, going with the same as Crytpopia

##### Parameters

This constructor has no parameters.

<a name='T-CryptoExchanges-TradingPair'></a>
## TradingPair [#](#T-CryptoExchanges-TradingPair 'Go To Here') [=](#contents 'Back To Contents')

##### Namespace

CryptoExchanges

<a name='P-CryptoExchanges-TradingPair-askPrice'></a>
### askPrice `property` [#](#P-CryptoExchanges-TradingPair-askPrice 'Go To Here') [=](#contents 'Back To Contents')

##### Summary

The cost to purchase.

<a name='P-CryptoExchanges-TradingPair-bidPrice'></a>
### bidPrice `property` [#](#P-CryptoExchanges-TradingPair-bidPrice 'Go To Here') [=](#contents 'Back To Contents')

##### Summary

The price you would get by selling.
