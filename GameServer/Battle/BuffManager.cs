using Common.Data;
using GameServer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Battle
{
    internal class BuffManager
    {
        private Creature Owner;
        /// <summary>
        /// 对象的buff列表
        /// </summary>
        private List<Buff> Buffs = new List<Buff>();

        /// <summary>
        /// buff索引
        /// </summary>
        private int idx = 1;

        private int BuffID
        {
            get {
                return this.idx++;
            }
        }

        public BuffManager(Creature creature)
        {
            this.Owner = creature;
        }

        internal void AddBuff(BattleContext context, BuffDefine define)
        {
            Buff buff = new Buff(this.BuffID, this.Owner, define, context);
            this.Buffs.Add(buff);
        }

        internal void Update()
        {
            foreach (var buff in this.Buffs)
            {
                if (!buff.Stoped)
                {
                    buff.Update();
                }
            }

            this.Buffs.RemoveAll((b) => b.Stoped);
        }
    }
}
