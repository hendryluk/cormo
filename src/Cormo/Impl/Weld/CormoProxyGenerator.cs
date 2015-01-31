using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Castle.DynamicProxy;
using Castle.DynamicProxy.Contributors;
using Castle.DynamicProxy.Generators;
using Cormo.Injects;

namespace Cormo.Impl.Weld
{
    public class CormoProxyGenerator
    {
        private const string PROXY_PREFIX = "Cormo.Weld.Proxies";
        private static readonly ProxyGenerator ProxyGenerator =
            new ProxyGenerator(new DefaultProxyBuilder(new ModuleScope(false, false, new CormoNamingScope(),
                PROXY_PREFIX, PROXY_PREFIX, PROXY_PREFIX, PROXY_PREFIX)));

        public class CormoNamingScope : INamingScope
        {
            private readonly INamingScope _delegate;

            public CormoNamingScope()
            {
                _delegate = new NamingScope();
            }
            private CormoNamingScope(CormoNamingScope cormoNamingScope)
            {
                ParentScope = cormoNamingScope;
                _delegate = cormoNamingScope._delegate.SafeSubScope();
            }

            public string GetUniqueName(string suggestedName)
            {
                var name = _delegate.GetUniqueName(suggestedName);
                return name.Replace("Castle.Proxies", PROXY_PREFIX);
            }

            public INamingScope SafeSubScope()
            {
                return new CormoNamingScope(this);
            }

            public INamingScope ParentScope { get; private set; }
        }

        public static object CreateMixins(Type type, object[] mixins, object[] ctorParams)
        {
            var pgo = new ProxyGenerationOptions();
            foreach (var mixin in mixins)
                pgo.AddMixinInstance(mixin);

            return ProxyGenerator.CreateClassProxy(type, pgo, ctorParams);
        }

        public static object CreateProxy(Type type, Func<object> underlyingObject)
        {
            if (type.IsInterface)
            {
                return ProxyGenerator.CreateInterfaceProxyWithoutTarget(type,
                    new ForwardingInterceptor(underlyingObject));
            }
            return ProxyGenerator.CreateClassProxy(type, new ForwardingInterceptor(underlyingObject));
        }

        public class ForwardingInterceptor : IInterceptor
        {
            private readonly Func<object> _underlyingObject;

            public ForwardingInterceptor(Func<object> underlyingObject)
            {
                _underlyingObject = underlyingObject;
            }

            public void Intercept(IInvocation invocation)
            {
                try
                {
                    invocation.ReturnValue = invocation.GetConcreteMethod()
                        .Invoke(_underlyingObject(), invocation.Arguments);
                }
                catch (TargetInvocationException e)
                {
                    ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                }
            }
        }
    }

    
}