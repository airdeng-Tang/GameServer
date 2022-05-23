using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Common;

namespace GameServer.Services
{
    class DBService : Singleton<DBService>
    {
        ExtremeWorldEntities entities;

        public ExtremeWorldEntities Entities
        {
            get { return this.entities; }
        }

        public void Init()
        {
            entities = new ExtremeWorldEntities();
        }

        float time = 0;
        public void Save()
        {
            //DateTime.Now.Ticks - time > xxxx    //相隔xxxx秒执行一次
            entities.SaveChangesAsync();//异步保存
        }
    }
}
