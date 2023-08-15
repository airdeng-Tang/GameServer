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
    class SkillManager
    {
        Creature Owner;

        public List<Skill> Skills { get; private set; }
        public List<NSkillInfo> Infos { get; private set; }

        /// <summary>
        /// 普通攻击
        /// </summary>
        public Skill NormalSkill { get; internal set; }

        public SkillManager(Creature owner)
        {
            this.Owner = owner;
            this.Skills = new List<Skill>();
            this.Infos = new List<NSkillInfo>();
            this.InitSkills();
        }

        void InitSkills()
        {
            this.Skills.Clear();
            this.Infos.Clear();

            //在数据库读取当前技能信息  作为扩展作业



            if(!DataManager.Instance.Skills.ContainsKey(this.Owner.Define.TID))
            {
                return;
            }

            foreach(var define in DataManager.Instance.Skills[this.Owner.Define.TID])
            {
                NSkillInfo info = new NSkillInfo();
                info.Id = define.Key;
                if(this.Owner.Info.Level >= define.Value.UnlockLevel)
                {
                    info.Level = 5;
                }
                else
                {
                    info.Level = 1;
                }
                this.Infos.Add(info);
                Skill skill = new Skill(info, this.Owner);
                if(define.Value.Type == Common.Battle.SkillType.Normal)
                {
                    this.NormalSkill = skill;
                }
                this.AddSkill(skill);
            }
        }

        public void AddSkill(Skill skill)
        {
            this.Skills.Add(skill);
        }

        public Skill GetSkill(int skillId)
        {
            for(int i = 0; i<this.Skills.Count; i++)
            {
                if (this.Skills[i].Define.ID == skillId)
                {
                    return this.Skills[i];
                }
            }
            return null;
        }

        public void Update()
        {
            for (int i = 0; i<this.Skills.Count; i++)
            {
                this.Skills[i].Update();
            }
        }
    }
}
