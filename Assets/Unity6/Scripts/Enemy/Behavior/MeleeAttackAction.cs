using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MeleeAttackAction", story: "[Self] Attack [Target] MeleeWeapon", category: "Action", id: "3bc76645cf2196d0232063bb8c89bb68")]
public partial class MeleeAttackAction : Action
{
    [SerializeReference] 
    public BlackboardVariable<GameObject> Self;
    [SerializeReference] 
    public BlackboardVariable<GameObject> Target;
    [SerializeReference]
    public BlackboardVariable<EnemyAnimation2D> animator;

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

