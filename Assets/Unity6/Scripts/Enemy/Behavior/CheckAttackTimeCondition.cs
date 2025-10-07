using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "CheckAttackTime", story: "Check if [LastAttackTime] is Ready : [AttackCoolDown]", category: "Conditions", id: "0cc4ef8aea19074b62ede5563f89f8ea")]
public partial class CheckAttackTimeCondition : Condition
{
    [SerializeReference] public BlackboardVariable<float> LastAttackTime;
    [SerializeReference] public BlackboardVariable<float> AttackCooldown;

    public override bool IsTrue()
    {
        return Time.time >= LastAttackTime.Value + AttackCooldown;
    }
}
