using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Death", story: "Death Action", category: "Action", id: "aef2a45eb68f7ceb7b6c227297a3ae56")]
public partial class DeathAction : Action
{

    EnemyAnimation2D animator;

    protected override Status OnStart()
    {
        if (animator == null) return Status.Failure;
        animator.Death();
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

