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

        public bool IsDeath = false;

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

            this.Attributes = new Attributes();
            this.Attributes.Init(this.Define, this.Info.Level, this.GetEquips(), this.Info.attrDynamic);
            this.Info.attrDynamic = this.Attributes.DynamicAttr;
        }

        void InitSkills()
        {
            SkillMgr = new SkillManager(this);
            this.Info.Skills.AddRange(this.SkillMgr.Infos);
        }

        public virtual List<EquipDefine> GetEquips()
        {
            return null;
        }

        public void CastSkill(BattleContext context, int skillId)
        {
            Skill skill = this.SkillMgr.GetSkill(skillId);

            context.Result = skill.Cast(context);
        }
        
        /// <summary>
        /// 角色受技能影响的效果
        /// </summary>
        /// <param name="damage"></param>
        /// <exception cref="NotImplementedException"></exception>
        internal void DoDamage(NDamageInfo damage)
        {
            this.Attributes.HP -= damage.Damage;
            if(this.Attributes.HP < 0)
            {
                this.IsDeath = true;
                damage.WillDead = true;
            }
        }

        public override void Update()
        {
            this.SkillMgr.Update();
        }
    }
}
