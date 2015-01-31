using System;
using System.Collections.Generic;
using Cormo.Impl.Weld.Utils;
using NUnit.Framework;

namespace Cormo.Weld.Test
{
    public class GenericMemberTranslationTest
    {
        private class GenericClass<T>
        {
            public IList<T> NormalMethod() { return null; }
            public IDictionary<T, Y> GenericMethod<Y>() { return null; }
            public IList<T> Field;
            public IList<T> Property { get; set; }
        }

        [TestCase]
        public void ResolveMethodClassGeneric()
        {
            var args = typeof (GenericClass<>).GetGenericArguments();
            var method = typeof (GenericClass<>).GetMethod("NormalMethod");
            var resolved = GenericUtils.TranslateMethodGenericArguments(method, 
                new Dictionary<Type, Type>{{args[0], typeof(string)}});

            Assert.AreEqual(typeof(GenericClass<string>).GetMethod("NormalMethod"),
                resolved);
        }

        [TestCase]
        public void ResolveMethodAndClassGenerics()
        {
            var args = typeof(GenericClass<>).GetGenericArguments();
            var method = typeof(GenericClass<>).GetMethod("GenericMethod");
            var methodArgs = method.GetGenericArguments();
            var resolved = GenericUtils.TranslateMethodGenericArguments(method,
                new Dictionary<Type, Type>
                {
                    { args[0], typeof(string) },
                    { methodArgs[0], typeof(int)}
                });

            Assert.AreEqual(typeof(GenericClass<string>).GetMethod("GenericMethod").MakeGenericMethod(typeof(int)),
                resolved);
        }

        [TestCase]
        public void ResolveFieldClassGenerics()
        {
            var args = typeof(GenericClass<>).GetGenericArguments();
            var field = typeof(GenericClass<>).GetField("Field");
            var resolved = GenericUtils.TranslateFieldType(field,
                new Dictionary<Type, Type> { { args[0], typeof(string) } });

            Assert.AreEqual(typeof(GenericClass<string>).GetField("Field"),
                resolved);
        }

        [TestCase]
        public void ResolvePropertyClassGenerics()
        {
            var args = typeof(GenericClass<>).GetGenericArguments();
            var property = typeof(GenericClass<>).GetProperty("Property");
            var resolved = GenericUtils.TranslatePropertyType(property,
                new Dictionary<Type, Type> { { args[0], typeof(string) } });

            Assert.AreEqual(typeof(GenericClass<string>).GetProperty("Property"),
                resolved);
        }
    }
}