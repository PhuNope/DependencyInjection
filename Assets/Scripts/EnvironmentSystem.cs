using UnityEngine;

namespace DependencyInjection {
    public interface IEnvironmentSystem {
        void Initilize();
    }

    public class EnvironmentSystem : MonoBehaviour, IDependencyProvider, IEnvironmentSystem {
        [Provide]
        IEnvironmentSystem ProvideEnvironmentSystem() {
            return this;
        }

        public void Initilize() {
            Debug.Log("EnvironmentSystem.Initilize()");
        }
    }
}
