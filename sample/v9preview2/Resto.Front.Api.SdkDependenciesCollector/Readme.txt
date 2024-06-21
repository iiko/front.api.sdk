Resto.Front.Api.SdkDependenciesCollector — вспомогательный проект для сборки зависимостей SDK.
По сути это фиктивный пустой проект, в нём нет кода, он используется лишь для выполнения post-build event.

Публикуемые в составе SDK библиотеки организованы следующим образом (все пути указаны относительно dev\iikoFront.Net\Sdk\Binaries):
* Vx — Resto.Front.Api.Vx и Resto.Front.Api.Vx.PaymentTypes;

Публикуемые в составе SDK плагины должны ссылаться на необходимые им библиотеки из этих папок и иметь на уровне iikoFront.Net.Plugins.sln зависимость от Resto.Front.Api.SdkDependenciesCollector.