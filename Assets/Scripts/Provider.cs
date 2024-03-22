using UnityEngine;

namespace DependencyInjection {
    public class Provider : MonoBehaviour, IDependencyProvider {

        [Provide]
        public ServiceA ProvideServiceA() => new ServiceA();

        [Provide]
        public ServiceB ProvideServiceB() => new ServiceB();

        [Provide]
        public FactoryA ProvideFactoryA() => new FactoryA();
    }

    public class ServiceA {
        // dummy implementation
        public void Initialize(string message = null) {
            Debug.Log($"ServiceA.Initialize({message})");
        }
    }

    public class ServiceB {
        // dummy implementation
        public void Initialize(string message = null) {
            Debug.Log($"ServiceB.Initialize({message})");
        }
    }

    public class FactoryA {
        ServiceA cachedServiceA;

        public ServiceA CreateServiceA() {
            if (cachedServiceA == null) {
                cachedServiceA = new ServiceA();
            }
            return cachedServiceA;
        }
    }
}
