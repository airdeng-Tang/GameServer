using Common.Data;
using GameServer.Core;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Entities
{
    class Character : CharacterBase
    {
       
        public TCharacter Data;

        //public int Id//一定注意!!!自己加  使Map.cs创建添加角色时调用的Character.Id不为0,否则创建角色时因为id冲突只能创建一个
        //{
        //    get { return this.Info.Id; }
        //}

        public Character(CharacterType type,TCharacter cha):
            base(new Core.Vector3Int(cha.MapPosX, cha.MapPosY, cha.MapPosZ),new Core.Vector3Int(100,0,0))//初始化角色位置和面对的方向
        {
            this.Data = cha;
            this.Info = new NCharacterInfo();
            this.Info.Type = type;
            this.Info.Id = cha.ID;
            this.Info.Name = cha.Name;
            this.Info.Level = 1;//cha.Level;
            this.Info.Tid = cha.TID;
            this.Info.Class = (CharacterClass)cha.Class;
            this.Info.mapId = cha.MapID;
            this.Info.Entity = this.EntityData;
            //this.Define = DataManager.Instance.Characters[this.Info.Tid];
        }
    }
}
