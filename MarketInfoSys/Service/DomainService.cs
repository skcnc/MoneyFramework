using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.IO;
using System.Xml;
using System.ServiceModel.Channels;

namespace MarketInfoSys.Service
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的类名“DomainService”。
    //public class DomainService : IDomainService
    //{
    //    #region IDomainService 成员

    //    public System.ServiceModel.Channels.Message ProvidePolicyFile()
    //    {
    //        FileStream filestream = File.Open(@"clientaccesspolicy.xml", FileMode.Open);
    //        // Either specify ClientAcessPolicy.xml file path properly
    //        // or put that in /Bin folder of the console application
    //        XmlReader reader = XmlReader.Create(filestream);
    //        System.ServiceModel.Channels.Message result = Message.CreateMessage(MessageVersion.None, "", reader);
    //        return result;
    //    }

    //    //CrossDomainServiceBehavior

    //    #endregion
    //}
}
