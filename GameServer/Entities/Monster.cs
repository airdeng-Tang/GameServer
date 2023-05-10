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
    class Monster : Creature
    {
        public Monster(int tid, int level, Vector3Int pos, Vector3Int dir) : base(CharacterType.Monster, tid, level, pos, dir)
        {
            //this.Info.attrDynamic = new NAttributeDynamic();
            //this.Info.attrDynamic.Hp = (int)DataManager.Instance.Characters[tid].MaxHP;
            //this.Info.attrDynamic.Mp = (int)DataManager.Instance.Characters[tid].MaxMP;
        }
    }
}
