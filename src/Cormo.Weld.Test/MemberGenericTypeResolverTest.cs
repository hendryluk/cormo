using System;
using System.Collections.Generic;
using Cormo.Weld.Utils;
using NUnit.Framework;

namespace Cormo.Weld.Test
{
    public class MemberGenericTypeResolverTest
    {
        private interface IList2<T, U>: IEnumerable<T>
        {
        }

        private interface IPartialDictionary<TKey> : IDictionary<TKey, int>
        {
        }

        private class GenericClass<T>
        {
            public T Tee() { return default(T); }

            public IList<T> List() { return null; }

            public IList<string> ListString() { return null; }

            public IList2<T, Y> List2<Y>() { return null; }

            public IDictionary<T, Y> Dictionary<Y>() { return null; }

            public IList<KeyValuePair<T, Y>> ListKeyValuePair<Y>() { return null; }

            public IDictionary<T, int> DictionaryTInt() { return null; }

            public IList<KeyValuePair<string, int>> ListKeyValuePairStringInt() { return null; }
        }

        private interface IDisposableList<T> : IList<T> where T : IDisposable { }


        public Type Resolve<T>(string name)
        {
            var resolution = GenericUtils.ResolveGenericType(GetMethodType(name), typeof(T));
            if (resolution == null)
                return null;
            return resolution.ResolvedType;
        }

        public Type GetMethodType(string name)
        {
            return typeof (GenericClass<>).GetMethod(name).ReturnType;
        }

        [Test]
        public void ResolveOpenGenerics()
        {
            Assert.AreEqual(typeof (IList<string>), Resolve<IEnumerable<string>>("List"));
        }

        [Test]
        public void RejectComponentWithTooManyGenericArguments()
        {
            Assert.IsNull(Resolve<IEnumerable<string>>("List2"));
        }

        [Test]
        public void ResolveNestedOpenGenerics()
        {
            Assert.AreEqual(typeof(IList<KeyValuePair<string, int>>),
                Resolve<IEnumerable<KeyValuePair<string, int>>>("ListKeyValuePairStringInt"));
        }

        [Test]
        public void ResolveChildOfNestedOpenGenerics()
        {
            Assert.AreEqual(typeof (IDictionary<string, int>),
                Resolve<IEnumerable<KeyValuePair<string, int>>>("Dictionary"));
        }

        [Test]
        public void ResolveNestedPartiallyClosedGenerics()
        {
            Assert.AreEqual(typeof(IDictionary<string, int>),
                Resolve<IEnumerable<KeyValuePair<string, int>>>("DictionaryTInt"));
        }

        [Test]
        public void RejectIncompatibleNestedPartiallyClosedGenerics()
        {
            Assert.IsNull(
                Resolve<IEnumerable<KeyValuePair<string, string>>>("DictionaryTInt"));
        }

        [Test]
        public void ResolveAssignableOutGenericArgument()
        {
            Assert.AreEqual(typeof (IList<string>),
                Resolve<IEnumerable<object>>("ListString"));
        }

        [Test]
        public void ResolveGenericArgumentWithRestrictiveConstraint()
        {
            var resolution = GenericUtils.ResolveGenericType(typeof(IDisposableList<>), GetMethodType("List"));
            Assert.IsNotNull(resolution);
        }

        [Test]
        public void ResolveTIntoString()
        {
            Assert.AreEqual(typeof(string),
                Resolve<string>("Tee"));
        }
    }
}