# Unity C# Dependency Injection Lite

![DependencyInjection](https://github.com/adammyhre/Unity-Dependency-Injection-Lite/assets/38876398/5a7c9183-8eb4-4346-b6e2-1d5b28ec7268)

Unity Dependency Injection Lite is a lightweight, easy-to-use dependency injection system for Unity. It leverages Unity's `MonoBehaviour` to automatically inject dependencies into your classes. This system supports field, method and property injections, and allows for easy setup of providers.

## Features

- **Automatic Dependency Injection**: Automatically injects dependencies into your Unity MonoBehaviours.
- **Custom Attributes**: Use `[Inject]` and `[Provide]` attributes to denote injectable members and providers.
- **Method Injection**: Supports method injection for more complex and multiple initialization.
- **Field Injection**: Simplify your code with direct field injection.
- **Property Injection**: Supports property injection.

## Usage

### Defining Injectable Fields, Methods and Properties

Use the `[Inject]` attribute on fields, methods, or properties to mark them as targets for injection.

```csharp
using DependencyInjection;
using UnityEngine;

public class ClassA : MonoBehaviour {
    [Inject] IServiceA serviceA;
    
    IServiceB serviceB;
    
    [Inject]
    public void Init(ServiceB service) {
        this.serviceB = service;
    }
    
    [Inject]
    public IServiceC Service { get; private set; }
}
```

### Creating Providers

Implement IDependencyProvider and use the `[Provide]` attribute on methods to define how dependencies are created.

```csharp
using DependencyInjection;
using UnityEngine;

public class Provider : MonoBehaviour, IDependencyProvider {
    [Provide]
    public IServiceA ProvideServiceA() {
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
    [Inject] IServiceA serviceA;
    
    IServiceB serviceB;
    IFactoryA factoryA;
        
    [Inject] // Method injection supports multiple dependencies
    public void Init(IFactoryA factoryA, IServiceB serviceB) {
        this.factoryA = factoryA;
        this.serviceB = serviceB;
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

[**Watch the tutorial video here**](https://youtu.be/PJcBJ60C970)

You can also check out my [YouTube channel](https://www.youtube.com/@git-amend?sub_confirmation=1) for more Unity content.

## Inspired by

This project takes inspiration from the following open source projects:

- [Zenject](https://github.com/modesttree/Zenject)
- [USyrup](https://github.com/Jeffan207/usyrup)
