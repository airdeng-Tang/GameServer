using Common.Battle;
using Common.Data;
using GameServer.Battle;
using GameServer.Core;
using GameServer.Managers;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Entities
{
    class Creature : Entity
    {

        public int Id
        {
            set;
            //get
            //{
            //    return this.entityId;
            //}
            get;
        }
        public string Name
        {
            get { return this.Info.Name; }
        }

        public NCharacterInfo Info;
        public CharacterDefine Define;

        public Attributes Attributes;
        public SkillManager SkillMgr;

        public BuffManager BuffMgr;

        /// <summary>
        /// buff效果管理器
        /// </summary>
        public EffectManager EffectMgr;

        public bool IsDeath = false;

        /// <summary>
        /// 怪物战斗状态
        /// </summary>
        public BattleState BattleState;

        /// <summary>
        /// 运动状态
        /// </summary>
        public CharacterState State;

        public Creature(Vector3Int pos, Vector3Int dir):base(pos,dir)
        {
            
        }

        public Creature(CharacterType type, int configId, int level, Vector3Int pos, Vector3Int dir) :
            base(pos, dir)
        {
            this.Define = DataManager.Instance.Characters[configId];

            this.Info = new NCharacterInfo();
            this.Info.Type = type;
            this.Info.Level = level;
            this.Info.ConfigId = configId;
            this.Info.Entity = this.EntityData;
            this.Info.EntityId = this.entityId;
            //this.Define = DataManager.Instance.Characters[this.Info.ConfigId];
            this.Info.Name = this.Define.Name;

            this.InitSkills();
            this.InitBuffs();

            this.Attributes = new Attributes();
            this.Attributes.Init(this.Define, this.Info.Level, this.GetEquips(), this.Info.attrDynamic);
            this.Info.attrDynamic = this.Attributes.DynamicAttr;
        }

        void InitSkills()
        {
            SkillMgr = new SkillManager(this);
            this.Info.Skills.AddRange(this.SkillMgr.Infos);
        }

        void InitBuffs()
        {
            BuffMgr = new BuffManager(this);
            EffectMgr = new EffectManager(this);
        }

        public virtual List<EquipDefine> GetEquips()
        {
            return null;
        }

        public void CastSkill(BattleContext context, int skillId)
        {
            Skill skill = this.SkillMgr.GetSkill(skillId);

            context.Result = skill.Cast(context);
            if(context.Result == SkillResult.Ok)
            {
                this.BattleState = BattleState.InBattle;
            }

            if(context.CastSkill == null)//为空时表示是怪物释放的技能
            {
                if(context.Result == SkillResult.Ok)
                {
                    context.CastSkill = new NSkillCastInfo()
                    {
                        casterId = this.entityId,
                        targetId = context.Target.entityId,
                        skillId = skill.Define.ID,
                        Position = new NVector3(),
                        Result = context.Result,
                    };
                    context.Battle.AddCastSkillInfo(context.CastSkill);
                }
            }
            else
            {
                context.CastSkill.Result = context.Result;
                context.Battle.AddCastSkillInfo(context.CastSkill);
            }
        }

        /// <summary>
        /// 角色受技能影响的效果
        /// </summary>
        /// <param name="damage">伤害数据</param>
        /// <param name="source">伤害来源</param>
        /// <exception cref="NotImplementedException"></exception>
        internal void DoDamage(NDamageInfo damage, Creature source)
        {
            this.BattleState = BattleState.InBattle;
            this.Attributes.HP -= damage.Damage;
            if(this.Attributes.HP < 0)
            {
                this.IsDeath = true;
                damage.WillDead = true;
            }

            this.OnDamage(damage, source);
        }

        public override void Update()
        {
            this.SkillMgr.Update();
            this.BuffMgr.Update();
        }

        /// <summary>
        /// 计算此对象与目标对象的距离
        /// </summary>
        /// <param name="target"> 目标对象 </param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal int Direction(Creature target)
        {
            return (int)Vector3Int.Distance(this.Position, target.Position);
        }

        /// <summary>
        /// 计算此对象与目标坐标的距离
        /// </summary>
        /// <param name="position"> 目标坐标 </param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal int Direction(Vector3Int position)
        {
            return (int)Vector3Int.Distance(this.Position, position);
        }

        internal void AddBuff(BattleContext context, BuffDefine buffDefine)
        {
            this.BuffMgr.AddBuff(context, buffDefine);
        }

        /// <summary>
        /// 造成伤害的来源
        /// </summary>
        /// <param name="damage">伤害信息</param>
        /// <param name="source">伤害来源</param>
        protected virtual void OnDamage(NDamageInfo damage, Creature source)
        {
            
        }

    }
}
