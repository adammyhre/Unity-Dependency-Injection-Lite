using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DependencyInjection {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    public sealed class InjectAttribute : PropertyAttribute { }
    
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ProvideAttribute : PropertyAttribute { }

    public interface IDependencyProvider { }

    [DefaultExecutionOrder(-1000)]
    public class Injector : MonoBehaviour {
        const BindingFlags k_bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        
        readonly Dictionary<Type, object> registry = new();

        void Awake() {
            var monoBehaviours = FindMonoBehaviours();
            
            // Find all modules implementing IDependencyProvider and register the dependencies they provide
            var providers = monoBehaviours.OfType<IDependencyProvider>();
            foreach (var provider in providers) {
                Register(provider);
            }
            
            // Find all injectable objects and inject their dependencies
            var injectables = monoBehaviours.Where(IsInjectable);
            foreach (var injectable in injectables) {
                Inject(injectable);
            }
        }

        // Register an instance of a type outside of the normal dependency injection process
        public void Register<T>(T instance) {
            registry[typeof(T)] = instance;
        }
        
        void Inject(object instance) {
            var type = instance.GetType();
            
            // Inject into fields
            var injectableFields = type.GetFields(k_bindingFlags)
                .Where(member => Attribute.IsDefined(member, typeof(InjectAttribute)));

            foreach (var injectableField in injectableFields) {
                if (injectableField.GetValue(instance) != null) {
                    Debug.LogWarning($"[Injector] Field '{injectableField.Name}' of class '{type.Name}' is already set.");
                    continue;
                }
                var fieldType = injectableField.FieldType;
                var resolvedInstance = Resolve(fieldType);
                if (resolvedInstance == null) {
                    throw new Exception($"Failed to inject dependency into field '{injectableField.Name}' of class '{type.Name}'.");
                }
                
                injectableField.SetValue(instance, resolvedInstance);
            }
            
            // Inject into methods
            var injectableMethods = type.GetMethods(k_bindingFlags)
                .Where(member => Attribute.IsDefined(member, typeof(InjectAttribute)));

            foreach (var injectableMethod in injectableMethods) {
                var requiredParameters = injectableMethod.GetParameters()
                    .Select(parameter => parameter.ParameterType)
                    .ToArray();
                var resolvedInstances = requiredParameters.Select(Resolve).ToArray();
                if (resolvedInstances.Any(resolvedInstance => resolvedInstance == null)) {
                    throw new Exception($"Failed to inject dependencies into method '{injectableMethod.Name}' of class '{type.Name}'.");
                }
                
                injectableMethod.Invoke(instance, resolvedInstances);
            }
            
            // Inject into properties
            var injectableProperties = type.GetProperties(k_bindingFlags)
                .Where(member => Attribute.IsDefined(member, typeof(InjectAttribute)));
            foreach (var injectableProperty in injectableProperties) {
                var propertyType = injectableProperty.PropertyType;
                var resolvedInstance = Resolve(propertyType);
                if (resolvedInstance == null) {
                    throw new Exception($"Failed to inject dependency into property '{injectableProperty.Name}' of class '{type.Name}'.");
                }

                injectableProperty.SetValue(instance, resolvedInstance);
            }
        }

        void Register(IDependencyProvider provider) {
            var methods = provider.GetType().GetMethods(k_bindingFlags);

            foreach (var method in methods) {
                if (!Attribute.IsDefined(method, typeof(ProvideAttribute))) continue;
                
                var returnType = method.ReturnType;
                var providedInstance = method.Invoke(provider, null);
                if (providedInstance != null) {
                    registry.Add(returnType, providedInstance);
                } else {
                    throw new Exception($"Provider method '{method.Name}' in class '{provider.GetType().Name}' returned null when providing type '{returnType.Name}'.");
                }
            }
        }

        public void ValidateDependencies() {
            var monoBehaviours = FindMonoBehaviours();
            var providers = monoBehaviours.OfType<IDependencyProvider>();
            var providedDependencies = GetProvidedDependencies(providers);

            var invalidDependencies = monoBehaviours
                .SelectMany(mb => mb.GetType().GetFields(k_bindingFlags), (mb, field) => new {mb, field})
                .Where(t => Attribute.IsDefined(t.field, typeof(InjectAttribute)))
                .Where(t => !providedDependencies.Contains(t.field.FieldType) && t.field.GetValue(t.mb) == null)
                .Select(t => $"[Validation] {t.mb.GetType().Name} is missing dependency {t.field.FieldType.Name} on GameObject {t.mb.gameObject.name}");
            
            var invalidDependencyList = invalidDependencies.ToList();
            
            if (!invalidDependencyList.Any()) {
                Debug.Log("[Validation] All dependencies are valid.");
            } else {
                Debug.LogError($"[Validation] {invalidDependencyList.Count} dependencies are invalid:");
                foreach (var invalidDependency in invalidDependencyList) {
                    Debug.LogError(invalidDependency);
                }
            }
        }
        
        HashSet<Type> GetProvidedDependencies(IEnumerable<IDependencyProvider> providers) {
            var providedDependencies = new HashSet<Type>();
            foreach (var provider in providers) {
                var methods = provider.GetType().GetMethods(k_bindingFlags);
                
                foreach (var method in methods) {
                    if (!Attribute.IsDefined(method, typeof(ProvideAttribute))) continue;
                    
                    var returnType = method.ReturnType;
                    providedDependencies.Add(returnType);
                }
            }

            return providedDependencies;
        }

        public void ClearDependencies() {
            foreach (var monoBehaviour in FindMonoBehaviours()) {
                var type = monoBehaviour.GetType();
                var injectableFields = type.GetFields(k_bindingFlags)
                    .Where(member => Attribute.IsDefined(member, typeof(InjectAttribute)));

                foreach (var injectableField in injectableFields) {
                    injectableField.SetValue(monoBehaviour, null);
                }
            }
            
            Debug.Log("[Injector] All injectable fields cleared.");
        }

        object Resolve(Type type) {
            registry.TryGetValue(type, out var resolvedInstance);
            return resolvedInstance;
        }

        static MonoBehaviour[] FindMonoBehaviours() {
            return FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.InstanceID);
        }

        static bool IsInjectable(MonoBehaviour obj) {
            var members = obj.GetType().GetMembers(k_bindingFlags);
            return members.Any(member => Attribute.IsDefined(member, typeof(InjectAttribute)));
        }
    }
}