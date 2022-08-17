using Common;
using Common.Data;
using GameServer.Core;
using GameServer.Managers;
using GameServer.Models;
using Network;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Entities
{
    class Character : CharacterBase, IPostResponser
    {
       
        public TCharacter Data;

        public ItemManager ItemManager;
        public QuestManager QuestManager;
        public StatusManager StatusManager;
        public FriendManager FriendManager;

        //public int Id//一定注意!!!自己加  使Map.cs创建添加角色时调用的Character.Id不为0,否则创建角色时因为id冲突只能创建一个
        //{
        //    get { return this.Info.Id; }
        //}

        public Team Team;
        public double TeamUpdateTS;//时间戳

        public Guild Guild;
        public double GuildUpdateTS;

        public Character(CharacterType type,TCharacter cha):
            base(new Core.Vector3Int(cha.MapPosX, cha.MapPosY, cha.MapPosZ),new Core.Vector3Int(100,0,0))//初始化角色位置和面对的方向
        {
            this.Data = cha;
            this.Id = cha.ID;
            this.Info = new NCharacterInfo();
            this.Info.Type = type;
            this.Info.Id = cha.ID;
            this.Info.EntityId = this.entityId;
            this.Info.Name = cha.Name;
            this.Info.Level = 10;//cha.Level;
            this.Info.ConfigId = cha.TID;
            this.Info.Class = (CharacterClass)cha.Class;
            this.Info.mapId = cha.MapID;
            this.Info.Gold = cha.Gold;
            this.Info.Entity = this.EntityData;
            this.Define = DataManager.Instance.Characters[this.Info.ConfigId];

            this.ItemManager = new ItemManager(this);
            this.ItemManager.GetItemInfos(this.Info.Items);

            this.Info.Bag = new NBagInfo();
            this.Info.Bag.Unlocked = this.Data.Bag.Unlocked;
            this.Info.Bag.Items = this.Data.Bag.Items;

            this.Info.Equips = this.Data.Equips;

            this.QuestManager = new QuestManager(this);
            this.QuestManager.GetQuestInfos(this.Info.Quests);

            this.StatusManager = new StatusManager(this);

            this.FriendManager = new FriendManager(this);
            this.FriendManager.GetFriendInfos(this.Info.Friends);
            //Log.InfoFormat(this.FriendManager.)

            if(this.Data.GuildId != null)
            {
                this.Guild = GuildManager.Instance.GetGuild((int)this.Data.GuildId);
            }

        }

        public long Gold
        {
            get { return this.Data.Gold; }
            set
            {
                if (this.Data.Gold == value)
                {
                    return;
                }
                this.StatusManager.AddGoldChange((int)(value - this.Data.Gold));
                this.Data.Gold = value;
            }
        }

        public void PostProcess(NetMessageResponse message)
        {
            Log.InfoFormat("PostProcess > Character : characterID :{0}:{1}", this.Id, this.Info.Name);
            this.FriendManager.PostProcess(message,this.FriendManager.get());

            if(this.Team != null)
            {
                int i =this.Id;
                Log.InfoFormat("PostProcess > Team: characterID : {0} : {1}   {2}<{3}", this.Id, this.Info.Name, TeamUpdateTS, this.Team.timestamp);
                //Team t = TeamManager.Instance.GetTeamTimestamp(this);
                //if (this.Team.timestamp < t.timestamp)
                //{
                //    this.Team = t;
                //}

                if (TeamUpdateTS < this.Team.timestamp)
                {
                    TeamUpdateTS = this.Team.timestamp;
                    this.Team.PostProcess(message);
                }
                
            }

            if(this.Guild != null)
            {
                Log.InfoFormat("PostProcess > Guild: characterID:{0} : {1}   {2} < {3} ",this.Id,this.Info.Name,GuildUpdateTS, this.Guild.timestamp);
                if(this.Info.Guild == null)
                {
                    this.Info.Guild = this.Guild.GuildInfo(this);
                    if(message.mapCharacterEnter != null)
                    {
                        GuildUpdateTS = Guild.timestamp;
                    }
                }
                if(GuildUpdateTS < this.Guild.timestamp && message.mapCharacterEnter == null)
                {
                    GuildUpdateTS = Guild.timestamp;
                    this.Guild.PostProcess(this, message);
                }
            }

            if (this.StatusManager.HasStatus)
            {
                this.StatusManager.PostProcess(message);
            }
        }

        /// <summary>
        /// 角色离开时调用
        /// </summary>
        public void Clear()
        {
            this.FriendManager.OfflineNotify();

            if(this.Team != null)
            {
                TeamManager.Instance.LeaveTeamByCharacterInList(this);
            }
        }


        public NCharacterInfo GetBasicInfo()
        {
            return new NCharacterInfo()
            {
                Id = this.Id,
                Name = this.Info.Name,
                Class = this.Info.Class,
                Level = this.Info.Level,
            };
        }

    }
}
