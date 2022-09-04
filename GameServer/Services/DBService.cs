using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Common;
using Common.Utils;

namespace GameServer.Services
{
    class DBService : Singleton<DBService>
    {
        ExtremeWorldEntities entities;

        float timestamp = 0;

        public ExtremeWorldEntities Entities
        {
            get { return this.entities; }
        }

        public void Init()
        {
            entities = new ExtremeWorldEntities();
            //this.timestamp = TimeUtil.timestamp;
        }

        public void Save()
        {
            //DateTime.Now.Ticks - time > xxxx    //相隔xxxx秒执行一次
            entities.SaveChangesAsync();//异步保存
        }

        ///// <summary>
        ///// 错误方法*****
        ///// </summary>
        //public void Save()
        //{
        //    //DateTime.Now.Ticks - time > xxxx    //相隔xxxx秒执行一次
        //    if(DateTime.Now.Ticks - timestamp > 100)
        //    {
        //        entities.SaveChangesAsync();//异步保存
        //    }
        //}

        //public async void Save()
        //{
        //    //DateTime.Now.Ticks - time > xxxx    //相隔xxxx秒执行一次
        //    try
        //    {
        //        await entities.SaveChangesAsync();//异步保存
        //    }
        //    catch (DbEntityValidationException ex)
        //    {
        //        Log.ErrorFormat("DbEntityValidationException");
        //    }

        //    //Task.Delay(100).Wait();
        //}
    }
}
