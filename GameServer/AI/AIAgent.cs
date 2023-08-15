using Common.Data;
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
    /// ai代理
    /// </summary>
    class AIAgent
    {

        public Monster Monster;

        /// <summary>
        /// ai基础类
        /// </summary>
        public AIBase ai;

        public AIAgent(Monster monster)
        {
            this.Monster = monster;

            string aiName = monster.Define.AI;
            if (string.IsNullOrEmpty(aiName))
            {
                aiName = AIMonsterPassive.ID;
            }

            switch(aiName)
            {
                case AIMonsterPassive.ID:
                    this.ai = new AIMonsterPassive(monster);
                    break;
                case AIBoss.ID:
                    this.ai = new AIBoss(monster);
                    break;
            }
        }

        internal void Update()
        {
            if(this.ai != null)
            {
                this.ai.Update();
            }
        }

        /// <summary>
        /// 受到伤害
        /// </summary>
        /// <param name="damage">伤害信息</param>
        /// <param name="source">伤害来源</param>
        internal void OnDameage(NDamageInfo damage, Creature source)
        {
            if(this.ai != null)
            {
                this.ai.OnDamage(damage, source);
            }
        }

    }
}
