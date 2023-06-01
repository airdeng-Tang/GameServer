using Common;
using Common.Battle;
using Common.Data;
using Common.Utils;
using GameServer.Core;
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
        //NSkillHitInfo HitInfo;

        List<Bullet> Bullets = new List<Bullet>();

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
                this.Bullets.Clear();

                this.AddBuff(TriggerType.SkillCast);

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

        /// <summary>
        /// 开始命中处理
        /// </summary>
        NSkillHitInfo InitHitInfo(bool isBullet)
        {
            NSkillHitInfo HitInfo = new NSkillHitInfo();
            HitInfo.casterId = this.Context.Caster.entityId;
            HitInfo.skillId = this.Info.Id;
            HitInfo.hitId = this.Hit;
            HitInfo.isBullet = isBullet;
            //Context.Battle.AddHitInfo(HitInfo);
            return HitInfo;
        }

        private void DoHit()
        {
            NSkillHitInfo HitInfo = InitHitInfo(false);
            Log.InfoFormat("Skill[{0}].DoHit[{1}] :: 技能名 & 造成伤害次数", this.Define.Name, this.Hit);

            this.Hit++;

            if (this.Define.Bullet)
            {
                CastBullet(HitInfo);
                return;
            }

            DoHit(HitInfo);
        }

        public void DoHit(NSkillHitInfo HitInfo) 
        {
            Context.Battle.AddHitInfo(HitInfo);
            Log.InfoFormat($"Skill[{this.Define.Name}].DoHit[{HitInfo.hitId}]  IsBullet:{HitInfo.isBullet}");


            if(this.Define.AOERange > 0)
            {
                this.HitRange(HitInfo);
                return;
            }

            if(this.Define.CastTarget == Common.Battle.TargetType.Target)
            {
                this.HitTarget(Context.Target,HitInfo);
            }
        }



        /// <summary>
        /// 生成子弹
        /// </summary>
        /// <param name="HitInfo"> 技能信息 </param>
        void CastBullet(NSkillHitInfo HitInfo)
        {
            Context.Battle.AddHitInfo(HitInfo);
            //Log.InfoFormat($"Skill[{this.Define.Name}].DoHit[{HitInfo.hitId}]  IsBullet:{HitInfo.isBullet}");
            Log.InfoFormat("Skill[{0}].CastBullet[{1}]  Target : {2}",
                this.Define.Name, this.Define.BulletResource,this.Context.Target.Name);

            Bullet bullet = new Bullet(this, this.Context.Target, HitInfo);
            this.Bullets.Add(bullet);
        }

        /// <summary>
        /// 范围伤害 AOE
        /// </summary>
        void HitRange(NSkillHitInfo HitInfo)
        {
            Vector3Int pos;
            if(this.Define.CastTarget == Common.Battle.TargetType.Target)
            {
                pos = Context.Target.Position;  //以目标对象为中心的aoe  (冰拳特效)
            }
            else if(this.Define.CastTarget == Common.Battle.TargetType.Position)
            {
                pos = Context.Position;  //以坐标作为技能中心  (常规aoe 暴风雪 烈焰风暴等)
            }
            else
            {
                pos = this.Owner.Position; //以自身为中心的aoe (大风车等)
            }

            List<Creature> units = this.Context.Battle.FindUnitsInMapRange(pos, this.Define.AOERange);
            foreach(var target in units)
            {
                MapManager.Instance[this.Owner.Info.mapId].Battle.JoinBattle(target);
                this.HitTarget(target, HitInfo);
            }
        }

        /// <summary>
        /// 选中目标释放技能
        /// </summary>
        /// <param name="target"></param>
        void HitTarget(Creature target, NSkillHitInfo HitInfo)
        {
            if(this.Define.CastTarget == Common.Battle.TargetType.Self && (target != Context.Caster))
            {
                return;
            }
            else if(target == Context.Caster)
            {
                return;
            }
            
            NDamageInfo damage = this.CalcSkillDamage(Context.Caster, target);
            Log.InfoFormat("Skill[{0}].HitTarget[{1}] 伤害Damage:{2} 暴击Crit: {3}",
                this.Define.Name, target.Name, damage.Damage, damage.Crit);
            target.DoDamage(damage);
            HitInfo.Damages.Add(damage);

            this.AddBuff(TriggerType.SkillHit);
        }


        private void AddBuff(TriggerType trigger)
        {
            if (this.Define.Buff == null || this.Define.Buff.Count == 0)
            {
                return;
            }

            foreach(var buffId in this.Define.Buff)
            {
                var buffDefine = DataManager.Instance.Buffs[buffId];

                if(buffDefine.Trigger != trigger) //触发时机不一致
                {
                    continue;
                }

                if(buffDefine.Target == TargetType.Self)
                {
                    this.Owner.AddBuff(this.Context, buffDefine);
                }
                else if(buffDefine.Target == TargetType.Target)
                {
                    this.Context.Target.AddBuff(this.Context, buffDefine);
                }
            }
        }


        /// <summary>
        /// 伤害计算
        /// 战斗计算公式: 
        /// 物理伤害 = 物理攻击或技能基础伤害 * (1-物理防御 / 物理防御 +100))
        /// 魔法伤害 = 魔法攻击或技能基础伤害 * (1-魔法防御 / 魔法防御 +100))
        /// 暴击伤害 = 固定伤害 * 2
        /// 注 : 伤害值最小值为1 当伤害值小于1时取值1
        /// 注 : 最终伤害值在最终取舍时随机上下浮动 5%. 即: 伤害值 * 95% <= 最终伤害值 <= 伤害值 * 105%
        /// </summary>
        /// <param name="caster">施法者</param>
        /// <param name="target">施法目标</param>
        /// <returns></returns>
        NDamageInfo CalcSkillDamage(Creature caster, Creature target)
        {
            float ad = this.Define.AD + caster.Attributes.AD * this.Define.ADFactor;
            float ap = this.Define.AP + caster.Attributes.AP * this.Define.APFactor;

            float addmg = ad * (1 - target.Attributes.DEF / (target.Attributes.DEF + 100));
            float apdmg = ap * (1 - target.Attributes.MDEF / (target.Attributes.MDEF + 100));

            float final = addmg + apdmg;
            bool isCrit = IsCrit(caster.Attributes.CRI);
            if (isCrit)
            {
                final = final * 2f;//暴击伤害 * 2
            }

            //随即浮动
            final = final + final * ((float)MathUtil.Random.NextDouble() * 0.1f - 0.05f);

            NDamageInfo damage = new NDamageInfo();
            damage.entityId = target.entityId;
            damage.Damage = Math.Max(1, (int)final);
            damage.Crit = isCrit;
            return damage;
        }

        /// <summary>
        /// 是否暴击
        /// </summary>
        /// <param name="cRI">暴击概率</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private bool IsCrit(float crit)
        {
            return MathUtil.Random.NextDouble() < crit;
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
        /// 释放技能中
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
                    if (!this.Define.Bullet)
                    {
                        this.Status = SkillStatus.None;
                        Log.InfoFormat("Skill[{0}].UpdateSkill Finish :: 技能释放完毕", this.Define.Name);
                    }
                }
            }

            if (this.Define.Bullet)
            {
                bool finish = true;
                foreach(Bullet bullet in this.Bullets)
                {
                    bullet.Update();
                    if(!bullet.Stoped)
                    {
                        finish = false;
                    }
                }

                if(finish && this.Hit >= this.Define.HitTimes.Count)
                {
                    this.Status = SkillStatus.None;
                    Log.InfoFormat($"Skill[{this.Define.Name}].UpdateSkill Finish");
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
