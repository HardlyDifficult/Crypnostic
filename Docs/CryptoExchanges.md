## `Coin`

```csharp
public class CryptoExchanges.Coin

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | fullName |  | 
| `String` | fullNameLower |  | 


Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `IEnumerable<TradingPair>` | allTradingPairs |  | 


Events

| Type | Name | Summary | 
| --- | --- | --- | 
| `Action<Coin>` | onPriceUpdate |  | 
| `Action<Coin>` | onStatusUpdate |  | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | AddPair(`TradingPair` pair) |  | 
| `TradingPair` | AddPair(`Exchange` exchange, `Coin` baseCoin, `Decimal` askPrice, `Decimal` bidPrice) |  | 
| `TradingPair` | Best(`Coin` baseCoin, `Boolean` sellVsBuy, `Nullable<ExchangeName>` exchangeName = null) |  | 
| `Boolean` | Equals(`Object` obj) |  | 
| `Int32` | GetHashCode() |  | 
| `Boolean` | IsActiveOn(`ExchangeName` exchangeName) |  | 
| `String` | ToString() |  | 
| `void` | UpdatePairStatus(`Exchange` exchange, `Coin` baseCoin, `Boolean` isInactive) |  | 


Static Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Coin` | bitcoin |  | 
| `Coin` | ethereum |  | 
| `Coin` | usd |  | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Coin` | FromName(`String` fullName) |  | 
| `Coin` | FromTicker(`String` ticker, `ExchangeName` onExchange) |  | 


## `Exchange`

```csharp
public abstract class CryptoExchanges.Exchange

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `ExchangeMonitor` | exchangeMonitor |  | 
| `ExchangeName` | exchangeName |  | 
| `Throttle` | throttle |  | 
| `Dictionary<String, Coin>` | tickerLowerToCoin |  | 


Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Boolean` | supportsOverlappingBooks |  | 


Events

| Type | Name | Summary | 
| --- | --- | --- | 
| `Action<Exchange>` | onPriceUpdate |  | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | AddTicker(`String` ticker, `Coin` coin, `Boolean` isCoinActive) |  | 
| `void` | AddTradingPair(`String` baseCoinTicker, `String` quoteCoinTicker, `Decimal` askPrice, `Decimal` bidPrice, `Nullable<Boolean>` isInactive = null) |  | 
| `Task` | GetAllPairs() |  | 
| `Task` | GetAllTradingPairs() |  | 
| `Boolean` | IsCoinActive(`Coin` coin) |  | 
| `Task` | LoadTickerNames() |  | 


Static Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `Exchange` | LoadExchange(`ExchangeMonitor` exchangeMonitor, `ExchangeName` exchangeName) |  | 


## `ExchangeMonitor`

```csharp
public class CryptoExchanges.ExchangeMonitor

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `Dictionary<String, Coin>` | aliasLowerToCoin |  | 
| `HashSet<String>` | blacklistedFullNameLowerList |  | 
| `Dictionary<String, Coin>` | fullNameLowerToCoin |  | 
| `Random` | random |  | 


Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `IEnumerable<Coin>` | allCoins |  | 
| `Boolean` | shouldStop |  | 


Events

| Type | Name | Summary | 
| --- | --- | --- | 
| `Action<Coin>` | onNewCoin |  | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | AddAlias(`String` alias, `String` name) |  | 
| `Exchange` | FindExchange(`ExchangeName` onExchange) |  | 
| `void` | OnNewCoin(`Coin` coin) |  | 
| `void` | Stop() |  | 


Static Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `ExchangeMonitor` | instance |  | 


## `ExchangeMonitorConfig`

```csharp
public class CryptoExchanges.ExchangeMonitorConfig

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `HashSet<String>` | blacklistedCoins |  | 
| `Dictionary<String, String>` | coinAliasToName |  | 
| `ExchangeName[]` | supportedExchangeList |  | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `void` | AddCoinMap(`String[][]` coinFullNameMapList) |  | 
| `void` | BlacklistCoins(`String[]` coinFullNames) |  | 


## `ExchangeName`

```csharp
public enum CryptoExchanges.ExchangeName
    : Enum, IComparable, IFormattable, IConvertible

```

Enum

| Value | Name | Summary | 
| --- | --- | --- | 
| `0` | Binance |  | 
| `1` | Cryptopia |  | 
| `2` | EtherDelta |  | 
| `3` | Kucoin |  | 
| `4` | GDax |  | 


## `TradingPair`

```csharp
public class CryptoExchanges.TradingPair

```

Fields

| Type | Name | Summary | 
| --- | --- | --- | 
| `Coin` | baseCoin |  | 
| `Exchange` | exchange |  | 
| `Coin` | quoteCoin |  | 


Properties

| Type | Name | Summary | 
| --- | --- | --- | 
| `Decimal` | askPrice |  | 
| `Decimal` | bidPrice |  | 
| `Boolean` | isInactive |  | 
| `DateTime` | lastUpdated |  | 


Events

| Type | Name | Summary | 
| --- | --- | --- | 
| `Action` | onPriceUpdate |  | 
| `Action` | onStatusChange |  | 


Methods

| Type | Name | Summary | 
| --- | --- | --- | 
| `String` | ToString() |  | 
| `void` | Update(`Decimal` askPrice, `Decimal` bidPrice) |  | 


