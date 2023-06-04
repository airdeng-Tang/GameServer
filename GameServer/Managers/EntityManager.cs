using Common;
using GameServer.Core;
using GameServer.Entities;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Managers
{
    class EntityManager : Singleton<EntityManager>
    {
        private int idx = 0;
        //public List<Entity> AllEntities = new List<Entity>();

        public Dictionary<int, Entity> AllEntities = new Dictionary<int, Entity>();

        /// <summary>
        /// 地图字典  int为地图编号
        /// </summary>
        public Dictionary<int, List<Entity>> MapEntities = new Dictionary<int, List<Entity>>();

        public void AddEntity(int mapId , Entity entity)
        {
            //AllEntities.Add(entity);

            if(entity.EntityData.Id == 0)
            {
                //加入管理器生成唯一Id  (其他类调用时的entityId)
                entity.EntityData.Id = ++this.idx;
            }

            AllEntities.Add(entity.EntityData.Id, entity);

            List<Entity> entities = null;

            if(!MapEntities.TryGetValue(mapId, out entities))
            {
                entities = new List<Entity>();
                MapEntities[mapId] = entities;
            }
            entities.Add(entity);
        }
        public void RemoveEntity(int mapId, Entity entity)
        {
            this.AllEntities.Remove(entity.entityId);
            this.MapEntities[mapId].Remove(entity);
        }

        public Entity GetEntity(int entityId)
        {
            Entity result = null;
            this.AllEntities.TryGetValue(entityId, out result);
            return result;
        }

        public Creature GetCreature(int entityId)
        {
            return GetEntity(entityId) as Creature; 
        }

        /// <summary>
        /// 查找地图中某一类别的对象 ( 玩家 Character , 怪物 Monster 等)
        /// </summary>
        /// <typeparam name="T"> 继承自Creature类的对象 </typeparam>
        /// <param name="mapId"> 地图ID </param>
        /// <param name="match"> 可自定义的条件函数(返回值为bool类型) </param>
        /// <returns></returns>
        public List<T> GetMapEntities<T>(int mapId, Predicate<Entity> match) where T : Creature
        {
            List<T> result = new List<T>();
            foreach(var entity in this.MapEntities[mapId])
            {
                if(entity is T && match.Invoke(entity))
                {
                    result.Add((T)entity);
                }
            }
            return result;
        }

        /// <summary>
        /// 查询地图中进入aoe技能释放范围的对象(调用EntityManager.GetMapEntities方法)
        /// </summary>
        /// <typeparam name="T"> 继承自Creature类的对象 </typeparam>
        /// <param name="mapId"> 地图ID </param>
        /// <param name="pos"> aoe释放中心的坐标 </param>
        /// <param name="range"> aoe范围半径 </param>
        /// <returns></returns>
        public List<T> GetMapEntitiesInRange<T>(int mapId, Vector3Int pos, int range) where T : Creature
        {
            return this.GetMapEntities<T>(mapId, (entity) =>
            {
                T creature = entity as T;
                return creature.Direction(pos) < range;
            });
        }
    }
}
