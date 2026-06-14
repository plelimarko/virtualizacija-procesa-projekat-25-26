using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface IEcgService
    {
        [OperationContract(IsInitiating = true)]
        [FaultContract(typeof(ValidationFault))]
        void StartSession(SessionMeta meta);

        // Dodajemo FaultContract-e kako bi klijent mogao da uhvati specifične greške
        [OperationContract(IsInitiating = false)]
        [FaultContract(typeof(DataFormatFault))]
        [FaultContract(typeof(ValidationFault))]
        void PushSample(EcgSample sample);

        [OperationContract(IsInitiating = false, IsTerminating = true)]
        void EndSession();
    }
}