using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;

namespace MultiplayerARPG.OpsiveBT
{
    public class MonsterDoAction : MonsterActionNode
    {
        private bool didAction;

        public override void OnStart()
        {
            didAction = false;
        }

        public override TaskStatus OnUpdate()
        {
            if (didAction && Entity.IsPlayingActionAnimation())
            {
                ClearActionState();
                return TaskStatus.Success;
            }

            IDamageableEntity tempTargetEnemy;
            if (!Entity.TryGetTargetEntity(out tempTargetEnemy) || Entity.Characteristic == MonsterCharacteristic.NoHarm)
            {
                // No target, stop attacking
                ClearActionState();
                return TaskStatus.Failure;
            }

            if (tempTargetEnemy.Entity == Entity.Entity || tempTargetEnemy.IsHideOrDead() || !tempTargetEnemy.CanReceiveDamageFrom(Entity.GetInfo()))
            {
                // If target is dead or in safe area stop attacking
                Entity.SetTargetEntity(null);
                ClearActionState();
                return TaskStatus.Failure;
            }

            bool tempIsLeftHand = isLeftHandAttacking.Value;
            Entity.AimPosition = Entity.GetAttackAimPosition(ref tempIsLeftHand);
            isLeftHandAttacking.Value = tempIsLeftHand;
            if (Entity.IsPlayingActionAnimation())
                return TaskStatus.Running;

            if (queueSkill.Value != null && Entity.IndexOfSkillUsage(queueSkill.Value.DataId, SkillUsageType.Skill) < 0)
            {
                // Use skill when there is queue skill or randomed skill that can be used
                Entity.UseSkill(queueSkill.Value.DataId, false, 0, new AimPosition()
                {
                    type = AimPositionType.Position,
                    position = tempTargetEnemy.OpponentAimTransform.position,
                });
            }
            else
            {
                // Attack when no queue skill
                Entity.Attack(false);
            }

            didAction = true;
            return TaskStatus.Running;
        }
    }
}
