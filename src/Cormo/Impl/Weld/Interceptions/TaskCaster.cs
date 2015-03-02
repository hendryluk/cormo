using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Cormo.Impl.Weld.Interceptions
{
    public interface ITaskCaster
    {
        Task Cast(Task<object> task);
        Task<object> ToObject(Task task);
    }

    public class TaskCasters
    {
        static readonly ConcurrentDictionary<Type, ITaskCaster> _casters = new ConcurrentDictionary<Type, ITaskCaster>();

        public static ITaskCaster ForVoid()
        {
            return TaskCaster.Instance;
        }
        public static ITaskCaster ForType(Type type)
        {
            if (type == typeof(object))
                return null;
            return _casters.GetOrAdd(type, _ => (ITaskCaster)Activator.CreateInstance(typeof(TaskCaster<>).MakeGenericType(type)));
        }

        private class TaskCaster : ITaskCaster
        {
            public static TaskCaster Instance = new TaskCaster();
            public async Task Cast(Task<object> task)
            {
                await task;
            }

            public async Task<object> ToObject(Task task)
            {
                await task;
                return null;
            }
        }

        private class TaskCaster<T> : ITaskCaster
        {
            private static async Task<T> CastTask(Task<object> task)
            {
                return (T)await task;
            }

            public Task Cast(Task<object> task)
            {
                return CastTask(task);
            }

            public async Task<object> ToObject(Task task)
            {
                return (object)await (Task<T>)task;
            }
        }
    }

    
}