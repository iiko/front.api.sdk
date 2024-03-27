# iikoFront API SDK #
This repository contains iikoFront Api SDK (iikoFront is POS software, part of the iikoRms product).

Functionality of the iikoFront application can be extended using plugins. Special programming interface (API) allows you to change application behaviour in certain cases, insert custom information into the cheques on printing, display order details on the second monitor (customer screen), integrate with external booking systems (such as web-site, digital menu, mobile waiter's station), collect statistics and build reports, connect to the external payment systems and many other.

# Getting started #
iikoFront Api is based on .Net Framework 4.7.2 and provides a set of interfaces and classes. Basically you should create a class library, add a reference to the _Resto.Front.Api.Vx.dll_ and implement `IFrontPlugin` interface: 

```C#
public sealed class MyPlugin : IFrontPlugin
{
    // add your plugin logic here
}
```

Most of Api features are available via static members of the `PluginContext` class. Just subscribe to the events, read or write entities, display messages, etc. Once you are ready, install your plugin into subdirectory of the _Plugins_ directory under iikoFront installation. From now on, iikoFront will load your library and create an instance of the plugin.

Note that debugging and running plugins requires a license, so you'll need to register as a plugin developer to obtain your id and get a dev license. This repository is about tech help only and doesn't touch legal questions. Please, contact us via official site for such purposes.

Links:

- [Code reference API V4](https://iiko.github.io/front.api.sdk/v4/) (not supported from 7.0).
- [Code reference API V5](https://iiko.github.io/front.api.sdk/v5/) (not supported from 7.9).
- [Code reference API V6](https://iiko.github.io/front.api.sdk/v6/) (not supported from 8.8).
- [Code reference API V7](https://iiko.github.io/front.api.sdk/v7/) (obsolete).
- [Code reference API V8](https://iiko.github.io/front.api.sdk/v8/) (current).
- [Code reference API V9](https://iiko.github.io/front.api.sdk/v9/) (preview).
- [Help topics](https://iiko.github.io/front.api.doc/) (available only in russian at the moment).
- [Official site of iiko APIs](https://ru.iiko.help/articles/#!api-documentations/getting-started).
- [Sample](https://github.com/iiko/front.api.sdk/tree/master/sample).

