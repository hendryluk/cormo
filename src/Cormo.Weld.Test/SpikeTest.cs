using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using Cormo.Impl.Weld.Reflects;
using Microsoft.CSharp;
using NUnit.Framework;

namespace Cormo.Weld.Test
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
        public string _xxx;

        [Test]
        public void Haha2()
        {
            //Assert.AreEqual("", typeof(IEnumerable<string>).Name);
            var one = typeof(Object).GetMethod("ToString");
            var two = typeof(AnnotatedType).GetMethod("ToString");


            Assert.AreEqual(one.GetBaseDefinition(), two.GetBaseDefinition());
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