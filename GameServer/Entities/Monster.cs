using GameServer.Battle;
using GameServer.Core;
using GameServer.Managers;
using GameServer.Models;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Entities
{
    class Monster : Creature
    {
        /// <summary>
        /// 怪物仇恨目标
        /// </summary>
        Creature Target;

        /// <summary>
        /// 所在地图
        /// </summary>
        Map map;

        public Monster(int tid, int level, Vector3Int pos, Vector3Int dir) :
            base(CharacterType.Monster, tid, level, pos, dir)
        {
            //this.Info.attrDynamic = new NAttributeDynamic();
            //this.Info.attrDynamic.Hp = (int)DataManager.Instance.Characters[tid].MaxHP;
            //this.Info.attrDynamic.Mp = (int)DataManager.Instance.Characters[tid].MaxMP;


        }

        /// <summary>
        /// 进入地图
        /// </summary>
        /// <param name="map"></param>
        public void OnEnterMap(Map map)
        {
            this.map = map;
        }

        /// <summary>
        /// 受到伤害
        /// </summary>
        /// <param name="damage">伤害信息</param>
        /// <param name="source">伤害来源</param>
        protected override void OnDamage(NDamageInfo damage, Creature source)
        {
            if(this.Target == null)
            {
                this.Target = source;
            }
        }

        public override void Update()
        {
            if(this.State == Common.Battle.CharState.InBattle)
            {
                this.UpdateBattle();
            }
            base.Update();
        }

        /// <summary>
        /// 战斗帧
        /// </summary>
        private void UpdateBattle()
        {
            if(this.Target != null)
            {
                BattleContext context = new BattleContext(this.map.Battle)
                {
                    Target = this.Target,
                    Caster = this,

                };
                Skill skill = this.FindSkill(context);
                if (skill != null)
                {
                    this.CastSkill(context, skill.Define.ID);
                }
            }
        }

        /// <summary>
        /// 查找可被释放的技能
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private Skill FindSkill(BattleContext context)
        {
            Skill cancast = null;
            foreach(var skill in this.SkillMgr.Skills)
            {
                var result = skill.CanCast(context);
                if(result == SkillResult.Casting)//当任意技能处于释放中时退出该方法
                {
                    return null;
                }
                if(result == SkillResult.Ok)
                {
                    cancast = skill;
                }
            }

            return cancast;
        }
    }
}
