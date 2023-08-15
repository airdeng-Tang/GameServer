using Common.Battle;
using GameServer.Battle;
using GameServer.Entities;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.AI
{
    /// <summary>
    /// AI基础
    /// </summary>
    class AIBase
    {
        private Monster owner;


        /// <summary>
        /// 怪物仇恨目标
        /// </summary>
        public Creature Target;

        /// <summary>
        /// 普通攻击 
        /// </summary>
        Skill normalSkill;

        public AIBase(Monster owner)
        {
            this.owner = owner;
            normalSkill = this.owner.SkillMgr.NormalSkill;
        }

        internal void Update()
        {
            if (this.owner.BattleState == Common.Battle.BattleState.InBattle)
            {
                this.UpdateBattle();
            }
        }

        /// <summary>
        /// 战斗帧
        /// </summary>
        private void UpdateBattle()
        {
            if (this.Target == null)
            {
                this.owner.BattleState = Common.Battle.BattleState.InBattle;
                return;
            }

            if (!TryCastSkill())
            {
                if (!TryCastNormal())
                {
                    FollowTarget();
                }
            }
        }

        /// <summary>
        /// 追随目标
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void FollowTarget()
        {
            int distance = this.owner.Direction(this.Target);
            if(distance > normalSkill.Define.CastRange - 50) //50作为优化参数以保证不能刚好等于普攻距离
            {
                this.owner.MovTo(this.Target.Position);
            }
            else
            {
                this.owner.StopMove();
            }
        }

        /// <summary>
        /// 尝试普攻
        /// </summary>
        /// <returns></returns>
        private bool TryCastNormal()
        {
            if (this.Target != null)
            {
                BattleContext context = new BattleContext(this.owner.map.Battle)
                {
                    Target = this.Target,
                    Caster = this.owner,
                };
                var result = this.normalSkill.CanCast(context);
                if (result == SkillResult.Ok)
                {
                    this.owner.CastSkill(context, normalSkill.Define.ID);
                }
                else if(result == SkillResult.OutOfRange)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 尝试技能
        /// </summary>
        /// <returns></returns>
        private bool TryCastSkill()
        {
            if(this.Target != null)
            {
                BattleContext context = new BattleContext(this.owner.map.Battle)
                {
                    Target = this.Target,
                    Caster = this.owner,
                };
                Skill skill = this.owner.FindSkill(context, SkillType.Skill);
                if (skill != null)
                {
                    this.owner.CastSkill(context, skill.Define.ID);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 受到伤害
        /// </summary>
        /// <param name="damage">伤害信息</param>
        /// <param name="source">伤害来源</param>
        internal void OnDamage(NDamageInfo damage, Creature source)
        {
            this.Target = source;
        }
    }
}
