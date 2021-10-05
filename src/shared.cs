using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Leopotam.EcsLite;

namespace GoodCat.EcsLite.Shared 
{
    public static class shared
    {
        private const string SharedFieldNameInEcsSystems = "_shared";
        private static readonly Type SharedAttrType = typeof(EcsInjectAttribute);

        public static EcsSystems InjectShared<T>(this EcsSystems systems,  T instance)
        {
            var sharedObject = systems.GetShared<object>();
            Shared shared = null;
            if (sharedObject == null)
            {
                var fieldShared = systems.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                    .FirstOrDefault(fieldInfo => fieldInfo.Name == SharedFieldNameInEcsSystems);
                shared = new Shared();
                // ReSharper disable once PossibleNullReferenceException
                fieldShared.SetValue(systems, shared);
            }
            else if (sharedObject is Shared == false)
            {
                throw new Exception(
                    $"You cannot use InjectShared() when the Shared object has already been used in EcsSystems ({sharedObject.GetType().Name})," +
                    $"it should be: new EcsSystems(world);");
            }
            else
            {
                shared = (Shared)sharedObject;
            }

            // ReSharper disable once PossibleNullReferenceException
            shared.Set(instance);
            return systems;
        }

        public static EcsSystems InitShared(this EcsSystems systems)
        {
            IEcsSystem[] allSystems = null;
            var systemsCount = systems.GetAllSystems(ref allSystems);

            var shared = systems.GetShared<Shared>();

            for (var i = 0; i < systemsCount; i++)
            {
                var system = allSystems[i];
                foreach (var fieldInfo in system.GetType()
                    .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    SetFieldValue(shared, system, fieldInfo);
                }
            }

            return systems;
        }

        private static void SetFieldValue(Shared shared, IEcsSystem system, FieldInfo fieldInfo)
        {
            if (shared != null && Attribute.IsDefined(fieldInfo, SharedAttrType))
            {
                var value = shared.Get(fieldInfo.FieldType);
                fieldInfo.SetValue(system, value);
            }
        }

        public class Shared
        {
            private readonly Dictionary<Type, object> _dictionary = new Dictionary<Type, object>();

            public void Set<T>(T instance)
            {
                _dictionary[typeof(T)] = instance;
            }

            public T Get<T>() => (T)Get(typeof(T));

            public object Get(Type type)
            {
#if DEBUG
                if (_dictionary.ContainsKey(type) == false)
                {
                    throw new Exception($"The instance of type {type.Name} has not been injected using the InjectShared(...) method!");
                }
#endif
                return _dictionary[type];
            }
        }
    }
    
    public class EcsInjectAttribute : Attribute
    {
    }
}