using UnityEngine;
using UnityEngine.AI;

// Responsible for making the NavMeshAgent follow the FPSController (default behavior)
// Or, when GuidedTourMode is active, FPSController will follow NavMeshAgent
// Must be attached to the NavMeshAgent in each scene

public class FollowPlayerOrAgent : MonoBehaviour
{
    private void Update()
    {
        // only follow player if guided tour is NOT active or paused
        if (!ModeState.isGuidedTourActive && !ModeState.isGuidedTourPaused)
        {
            if (ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform != null)
            {
                // update this object's position to match the player's last known position
                GetComponent<NavMeshAgent>().nextPosition = ManageFPSControllers.FPSControllerGlobals.activeFPSControllerTransform.position;
                this.GetComponent<NavMeshAgent>().updatePosition = true;
            }
            // if guided tour is active, FPSController must follow agent
        } else
        {
            // this is required to be false for NavMeshAgent to not interfere
            this.GetComponent<NavMeshAgent>().updatePosition = false;

            // move the CharacterController, the parent, and the agent's transform
            // to the NavMeshAgent's next position
            this.transform.parent.GetComponentInChildren<CharacterController>().SimpleMove(this.GetComponent<NavMeshAgent>().velocity);
            Vector3 positionNoY = new Vector3(this.GetComponent<NavMeshAgent>().nextPosition.x, this.transform.parent.transform.position.y, this.GetComponent<NavMeshAgent>().nextPosition.z);
            this.transform.parent.transform.position = positionNoY;
            this.transform.position = positionNoY;
        }
    }
}