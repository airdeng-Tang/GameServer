using Common.Battle;
using GameServer.AI;
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
        /// 怪物ai
        /// </summary>
        AIAgent AI;

        /// <summary>
        /// 所在地图
        /// </summary>
        public Map map;

        /// <summary>
        /// 进行移动的目的地坐标
        /// </summary>
        private Vector3Int moveTarget;

        /// <summary>
        /// 当前移动到的位置
        /// </summary>
        Vector3 movePosition;

        public Monster(int tid, int level, Vector3Int pos, Vector3Int dir) :
            base(CharacterType.Monster, tid, level, pos, dir)
        {
            //this.Info.attrDynamic = new NAttributeDynamic();
            //this.Info.attrDynamic.Hp = (int)DataManager.Instance.Characters[tid].MaxHP;
            //this.Info.attrDynamic.Mp = (int)DataManager.Instance.Characters[tid].MaxMP;
            this.AI = new AIAgent(this);

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
            if(this.AI != null)
            {
                this.AI.OnDameage(damage,source);
            }
        }

        public override void Update()
        {
            base.Update();
            this.AI.Update();
            this.UpdateMovement();
        }

        /// <summary>
        /// 查找可被释放的技能
        /// </summary>
        /// <returns></returns>
        public Skill FindSkill(BattleContext context, SkillType type)
        {
            Skill cancast = null;
            foreach(var skill in this.SkillMgr.Skills)
            {
                if((skill.Define.Type & type) != skill.Define.Type) // & 符号为位运算符
                {
                    continue;
                }
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

        /// <summary>
        /// 朝向目标移动
        /// </summary>
        /// <param name="position">目标坐标</param>
        internal void MovTo(Vector3Int position)
        {
            if(State == CharacterState.Idle)
            {
                State = CharacterState.Move;
            }
            if(this.moveTarget != position)
            {
                this.moveTarget = position;
                this.movePosition = Position;   

                var dist = (this.moveTarget - this.Position);

                this.direction = dist.normalized;

                this.Speed = this.Define.Speed;


                NEntitySync sync = new NEntitySync();
                sync.Entity = this.EntityData;
                sync.Event = EntityEvent.MoveFwd;
                sync.Id = this.entityId;

                this.map.UpdateEntity(sync);
            }
        }

        /// <summary>
        /// 更新移动位置
        /// </summary>
        private void UpdateMovement()
        {
            if(State == CharacterState.Move)
            {
                if(this.Direction(this.moveTarget) < 50)
                {
                    this.StopMove();
                }
            }

            if(this.Speed > 0)
            {
                Vector3 dir = this.direction;
                this.movePosition += dir * Speed * Time.deltaTime / 100f;
                this.Position = this.movePosition;
            }
        }

        /// <summary>
        /// 停止移动
        /// </summary>
        internal void StopMove()
        {
            this.State = CharacterState.Idle;
            this.moveTarget = Vector3Int.zero;
            this.Speed = 0;

            NEntitySync sync = new NEntitySync();
            sync.Entity = this.EntityData;
            sync.Event = EntityEvent.Idle;
            sync.Id = this.entityId;

            this.map.UpdateEntity(sync) ;
        }
    }
}
