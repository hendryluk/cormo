using System;
using System.Collections.Generic;
using System.Linq;
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

        public static object CreateMixins(Type type, IDictionary<Type, object> mixins, object[] ctorParams)
        {
            //var pgo = new ProxyGenerationOptions();
            //foreach (var mixin in mixins)
            //    pgo.AddMixinInstance(mixin);

            return ProxyGenerator.CreateClassProxy(type, mixins.Keys.ToArray(), new ProxyGenerationOptions(), ctorParams, new MixinInterceptor(mixins));
        }

        public class MixinInterceptor: IInterceptor
        {
            private readonly IDictionary<Type, object> _mixins;

            public MixinInterceptor(IDictionary<Type, object> mixins)
            {
                _mixins = mixins;
            }

            public void Intercept(IInvocation invocation)
            {
                object mixin;
                if (_mixins.TryGetValue(invocation.Method.DeclaringType, out mixin))
                {
                    invocation.ReturnValue = invocation.Method.Invoke(mixin, invocation.Arguments);
                }
                else
                {
                    invocation.Proceed();
                } 
            }
        }

        public static object CreateProxy(Type[] types, Func<object> underlyingObject)
        {
            var classType = types.FirstOrDefault(x => !x.IsInterface);
            if (classType == null)
            {
                return ProxyGenerator.CreateInterfaceProxyWithoutTarget(
                    types.First(), types.Skip(1).ToArray(),
                    new ForwardingInterceptor(underlyingObject));
            }
            return ProxyGenerator.CreateClassProxy(classType, 
                types.Except(new[]{classType}).ToArray(), 
                new ForwardingInterceptor(underlyingObject));
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