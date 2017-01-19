using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace StaticInterfaceTest {
    public interface IInterface {
        int Run();
    }

    public struct Explicit : IInterface {
        [Benchmark]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        int IInterface.Run() {
            var i = 0;
            for (; i < 100;) i++;
            return i;
        }
    }

    public struct Implicit : IInterface {
        [Benchmark]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public int Run() {
            var i = 0;
            for (; i < 100;) i++;
            return i;
        }
    }

    public class Tester {
        private readonly Func<int> _compiledExplicitTest;
        private readonly Func<int> _compiledImplicitTest;
        private readonly IInterface _dExplicit = default(Explicit);

        private readonly IInterface _dImplicit = default(Implicit);
        private readonly MethodInfo _explicitMethod;
        private readonly MethodInfo _implicitMethod;

        public Tester() {
            var methodInfo = typeof(Tester).GetMethod(nameof(GenericRunner),
                BindingFlags.Static | BindingFlags.NonPublic);

            _explicitMethod = methodInfo
                .MakeGenericMethod(typeof(Explicit));
            _compiledExplicitTest = (Func<int>) _explicitMethod
                .CreateDelegate(typeof(Func<int>));

            _implicitMethod = methodInfo
                .MakeGenericMethod(typeof(Implicit));
            _compiledImplicitTest = (Func<int>) _implicitMethod
                .CreateDelegate(typeof(Func<int>));
        }

        [Benchmark(Description = "Implicit Interface default() Call", Baseline = true)]
        public int ImplicitTest() => default(Implicit).Run();

        [Benchmark(Description = "Explicit Interface default() Call")]
        public int ExplicitTest() => ((IInterface) default(Explicit)).Run();

        [Benchmark(Description = "Implicit Interface Instance Call")]
        public int FieldImplicitTest() => _dImplicit.Run();

        [Benchmark(Description = "Explicit Interface Instance Call")]
        public int FieldExplicitTest() => _dExplicit.Run();

        [Benchmark(Description = "Implicit Interface Generic Call")]
        public int GenericImplicitTest() => GenericRunner<Implicit>();

        [Benchmark(Description = "Explicit Interface Generic Call")]
        public int GenericExplicitTest() => GenericRunner<Explicit>();

        [Benchmark(Description = "Implicit Interface Runtime Delegate Call")]
        public int ActionImplicitTest() => _compiledImplicitTest();

        [Benchmark(Description = "Explicit Interface Runtime Delegate Call")]
        public int ActionExplicitTest() => _compiledExplicitTest();

        [Benchmark(Description = "Implicit Interface Runtime MethodInfo Invoke")]
        public int MethodInfoImplicitTest() => (int) _implicitMethod.Invoke(null, null);

        [Benchmark(Description = "Explicit Interface Runtime MethodInfo Invoke")]
        public int MethodInfoExplicitTest() => (int) _explicitMethod.Invoke(null, null);

        private static int GenericRunner<T>() where T : struct, IInterface => default(T).Run();
    }

    internal class Program {
        private static void Main() => BenchmarkRunner.Run<Tester>();
    }
}