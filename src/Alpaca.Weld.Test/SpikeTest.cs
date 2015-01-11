using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Alpaca.Weld.Test
{
    public class Hen<T, Y> : IEnumerable<T> where Y: new()
    {
        public IList<IEnumerable<T>> xxx()
        {
            return null;
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public class Haha<Y>
        {
            
        }
    }

    public class Xxx<T>
    {
        public IDictionary<string, T> Blah()
        {
            return null; 
        } 
    }

    public class SpikeTest
    {
        [Test]
        public void Haha2()
        {
            var a = typeof (Xxx<>).GetMethod("Blah").ReturnType;
            var ae = a.GetInterface(typeof (IEnumerable<>).FullName);

            Assert.IsNull(ae.GetGenericArguments());

            //var a = typeof (Hen<,>);
            //var b = typeof (IDictionary<,>);
            //Assert.IsNotNull(a.MakeGenericType(b.GetGenericArguments()));
            //Assert.IsNull(a.GetGenericArguments()[1].GetGenericParameterConstraints());
        }

        public void Haha1()
        {
            var a = typeof (Hen<,>).GetMethod("xxx");
            //a.ReflectedType.GetMethod(a.)
            Assert.IsNull(a.GetGenericArguments());
        }

        public void Haha()
        {
            var a = typeof(Hen<, >.Haha<>);
            var x = typeof (Hen<int, object>.Haha<string>);

            //Assert.AreEqual(3, a.GetGenericArguments());

            var b = a.MakeGenericType(typeof(int), typeof(object), typeof (string));

            Assert.AreEqual(b, x);
        }

    }
    
}