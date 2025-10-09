using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using static UnityEngine.EventSystems.EventTrigger;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SelfSetUp", story: "[Self] SetUp", category: "Action", id: "159295c9693ec82476b4a077c37eef94")]
public partial class SelfSetUpAction : Action
{
    [SerializeReference]
    public BlackboardVariable<GameObject> Self;
    [SerializeReference] 
    public BlackboardVariable<Movement2D> mover;
    [SerializeReference] 
    public BlackboardVariable<EnemyAnimator> animator;
    [SerializeReference] 
    public BlackboardVariable<EnemyHp> hp;
    protected override Status OnStart()
    {
        if (mover.Value == null)
            mover.Value = Self.Value.GetComponent<Movement2D>();

        if (animator.Value == null)
            animator.Value = Self.Value.GetComponentInChildren<EnemyAnimator>();

        if (hp.Value == null)
            hp.Value = Self.Value.GetComponent<EnemyHp>();

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

