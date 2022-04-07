﻿using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Managers
{
    class MapManager : Singleton<CharacterManager>
    {
        Dictionary<int, Map> Maps = new Dictionary<int, Map>();

        public void Init()
        {
            foreach(var mapdefine in DataManager.Instance.Maps.Values)
            {
                Map map = new Map(mapdefine);
                Log.InfoFormat("MapManager.Init => Map:{0} : {1}",map.Define.Id, map.Define.Name);
                this.Maps[mapdefine.ID] = map;
            }
        }

        public Map this[int key]
        {
            get
            {
                return this.Maps[key];
            }
        }

        public void Update()
        {
            foreach(var map in this.Maps.Values)
            {
                map.Update();
            }
        }
    }
}
