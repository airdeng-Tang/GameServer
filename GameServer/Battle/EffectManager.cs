using Common;
using Common.Battle;
using GameServer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Battle
{
    internal class EffectManager
    {
        private Creature Owner;

        /// <summary>
        /// [buff效果类型, 层数]
        /// </summary>
        Dictionary<BuffEffect, int> Effects = new Dictionary<BuffEffect, int>();

        public EffectManager(Creature creature)
        {
            this.Owner = creature;
        }

        /// <summary>
        /// 判断效果管理器中是否存在效果 effect
        /// </summary>
        /// <param name="effect"> buff效果 </param>
        /// <returns></returns>
        public bool HasEffect(BuffEffect effect)
        {
            if(this.Effects.TryGetValue(effect, out int val))
            {
                return val > 0;
            }
            return false;
        }


        /// <summary>
        /// 增加buff效果
        /// </summary>
        /// <param name="effect">BuffEffect buff效果</param>
        internal void AddEffect(BuffEffect effect)
        {
            Log.InfoFormat($"EffectManager.AddEffect :: 对象名称[{this.Owner.Name}].AddEffect(增加效果){effect}");
            if (!this.Effects.ContainsKey(effect))
            {
                this.Effects[effect] = 1;
            }
            else
            {
                this.Effects[effect]++;
            }
        }

        /// <summary>
        /// 减少buff效果
        /// </summary>
        /// <param name="effect">BuffEffect buff效果</param>
        internal void RemoveEffect(BuffEffect effect)
        {
            Log.InfoFormat($"EffectManager.RemoveEffect :: 对象名称[{this.Owner.Name}].RemoveEffect(减少效果){effect}");
            if (this.Effects[effect] > 0)
            {
                this.Effects[effect]--;
            }
        }
    }
}
