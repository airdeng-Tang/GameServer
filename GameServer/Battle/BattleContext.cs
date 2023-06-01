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
        /// <summary>
        /// 技能
        /// </summary>
        public Battle Battle;
        /// <summary>
        /// 施法者
        /// </summary>
        public Creature Caster;
        /// <summary>
        /// 施法目标
        /// </summary>
        public Creature Target;
        /// <summary>
        /// 施法坐标
        /// </summary>
        public Vector3Int Position
        {
            get { return this.CastSkill.Position; }
        }

        public NSkillCastInfo CastSkill;

        public SkillResult Result;

        public BattleContext(Battle battle)
        {
            this.Battle = battle;
        }
    }
}
