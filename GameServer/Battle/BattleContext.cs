using GameServer.Core;
using GameServer.Entities;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Battle
{
    /// <summary>
    /// 战斗上下文 (战斗信息详情)
    /// </summary>
    class BattleContext
    {
        public Battle Battle;
        public Creature Caster;
        public Creature Target;
        public Vector3Int Position;

        public NSkillCastInfo CastSkill;

        /// <summary>
        /// 技能造成的伤害 (或生命值恢复)
        /// </summary>
        public NDamageInfo Damage;

        public SkillResult Result;

        public BattleContext(Battle battle)
        {
            this.Battle = battle;
        }
    }
}
