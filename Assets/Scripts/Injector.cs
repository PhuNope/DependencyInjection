using Malevolent;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DependencyInjection {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method)]
    public sealed class InjectAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ProvideAttribute : Attribute { }

    public interface IDependencyProvider { }

    [DefaultExecutionOrder(-1000)]
    public class Injector : Singleton<Injector> {
        const BindingFlags k_bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        readonly Dictionary<Type, object> registry = new Dictionary<Type, object>();

        protected override void Awake() {
            base.Awake();

            // Find all modules implementing IDependencyProvider
            var providers = FindMonoBehaviors().OfType<IDependencyProvider>();
            foreach (var provider in providers) {
                RegisterProvider(provider);
            }

            // Find all injectable objects and inject their dependencies
            var injectables = FindMonoBehaviors().Where(IsInjectable);
            foreach (var injectable in injectables) {
                Inject(injectable);
            }
        }

        void Inject(object instance) {
            var type = instance.GetType();
            var injectableFields = type.GetFields(k_bindingFlags)
                .Where(member => Attribute.IsDefined(member, typeof(InjectAttribute)));

            foreach (var injectableField in injectableFields) {
                var fieldType = injectableField.FieldType;
                var resolvedInstance = Resolve(fieldType);
                if (resolvedInstance == null) {
                    throw new Exception($"Failed to inject {fieldType.Name} into {type.Name}");
                }

                injectableField.SetValue(instance, resolvedInstance);
                Debug.Log($"Field Injected {fieldType.Name} into {type.Name}");
            }

            var injectableMethods = type.GetMethods(k_bindingFlags)
                .Where(member => Attribute.IsDefined(member, typeof(InjectAttribute)));

            foreach (var injectablMethod in injectableMethods) {
                var requiredParameters = injectablMethod.GetParameters()
                    .Select(parameter => parameter.ParameterType)
                    .ToArray();

                var resolvedInstance = requiredParameters.Select(Resolve).ToArray();
                if (requiredParameters.Any(resolvedInstance => resolvedInstance == null)) {
                    throw new Exception($"Failed to inject {type.Name}.{injectablMethod.Name}");
                }

                injectablMethod.Invoke(instance, resolvedInstance);
                Debug.Log($"Method Injected {type.Name}.{injectablMethod.Name}");
            }
        }

        object Resolve(Type type) {
            registry.TryGetValue(type, out var resolvedInstance);
            return resolvedInstance;
        }

        static bool IsInjectable(MonoBehaviour obj) {
            var members = obj.GetType().GetMembers(k_bindingFlags);
            return members.Any(member => Attribute.IsDefined(member, typeof(InjectAttribute)));
        }

        void RegisterProvider(IDependencyProvider provider) {
            var methods = provider.GetType().GetMethods(k_bindingFlags);

            foreach (var method in methods) {
                if (!Attribute.IsDefined(method, typeof(ProvideAttribute))) continue;

                var returnType = method.ReturnType;
                var providedInstance = method.Invoke(provider, null);
                if (provider != null) {
                    registry.Add(returnType, providedInstance);
                    Debug.Log($"Registered {returnType.Name} from {provider.GetType().Name}");
                }
                else {
                    throw new Exception($"Provider {provider.GetType().Name} returned null for {returnType.Name}");
                }
            }
        }

        static MonoBehaviour[] FindMonoBehaviors() {
            return FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.InstanceID);
        }
    }
}
