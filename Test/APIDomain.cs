using LJC.FrameWork.HttpApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test
{
    public class APIDomain
    {
        /// <summary>
        /// 获取人类信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [APIMethod]
        public GetPersonResponse GetPersion(GetPersonRequest request)
        {
            return new GetPersonResponse
            {

            };
        }
    }
}
