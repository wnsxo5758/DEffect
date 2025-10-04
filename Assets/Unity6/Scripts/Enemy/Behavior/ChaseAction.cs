using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ChaseAction", story: "[Self] Chase [Target]", category: "Action", id: "8c2bbd2ff2b538b57bca16da274c3b98")]
public partial class ChaseAction : Action
{
    [SerializeReference] 
    public BlackboardVariable<GameObject> Self;
    [SerializeReference] 
    public BlackboardVariable<GameObject> Target;
    [SerializeReference]
    public BlackboardVariable<float> chaseSpeed; 

    public Movement2D Movement;
    protected override Status OnStart()
    {

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

