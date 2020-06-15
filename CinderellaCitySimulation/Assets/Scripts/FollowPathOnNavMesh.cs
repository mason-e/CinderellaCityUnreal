using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]

// this script needs to be attached to an object that should follow a defined path

public class FollowPathOnNavMesh : MonoBehaviour
{
    public NavMeshAgent thisAgent;
    public UpdateNPCAnimatorByState thisAnimatorUpdateScript;
    public Vector3 initialDestination;
    public NavMeshPath path;

    private void OnEnable()
    {
        // keep track of how many NPCControllers are active and pathfinding
        NPCControllerGlobals.activeNPCControllers++;
        //Utils.DebugUtils.DebugLog("Active NPCControllers following paths: " + NPCControllerGlobals.activeNPCControllers);

        // if a path was previously recorded, use it
        if (path != null)
        {
            this.GetComponent<NavMeshAgent>().path = path;
        }

    }

    private void Awake()
    {
        thisAgent = this.GetComponent<NavMeshAgent>();
        thisAnimatorUpdateScript = this.GetComponent<UpdateNPCAnimatorByState>();

        // record this object's position in the pool so others can reference it later
        NPCControllerGlobals.initialNPCPositionsList.Add(this.transform.position);
    }

    void Start()
    {
        // if the NPC positions array hasn't been converted yet, convert it
        if (NPCControllerGlobals.initialNPCPositionsArray == null)
        {
            NPCControllerGlobals.initialNPCPositionsArray = NPCControllerGlobals.initialNPCPositionsList.ToArray();
        }

        // instantiate an empty path that it will follow
        path = new NavMeshPath();

        // set the destination
        initialDestination = Utils.GeometryUtils.GetRandomPointOnNavMeshFromPool(this.transform.position, NPCControllerGlobals.initialNPCPositionsArray, 0, NPCControllerGlobals.maxDestinationDistance, true);

        // find a path to the destination
        bool pathSuccess = NavMesh.CalculatePath(this.gameObject.transform.position, initialDestination, NavMesh.AllAreas, path);

        // if a path was created, set this agent to use it
        if (pathSuccess)
        {
            thisAgent.GetComponent<NavMeshAgent>().SetPath(path);
        }
        else
        {
            Utils.DebugUtils.DebugLog("Agent " + thisAgent.transform.gameObject.name + " failed to find a path.");
        }
    }

    private void Update()
    {
        if (!thisAgent.pathPending)
        {
            float currentVelocity = thisAgent.velocity.magnitude;

            // if this agent's speed gets too low, it's likely colliding badly with others
            // to prevent a traffic jam, find a different random point from the pool to switch directions
            if (currentVelocity > 0f && currentVelocity < NPCControllerGlobals.minimumSpeedBeforeRepath)
            {
                thisAgent.SetDestination(Utils.GeometryUtils.GetRandomPointOnNavMeshFromPool(this.transform.position, NPCControllerGlobals.initialNPCPositionsArray, NPCControllerGlobals.minDiscardDistance, NPCControllerGlobals.maxDiscardDistance, true));

                // optional: visualize the path with a red line in the editor
                Debug.DrawLine(this.transform.position, thisAgent.destination, Color.red, Time.deltaTime);
            }

            // if the agent gets within range of the destination, consider it arrived
            // this prevents the agent from fighting with another for the same point in space
            else if (thisAgent.remainingDistance <= NPCControllerGlobals.defaultNPCStoppingDistance)
            {
                // reached the destination - now set another one
                thisAgent.SetDestination(Utils.GeometryUtils.GetRandomPointOnNavMeshFromPool(this.transform.position, NPCControllerGlobals.initialNPCPositionsArray, 0, NPCControllerGlobals.maxDestinationDistance, true));
                Utils.DebugUtils.DebugLog("Agent " + thisAgent.gameObject.name + " reached its destination.");
            }
        }
    }

    private void OnDisable()
    {
        NPCControllerGlobals.activeNPCControllers--;

        // keep track of how many NPCControllers are active and pathfinding
        path = this.GetComponent<NavMeshAgent>().path;
    }
}