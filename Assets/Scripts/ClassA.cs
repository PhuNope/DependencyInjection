using UnityEngine;

namespace DependencyInjection {
    public class ClassA : MonoBehaviour {
        ServiceA serviceA;

        [Inject]
        public void Init(ServiceA serviceA) {
            this.serviceA = serviceA;
        }

        [Inject] IEnvironmentSystem environmentSystem;

        private void Start() {
            serviceA.Initialize("ServiceA initialized from ClassA");
            environmentSystem.Initilize();
        }
    }
}
