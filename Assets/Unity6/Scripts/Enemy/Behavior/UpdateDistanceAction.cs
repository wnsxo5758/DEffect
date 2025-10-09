using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "UpdateDistanceAction", story: "Update [Self] and [Target] [currentDistance]", category: "Action", id: "1687b63a4c2696b02726d73d1d43fa80")]
public partial class UpdateDistanceAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> currentDistance;

    protected override Status OnUpdate()
    {
        if (Target == null || Target.Value == null)
        {
            Debug.LogWarning("UpdateDistanceAction: Target is null!");
            return Status.Failure;
        }

        currentDistance.Value = Vector2.Distance(Self.Value.transform.position, Target.Value.transform.position);
        //Debug.Log($"distance{CurrentDistance.Value}");
        return Status.Success;
    }

}

