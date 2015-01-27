using System;
using System.Collections.Generic;
using Cormo.Injects;
using Cormo.Weld.Utils;
using NUnit.Framework;

namespace Cormo.Weld.Test
{
    public class ClassGenericTypeResolverTest
    {
        private interface IList2<T, U>: IEnumerable<T>
        {
        }

        private interface IPartialDictionary<TKey> : IDictionary<TKey, int>
        {
        }

        public Type Resolve<TInject>(Type produce)
        {
            var resolution = GenericUtils.ResolveGenericType(produce, typeof(TInject));
            if (resolution == null)
                return null;
            return resolution.ResolvedType;
        }

        [Test]
        public void ResolveOpenGenerics()
        {
            Assert.AreEqual(typeof (IList<string>), Resolve<IEnumerable<string>>(typeof(IList<>)));
        }

        [Test]
        public void ResolveClosedGenerics()
        {
            Assert.AreEqual(typeof(IList<string>), Resolve<IEnumerable<string>>(typeof(IList<string>)));
        }

        [Test]
        public void RejectIncompatibleClosedGenerics()
        {
            Assert.IsNull(Resolve<IEnumerable<int>>(typeof(IList<string>)));
        }

        [Test]
        public void RejectComponentWithTooManyGenericArguments()
        {
            Assert.IsNull(Resolve<IEnumerable<string>>(typeof(IList2<,>)));
        }

        [Test]
        public void ResolveNestedOpenGenerics()
        {
            Assert.AreEqual(typeof(IList<KeyValuePair<string, int>>),
                Resolve<IEnumerable<KeyValuePair<string, int>>>(typeof(IList<KeyValuePair<string, int>>)));
        }

        [Test]
        public void ResolveChildOfNestedOpenGenerics()
        {
            Assert.AreEqual(typeof (IDictionary<string, int>),
                Resolve<IEnumerable<KeyValuePair<string, int>>>(typeof(IDictionary<,>)));
        }

        [Test]
        public void ResolveChildOfNestedClosedGenerics()
        {
            Assert.AreEqual(typeof(IDictionary<string, int>),
                Resolve<IEnumerable<KeyValuePair<string, int>>>(typeof(IDictionary<string, int>)));
        }

        [Test]
        public void RejectIncompatibleChildOfNestedClosedGenerics()
        {
            Assert.IsNull(
                Resolve<IEnumerable<KeyValuePair<string, int>>>(typeof(IDictionary<string, object>)));
        }

        [Test]
        public void ResolveNestedPartiallyClosedGenerics()
        {
            Assert.AreEqual(typeof(IPartialDictionary<string>),
                Resolve<IEnumerable<KeyValuePair<string, int>>>(typeof(IPartialDictionary<>)));
        }

        [Test]
        public void RejectIncompatibleNestedPartiallyClosedGenerics()
        {
            Assert.IsNull(
                Resolve<IEnumerable<KeyValuePair<string, string>>>(typeof(IPartialDictionary<>)));
        }

        [Test]
        public void ResolveAssignableOutGenericArgument()
        {
            Assert.AreEqual(typeof (IList<string>),
                Resolve<IEnumerable<object>>(typeof(IList<string>)));
        }

        [Test]
        public void ResolveMismatchedGenerics()
        {
            Assert.IsNull(Resolve<IComparer<string>>(typeof(ComparerEnumerable<>)));
        }

        private abstract class ComparerEnumerable<T> : IComparer<IEnumerable<T>> {
            public int Compare(IEnumerable<T> x, IEnumerable<T> y)
            {
                throw new NotImplementedException();
            }
        } 
    }
}