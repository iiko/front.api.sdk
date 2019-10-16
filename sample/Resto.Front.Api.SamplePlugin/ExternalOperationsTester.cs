using System;
using System.Reactive.Disposables;
using System.Reflection;
using System.Runtime.Serialization;
using Resto.Front.Api.Extensions;

namespace Resto.Front.Api.SamplePlugin
{
    internal static class ExternalOperationsTester
    {
        public static void TestCalculator()
        {
            // first plugin provides some service
            using (new CalculatorServer())
            {
                // second plugin uses operations provided by the first plugin
                var client = new CalculatorClient();
                client.TestAddition(4, 7);
                client.TestAddition(-42, 42);
                client.TestSubtraction(1024, 256);
                client.TestDivision(100, 7);
            }
        }

        private sealed class CalculatorServer : IDisposable
        {
            private readonly CompositeDisposable subscriptions;

            public CalculatorServer()
            {
                subscriptions = new CompositeDisposable
                {
                    PluginContext.Operations.RegisterExternalOperation<Tuple<int, int>, int>(nameof(CalculatorServer), nameof(Add), x => Add(x.Item1, x.Item2)),
                    PluginContext.Operations.RegisterExternalOperation<Tuple<int, int>, int>(nameof(CalculatorServer), nameof(Subtract), x => Subtract(x.Item1, x.Item2)),
                    PluginContext.Operations.RegisterExternalOperation<DivisionRequest, DivisionResponse>(nameof(CalculatorServer), nameof(Divide), Divide, CustomBinder.Instance)
                };
            }

            public void Dispose()
            {
                subscriptions.Dispose();
            }

            private int Add(int a, int b)
            {
                PluginContext.Log.Info($"Calculating {a} + {b}...");
                return a + b;
            }

            private int Subtract(int minuend, int subtrahend)
            {
                PluginContext.Log.Info($"Calculating {minuend} - {subtrahend}...");
                return minuend - subtrahend;
            }

            private DivisionResponse Divide(DivisionRequest request)
            {
                return new DivisionResponse(request.Dividend / request.Divisor, request.Dividend % request.Divisor);
            }
        }

        private sealed class CalculatorClient
        {
            public void TestAddition(int a, int b)
            {
                var sum = PluginContext.Operations.CallExternalOperation<Tuple<int, int>, int>("CalculatorServer", "Add", Tuple.Create(a, b));
                PluginContext.Log.Info($"{a} + {b} = {sum}");
            }

            public void TestSubtraction(int minuend, int subtrahend)
            {
                var diff = PluginContext.Operations.CallExternalOperation<Tuple<int, int>, int>("CalculatorServer", "Subtract", Tuple.Create(minuend, subtrahend));
                PluginContext.Log.Info($"{minuend} - {subtrahend} = {diff}");
            }

            public void TestDivision(int dividend, int divisor)
            {
                var request = new DivisionRequest(dividend, divisor);
                var response = PluginContext.Operations.CallExternalOperation<DivisionRequest, DivisionResponse>("CalculatorServer", "Divide", request, CustomBinder.Instance);
                PluginContext.Log.Info($"{dividend} = {divisor} * {response.Quotient} + {response.Remainder}");
            }
        }

        [Serializable]
        private sealed class DivisionRequest
        {
            public DivisionRequest(int dividend, int divisor)
            {
                Dividend = dividend;
                Divisor = divisor;
            }

            public int Dividend { get; }
            public int Divisor { get; }
        }

        [Serializable]
        private sealed class DivisionResponse
        {
            public DivisionResponse(int quotient, int remainder)
            {
                Quotient = quotient;
                Remainder = remainder;
            }

            public int Quotient { get; }
            public int Remainder { get; }
        }

        private sealed class CustomBinder : SerializationBinder
        {
            public static readonly SerializationBinder Instance = new CustomBinder();

            private CustomBinder()
            { }

            public override Type BindToType(string assemblyName, string typeName)
            {
                return Assembly.Load(assemblyName).GetType(typeName);
            }
        }
    }
}
