# Unity C# Dependency Injection Lite

Unity Dependency Injection Lite is a lightweight, easy-to-use dependency injection system for Unity. It leverages Unity's `MonoBehaviour` to automatically inject dependencies into your classes. This system supports field and method injections, and allows for easy setup of providers.

## Features

- **Automatic Dependency Injection**: Automatically injects dependencies into your Unity MonoBehaviours.
- **Custom Attributes**: Use `[Inject]` and `[Provide]` attributes to denote injectable members and providers.
- **Method Injection**: Supports method injection for more complex initialization.
- **Field Injection**: Simplify your code with direct field injection.

## Usage

### Defining Injectable Classes

Use the `[Inject]` attribute on fields, methods, or properties (coming soon) to mark them as targets for injection.

```csharp
using DependencyInjection;
using UnityEngine;

public class ClassA : MonoBehaviour {
    [Inject] IEnvironmentSystem environmentSystem;
    ServiceA serviceA;

    [Inject]
    public void Init(ServiceA serviceA) {
        this.serviceA = serviceA;
    }

    void Start() {
        serviceA.Initialize("ServiceA initialized from ClassA");
        environmentSystem.Initialize();
    }
}
```

### Creating Providers

Implement IDependencyProvider and use the [Provide] attribute on methods to define how dependencies are created.

```csharp
using DependencyInjection;
using UnityEngine;

public class Provider : MonoBehaviour, IDependencyProvider {
    [Provide]
    public ServiceA ProvideServiceA() {
        return new ServiceA();
    }

    // Other provides...
}
```

### Example of Using Multiple Dependencies

```csharp
using DependencyInjection;
using UnityEngine;

public class ClassB : MonoBehaviour {
    [Inject] ServiceA serviceA;
    [Inject] ServiceB serviceB;
    FactoryA factoryA;

    [Inject]
    public void Init(FactoryA factoryA) {
        this.factoryA = factoryA;
    }

    void Start() {
        serviceA.Initialize("ServiceA initialized from ClassB");
        serviceB.Initialize("ServiceB initialized from ClassB");
        factoryA.CreateServiceA().Initialize("ServiceA initialized from FactoryA");
    }
}
```

## Setup

- Include the Dependency Injection System: Add the provided DependencyInjection namespace and its classes to your project.
- Add the Injector Component: Attach the Injector MonoBehaviour to a GameObject in your scene.
- Define Providers: Create provider MonoBehaviours and attach them to GameObjects.
- Mark Providers: Use [Provide] in your MonoBehaviours to provide a dependency of a particular type.
- Mark Dependencies: Use [Inject] in your MonoBehaviours to satifsy dependencies.

## YouTube

[**Watch the tutorial video here**](https://youtu.be/4_DTAnigmaQ)

You can also check out my [YouTube channel](https://www.youtube.com/@git-amend?sub_confirmation=1) for more Unity content.

## Inspired by

This project takes inspiration from the following open source projects:

- [Zenject](https://github.com/modesttree/Zenject)
- [USyrup](https://github.com/Jeffan207/usyrup)
