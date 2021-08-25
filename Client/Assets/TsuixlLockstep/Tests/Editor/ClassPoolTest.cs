using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Network.Http;
using NUnit.Framework;
using Tsuixl.Net;


namespace Tests
{
    public class ClassPoolTest
    {
        [Test(Description = "ClassPool Parallel Test (并发测试)", Author = "cxl")]
        public void PoolTest()
        {
            ClassPool<PacketData>.Clear();
            
            var listArray = new List<PacketData>[3];
            for (int i = 0; i < 3; i++)
            {
                listArray[i] = new List<PacketData>(1000);
            }
            
            var actions = new Action[3];
            for (var i = 0; i < 3; i++)
            {
                var index = i;
                actions[i] = () =>
                {
                    for (var j = 0; j < 1000; j++)
                    {
                        listArray[index].Add(ClassPool<PacketData>.Get());
                    }
                };
            }
            
            Parallel.Invoke(actions);
            Assert.IsTrue(ClassPool<PacketData>.InactiveCount() == 0);
            
            for (var i = 0; i < 3; i++)
            {
                var index = i;
                actions[i] = () =>
                {
                    var array = listArray[index];
                    for (var j = 0; j < 1000; j++)
                    {
                        ClassPool<PacketData>.Recycle(array[j]);
                    }
                };
            }
            Parallel.Invoke(actions);
            Assert.IsTrue(ClassPool<PacketData>.InactiveCount() == 3000);
            
            ClassPool<PacketData>.Clear();
        }
    }
}
