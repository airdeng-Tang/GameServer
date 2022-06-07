using Common;
using Common.Data;
using GameServer.Entities;
using GameServer.Services;
using Network;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Managers
{
    class EquipManager : Singleton<EquipManager>
    {
        public Result EquipItem(NetConnection<NetSession> sender, int slot, int itemId, bool isEquip)
        {
            Character character = sender.Session.Character;
            if (!character.ItemManager.Items.ContainsKey(itemId))//查看角色是否拥有这件装备
            {
                return Result.Failed;
            }
            UpdateEquip(character.Data.Equips, slot, itemId, isEquip);

            DBService.Instance.Save();

            return Result.Success;
        }

        unsafe void UpdateEquip(byte[] equipData, int slot, int itemId, bool isEquip)
        {
            fixed(byte* pt = equipData)
            {
                //int i = 1;
                //int* b = (int*)i;
                //Log.InfoFormat("======================i为{0} ;  *b为{1}", i, *b);
                int* slotid = (int*)(pt + slot * sizeof(int));
                if (isEquip)
                {
                    *slotid = itemId;
                }
                else
                {
                    *slotid = 0;
                }
            }
        }
    }
}
