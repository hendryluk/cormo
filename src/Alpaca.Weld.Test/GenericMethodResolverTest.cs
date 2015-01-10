using System.Collections.Generic;
using Alpaca.Weld.Utils;
using NUnit.Framework;

namespace Alpaca.Weld.Test
{
    public class GenericMethodResolverTest
    {
        private class GenericClass<T>
        {
            public IList<T> NormalMethod() { return null; }

            public IDictionary<T, Y> GenericMethod<Y>() { return null; }
        }

        [TestCase]
        public void ResolveClassGeneric()
        {
            var method = typeof (GenericClass<>).GetMethod("NormalMethod");
            var resolved = GenericUtils.ResolveMethodToReturn(method, typeof (IList<string>));

            Assert.AreEqual(typeof(GenericClass<string>),
                resolved.ReflectedType);
        }
    }
}