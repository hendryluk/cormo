using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Alpaca.Weld.Utils;
using NUnit.Framework;

namespace Alpaca.Weld.Test
{
    public class GenericTypeResolverTest
    {
        private interface IList2<T, U>: IEnumerable<T>
        {
        }

        private interface IPartialDictionary<TKey> : IDictionary<TKey, int>
        {
        }

        private class GenericClass<T>
        {
            public IList<T> List() { return null; }

            public IList<string> ListString() { return null; }

            public IList2<T, Y> List2<Y>() { return null; }

            public IDictionary<T, Y> Dictionary<Y>() { return null; }

            public IList<KeyValuePair<T, Y>> ListKeyValuePair<Y>() { return null; }

            public IDictionary<T, int> DictionaryTInt() { return null; }

            public IDictionary<string, int> DictionaryStringInt() { return null; }

            public IDictionary<string, object> DictionaryStringObject() { return null; }

            public IList<KeyValuePair<string, int>> ListKeyValuePairStringInt() { return null; }
        }

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
        public void ResolveClosedGenerics()
        {
            Assert.AreEqual(typeof(IList<string>), Resolve<IEnumerable<string>>("ListString"));
        }

        [Test]
        public void RejectIncompatibleClosedGenerics()
        {
            Assert.IsNull(Resolve<IEnumerable<int>>("ListString"));
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
        public void ResolveChildOfNestedClosedGenerics()
        {
            Assert.AreEqual(typeof(IDictionary<string, int>),
                Resolve<IEnumerable<KeyValuePair<string, int>>>("DictionaryStringInt"));
        }

        [Test]
        public void RejectIncompatibleChildOfNestedClosedGenerics()
        {
            Assert.IsNull(
                Resolve<IEnumerable<KeyValuePair<string, int>>>("DictionaryStringObject"));
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
    }
}