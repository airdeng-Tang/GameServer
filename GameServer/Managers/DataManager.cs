using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using System.IO;

using Common;
using Common.Data;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GameServer.Models;

namespace GameServer.Managers
{
    public class DataManager : Singleton<DataManager>
    {
        internal string DataPath;
        internal Dictionary<int, MapDefine> Maps = null;
        internal Dictionary<int, CharacterDefine> Characters = null;
        internal Dictionary<int, TeleporterDefine> Teleporters = null;
        public Dictionary<int, NpcDefine> Npcs = null;
        public Dictionary<int, ItemDefine> Itesms = null;

        public Dictionary<int, Dictionary<int, SpawnPointDefine>> SpawnPoints = null;
        public Dictionary<int, Dictionary<int, SpawnRuleDefine>> SpawnRules = null;

        public Dictionary<int, ShopDefine> Shops = null;
        public Dictionary<int, Dictionary<int, ShopItemDefine>> ShopItems = null;
        public Dictionary<int, QuestDefine> Quests = null;

        public Dictionary<int, BuffDefine> Buffs = null;
        public Dictionary<int, Dictionary<int, SkillDefine>> Skills = null;

        public DataManager()
        {
            this.DataPath = "Data/";
            Log.Info("DataManager > DataManager()");
        }

        internal void Load()
        {
            string json = File.ReadAllText(this.DataPath + "MapDefine.txt");
            this.Maps = JsonConvert.DeserializeObject<Dictionary<int, MapDefine>>(json);

            json = File.ReadAllText(this.DataPath + "TeleporterDefine.txt");
            this.Teleporters = JsonConvert.DeserializeObject<Dictionary<int, TeleporterDefine>>(json);

            json = File.ReadAllText(this.DataPath + "CharacterDefine.txt");
            this.Characters = JsonConvert.DeserializeObject<Dictionary<int, CharacterDefine>>(json);

            json = File.ReadAllText(this.DataPath + "NpcDefine.txt");
            this.Npcs = JsonConvert.DeserializeObject<Dictionary<int, NpcDefine>>(json);
            
            json = File.ReadAllText(this.DataPath + "ItemDefine.txt");
            this.Itesms = JsonConvert.DeserializeObject<Dictionary<int, ItemDefine>>(json);

            json = File.ReadAllText(this.DataPath + "SpawnPointDefine.txt");
            this.SpawnPoints = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<int, SpawnPointDefine>>>(json);

            json = File.ReadAllText(this.DataPath + "SpawnRuleDefine.txt");
            this.SpawnRules = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<int, SpawnRuleDefine>>>(json);

            json = File.ReadAllText(this.DataPath + "ShopDefine.txt");
            this.Shops = JsonConvert.DeserializeObject<Dictionary<int, ShopDefine>>(json);

            json = File.ReadAllText(this.DataPath + "ShopItemDefine.txt");
            this.ShopItems = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<int, ShopItemDefine>>>(json);

            json = File.ReadAllText(this.DataPath + "QuestDefine.txt");
            this.Quests = JsonConvert.DeserializeObject<Dictionary<int, QuestDefine>>(json);

            json = File.ReadAllText(this.DataPath + "BuffDefine.txt");
            this.Buffs = JsonConvert.DeserializeObject<Dictionary<int, BuffDefine>>(json);

            json = File.ReadAllText(this.DataPath + "SkillDefine.txt");
            this.Skills = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<int, SkillDefine>>>(json);
            
            Console.WriteLine("Console.WriteLine :: 技能名: {0} 间隔时间: {1} 持续时间: {2} MP消耗 : {3}",
                Skills[1][103].Name, Skills[1][103].Interval, Skills[1][103].Duration, Skills[1][103].MPCost);
            Console.WriteLine("Console.WriteLine :: 技能名: {0} 间隔时间: {1} 持续时间: {2} MP消耗 : {3}",
                Skills[2][203].Name, Skills[2][203].Interval, Skills[2][203].Duration, Skills[2][203].MPCost);
            //Console.WriteLine("Console.WriteLine :: 技能名: {0} 间隔时间: {1} 持续时间: {2} MP消耗 : {3} Fuck : {4}",
            //    Skills[1][103].Name, Skills[1][103].Interval, Skills[1][103].Duration, Skills[1][103].MPCost , Skills[1][103].fuck);
            //Console.WriteLine("Console.WriteLine :: 技能名: {0} 间隔时间: {1} 持续时间: {2} MP消耗 : {3} Fuck : {4}",
            //    Skills[2][203].Name, Skills[2][203].Interval, Skills[2][203].Duration, Skills[2][203].MPCost , Skills[2][203].fuck);
        }
    }
}