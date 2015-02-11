using System;
using System.Collections.Generic;
using Cormo.Impl.Weld.Utils;
using NUnit.Framework;

namespace Cormo.Weld.Test
{
    public class GenericAncestorTypeResolverTest
    {
        public Type Resolve<TInject>(Type produce)
        {
            var resolution = GenericResolver.AncestorResolver.ResolveType(produce, typeof(TInject));
            if (resolution == null)
                return null;
            return resolution.ResolvedType;
        }

        [Test]
        public void ResolveOpenGenerics()
        {
            Assert.AreEqual(typeof(IEnumerable<string>), Resolve<List<string>>(typeof(IEnumerable<>)));
        }
    }
}