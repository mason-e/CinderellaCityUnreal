using UnityEngine;

public class AutoResumeParticleSystem : MonoBehaviour
{
    private void OnEnable()
    {
        // get all particle systems within this object
        ParticleSystem[] particleSystems = this.GetComponentsInChildren<ParticleSystem>();

        foreach (ParticleSystem particleSystem in particleSystems)
        {
            // fast-forward the particle system so when it's enabled, it doesn't start from 0
            var mainSystem = particleSystem.main;
            particleSystem.Simulate(mainSystem.startLifetimeMultiplier);

            // animate the particle system
            particleSystem.Play();
        }
    }
}