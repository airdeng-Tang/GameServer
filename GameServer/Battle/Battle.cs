using GameServer.Core;
using GameServer.Entities;
using GameServer.Managers;
using GameServer.Models;
using Network;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Battle
{
    class Battle
    {
        public Map Map;

        /// <summary>
        /// 所有参加战斗的单位
        /// </summary>
        Dictionary<int, Creature> AllUnits = new Dictionary<int, Creature>();

        Queue<NSkillCastInfo> Actions = new Queue<NSkillCastInfo>();

        List<NSkillHitInfo> Hits = new List<NSkillHitInfo>();   

        /// <summary>
        /// 已死亡的单位
        /// </summary>
        List<Creature> DeahPool = new List<Creature>(); 

        public Battle(Map map)
        {
            this.Map = map;
        }

        /// <summary>
        /// 战斗信息处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="request"></param>
        internal void ProcessBattleMessage(NetConnection<NetSession> sender, SkillCastRequest request)
        {
            Character character = sender.Session.Character;

            if(request.castInfo != null)
            {
                //判断是否为当前玩家释放的技能, 即释放技能的角色id和网络信息中的玩家id是否一致
                if (character.entityId != request.castInfo.casterId)
                {
                    return;
                }
                this.Actions.Enqueue(request.castInfo);
            }
        }

        internal void Update()
        {
            this.Hits.Clear();
            if(this.Actions.Count > 0)
            {
                NSkillCastInfo skillCast = this.Actions.Dequeue();
                this.ExecuteAction(skillCast);
            }
             
            this.UpdateUnits();

            this.BroadcastHitsMessage();
        }

        /// <summary>
        /// 角色加入战斗
        /// </summary>
        /// <param name="unit"></param>
        public void JoinBattle(Creature unit)
        {
            this.AllUnits[unit.entityId] = unit;
        }

        public void LeaveBattle(Creature unit)
        {
            this.AllUnits.Remove(unit.entityId);
        }

        /// <summary>
        /// 广播受击消息
        /// </summary>
        /// <param name="cast"></param>
        void ExecuteAction(NSkillCastInfo cast)
        {
            BattleContext context = new BattleContext(this);
            context.Caster = EntityManager.Instance.GetCreature(cast.casterId);
            context.Target = EntityManager.Instance.GetCreature(cast.targetId);
            context.CastSkill = cast;
            if(context.Caster != null)
            {
                this.JoinBattle(context.Caster);
            }
            if(context.Target != null)
            {
                this.JoinBattle(context.Target);
            }

            context.Caster.CastSkill(context, cast.skillId);

            NetMessageResponse message = new NetMessageResponse();
            message.skillCast = new SkillCastResponse();
            message.skillCast.castInfo = context.CastSkill;
            message.skillCast.Damage = context.Damage;
            message.skillCast.Result = context.Result == SkillResult.Ok ? Result.Success : Result.Failed;
            message.skillCast.Errormsg = context.Result.ToString();
            this.Map.BroadcastBattleResponse(message);
        }

        /// <summary>
        /// 创建伤害信息网络响应并用地图类广播
        /// </summary>
        private void BroadcastHitsMessage()
        {
            if(this.Hits.Count == 0)
            {
                return;
            }
            NetMessageResponse message = new NetMessageResponse();
            message.skillHits = new SkillHitResponse();
            message.skillHits.Hits.AddRange(this.Hits);
            message.skillHits.Result = Result.Success;
            message.skillHits.Errormsg = "";
            this.Map.BroadcastBattleResponse(message);
        }


        void UpdateUnits()
        {
            this.DeahPool.Clear();
            foreach(var kv in this.AllUnits)//集合类不能在遍历是做增删操作
            {
                kv.Value.Update();
                if (kv.Value.IsDeath)
                {
                    this.DeahPool.Add(kv.Value);
                }
            }

            foreach(var unit in this.DeahPool)
            {
                this.LeaveBattle(unit);
            }
        }

        internal List<Creature> FindUnitsInRange(Vector3Int pos, int range)
        {
            List<Creature> result = new List<Creature>();
            foreach(var unit in this.AllUnits)
            {
                if(unit.Value.Direction(pos) < range)
                {
                    result.Add(unit.Value);
                }
            }
            return result;
        }

        //internal void AddHitInfo(NSkillHitInfo hitInfo)
        //{
        //    throw new NotImplementedException();
        //}

        public void AddHitInfo(NSkillHitInfo hit)
        {
            this.Hits.Add(hit);
        }
    }
}
