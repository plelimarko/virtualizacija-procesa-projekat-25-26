using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.IO;

namespace Common
{
    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface IEcgService
    {
        [OperationContract(IsInitiating = true)]
        void StartSession(SessionMeta meta);

        [OperationContract(IsInitiating = false)]
        void PushSample(EcgSample sample);

        [OperationContract(IsInitiating = false, IsTerminating = true)]
        void EndSession();
    }
}