using Common;
using Common.Data;
using GameServer.Entities;
using GameServer.Managers;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Battle
{
    class Skill
    {
        public NSkillInfo Info;
        public Creature Owner;
        public SkillDefine Define;

        public SkillStatus Status = SkillStatus.None;

        private float cd = 0;

        public float CD
        {
            get { return cd; }
        }

        /// <summary>
        /// 施法时间
        /// </summary>
        private float castingTime = 0;

        /// <summary>
        /// 释放时间 
        /// </summary>
        private float skillTime = 0;
        private int Hit = 0;
        BattleContext Context;

        /// <summary>
        /// 是否为瞬发技能
        /// </summary>
        public bool Instant {
            get {
                if(this.Define.CastTime > 0)
                {
                    return false;
                }
                if (this.Define.Bullet)
                {
                    return false;
                }
                if(this.Define.Duration > 0)
                {
                    return false;
                }
                if(this.Define.HitTimes != null && this.Define.HitTimes.Count > 0)
                {
                    return false;
                }
                return true;
            }
        }

        public Skill(NSkillInfo info, Creature owner)
        {
            Info=info;
            Owner=owner;
            //Define=DataManager.Instance.Skills[(int)this.Owner.Define.Class][this.Info.Id];
            Define=DataManager.Instance.Skills[(int)this.Owner.Define.TID][this.Info.Id];

        }

        /// <summary>
        /// 检查此技能能否释放
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public SkillResult CanCast(BattleContext context)
        {
            if(this.Status != SkillStatus.None)
            {
                return SkillResult.Casting;//技能释放中
            }
            if(this.Define.CastTarget == Common.Battle.TargetType.Target)
            {
                if(context.Target == null || context.Target == this.Owner) //目标为空 || 目标为自己
                {
                    return SkillResult.InvalidTarget;
                }

                int distance = this.Owner.Direction(context.Target);
                if(distance > this.Define.CastRange)
                {
                    return SkillResult.OutOfRange;
                }
            }

            if(this.Define.CastTarget == Common.Battle.TargetType.Position)
            {
                if(context.CastSkill.Position == null)
                {
                    return SkillResult.InvalidTarget;
                }
                if(this.Owner.Direction(context.Position) > this.Define.CastRange)
                {
                    return SkillResult.OutOfRange;
                }
            }

            if(this.Owner.Attributes.MP < this.Define.MPCost)
            {
                return SkillResult.OutOfMp;
            }

            if(this.cd > 0)
            {
                return SkillResult.CoolDown;
            }

            return SkillResult.Ok;
        }

        internal SkillResult Cast(BattleContext context)
        {
            SkillResult result = this.CanCast(context);
            if(result == SkillResult.Ok)
            {
                //if (context.Target != null)
                //{
                //    this.DoSkillDamage(context);
                //}
                //this.cd = this.Define.CD;
                this.castingTime = 0;
                this.skillTime = 0;
                this.cd = this.Define.CD;
                this.Hit = 0;
                this.Context = context;


                if (this.Instant)
                {
                    this.DoHit();
                }
                else
                {
                    if(this.Define.CastTime > 0)
                    {
                        this.Status = SkillStatus.Casting;
                    }
                    else
                    {
                        this.Status = SkillStatus.Running;
                    }
                }
            }

            //else
            //{
            //    result =  SkillResult.InvalidTarget;
            //}
            Log.InfoFormat("Skill[{0}].Cast Result:[{1}] Status:{2} || Ower :: {3}", 
                this.Define.Name, result, this.Status, this.Owner.Name);
            return result;
        }

        private void DoHit()
        {
            this.Hit++;
            Log.InfoFormat("Skill[{0}].DoHit[{1}] :: 技能名 & 造成伤害次数", this.Define.Name, this.Hit);
        }


        /// <summary>
        /// 执行技能逻辑, 实现技能效果
        /// </summary>
        /// <param name="context"></param>
        private void DoSkillDamage(BattleContext context)
        {
            context.Damage = new NDamageInfo();
            context.Damage.entityId = context.Target.entityId;
            context.Damage.Damage = 100;
            context.Target.DoDamage(context.Damage);
        }

        public void Update()
        {
            UpdateCD();
            if(this.Status == SkillStatus.Running)
            {
                this.UpdateSkill();
            }
            else if (this.Status == SkillStatus.Casting)
            {
                this.UpdateCasting();
            }
        }

        /// <summary>
        /// 施法中
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void UpdateCasting()
        {
            if(this.castingTime < this.Define.CastTime)
            {
                this.castingTime += Time.deltaTime;
            }
            else
            {
                this.castingTime = 0;
                this.Status = SkillStatus.Running;
                Log.InfoFormat("Skill[{0}].UpdateCasting Finish :: 施法完毕准备释放技能", this.Define.Name);
            }
        }

        /// <summary>
        /// 释放中
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void UpdateSkill()
        {
            this.skillTime += Time.deltaTime;
            if(this.Define.Duration > 0)
            {//持续技能 (间隔时间计算次数)
                if(this.skillTime > this.Define.Interval * (this.Hit + 1))
                {
                    this.DoHit();
                }

                if(this.skillTime >= this.Define.Duration)
                {
                    this.Status = SkillStatus.None;
                    Log.InfoFormat("Skill[{0}].UpdateSkill Finish :: 技能释放完毕", this.Define.Name);
                }
            }
            else if(this.Define.HitTimes != null && this.Define.HitTimes.Count > 0)
            {//多段伤害 (固定伤害次数与间隔时间)
                if(this.Hit < this.Define.HitTimes.Count)
                {
                    if(this.skillTime > this.Define.HitTimes[this.Hit])
                    {
                        this.DoHit();
                    }
                }
                else
                {
                    this.Status = SkillStatus.None;
                    Log.InfoFormat("Skill[{0}].UpdateSkill Finish :: 技能释放完毕", this.Define.Name);
                }
            }
        }

        private void UpdateCD()
        {
            if(this.cd > 0)
            {
                this.cd -= Time.deltaTime;
            }
            if(cd < 0)
            {
                this.cd = 0;
            }
        }
    }
}
