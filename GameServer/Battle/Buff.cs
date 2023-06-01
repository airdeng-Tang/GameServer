using Common;
using Common.Data;
using Common.Utils;
using GameServer.Entities;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Battle
{
    internal class Buff
    {
        public int BuffID;
        private Creature owner;
        private BuffDefine Define;
        private BattleContext Context;

        private float time;
        private int hit;

        public Buff(int buffID, Creature owner, BuffDefine define, BattleContext context)
        {
            this.BuffID = buffID;
            this.owner = owner;
            this.Define = define;
            this.Context = context;

            this.OnAdd();
        }

        public bool Stoped { get; internal set; }

        internal void Update()
        {
            if(Stoped)
            {
                return;
            }

            this.time += Time.deltaTime;

            if(this.Define.Interval > 0) //带有间隔时间
            {
                if(this.time > this.Define.Interval * (this.hit + 1))
                {
                    this.DoBuffDamage();
                }
            }

            if(time > this.Define.Duration)
            {
                this.OnRemove();
            }
        }

        /// <summary>
        /// 执行buff的dot伤害, 将buff伤害传递给Creature
        /// </summary>
        private void DoBuffDamage()
        {
            this.hit++;

            NDamageInfo damage = this.CalcBuffDamage(Context.Caster);
            Log.InfoFormat(
                $"Buff[{this.Define.Name}].DoBuffDamage[{this.owner.Name}] ::" +
                $" (Buff伤害)Damage:{damage.Damage} (是否暴击)Crit:{damage.Crit}");
            this.owner.DoDamage(damage);

            NBuffInfo buff = new NBuffInfo()
            {
                buffId = this.BuffID,
                buffType = this.Define.ID,
                casterId = this.Context.Caster.entityId,
                ownerId = this.owner.entityId,
                Action = BuffAction.Hit,
                Damage = damage,
            };
            Context.Battle.AddBuffAction(buff);
        }

        /// <summary>
        /// 计算buff伤害
        /// </summary>
        /// <param name="caster"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private NDamageInfo CalcBuffDamage(Creature caster)
        {
            float ad = this.Define.AD + caster.Attributes.AD * this.Define.ADFator;
            float ap = this.Define.AP + caster.Attributes.AP * this.Define.APFator;

            float addmg = ad * (1 - this.owner.Attributes.DEF / (this.owner.Attributes.DEF + 100));
            float apdmg = ap * (1 - this.owner.Attributes.MDEF / (this.owner.Attributes.MDEF + 100));

            float final = addmg + apdmg;

            NDamageInfo damage = new NDamageInfo();
            damage.entityId = this.owner.entityId;
            damage.Damage = Math.Max(1, (int)final);
            return damage;
        }

        /// <summary>
        /// 添加buff效果
        /// </summary>
        private void OnAdd()
        {
            if(this.Define.Effect != Common.Battle.BuffEffect.None)
            {
                this.owner.EffectMgr.AddEffect(this.Define.Effect);
            }

            AddAttr();

            NBuffInfo buff = new NBuffInfo()
            {
                buffId = this.BuffID,
                buffType = this.Define.ID,
                casterId = this.Context.Caster.entityId,
                ownerId = this.owner.entityId,
                Action = BuffAction.Add
            };
            Context.Battle.AddBuffAction(buff);
        }

        /// <summary>
        /// 增加属性增益
        /// </summary>
        private void AddAttr()
        {
            if(this.Define.DEFRatio != 0)
            {
                this.owner.Attributes.Buff.DEF += this.owner.Attributes.Basic.DEF * this.Define.DEFRatio;
                this.owner.Attributes.InitFinalAttributes();
            }
        }

        /// <summary>
        /// 删除buff
        /// </summary>
        private void OnRemove()
        {
            RemoveAttr();
            Stoped = true;
            this.hit = 0;
            if (this.Define.Effect != Common.Battle.BuffEffect.None)
            {
                this.owner.EffectMgr.RemoveEffect(this.Define.Effect);
            }

            NBuffInfo buff = new NBuffInfo()
            {
                buffId = this.BuffID,
                buffType = this.Define.ID,
                casterId = this.Context.Caster.entityId,
                ownerId = this.owner.entityId,
                Action = BuffAction.Remove
            };
            Context.Battle.AddBuffAction(buff);
        }

        /// <summary>
        /// 移除属性增益
        /// </summary>
        private void RemoveAttr()
        {
            if (this.Define.DEFRatio != 0)
            {
                this.owner.Attributes.Buff.DEF -= this.owner.Attributes.Basic.DEF * this.Define.DEFRatio;
                this.owner.Attributes.InitFinalAttributes();
            }
        }
    }
}
