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

- [Code reference API V5](https://iiko.github.io/front.api.sdk/v5/) (obsolete).
- [Code reference API V6](https://iiko.github.io/front.api.sdk/v6/) (current).
- [Code reference API V7](https://iiko.github.io/front.api.sdk/v7/) (preview).
- [Code reference Web API](https://iiko.github.io/front.api.sdk/WebApiServer/).
- [Help topics](https://iiko.github.io/front.api.doc/) (available only in russian at the moment).
- [Official site of iiko APIs](http://help.iiko.ru/articles/api-documentations/getting-started).
- [Sample](https://github.com/iiko/front.api.sdk/tree/master/sample).

