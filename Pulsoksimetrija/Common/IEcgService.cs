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
        // Klijent šalje meta podatke i otvara sesiju
        [OperationContract(IsInitiating = true)]
        void StartSession(SessionMeta meta);

        // Klijent šalje blok od N redova spakovan u MemoryStream
        [OperationContract(IsInitiating = false)]
        void PushBatch(Stream batchStream);

        // Klijent javlja da je kraj
        [OperationContract(IsInitiating = false, IsTerminating = true)]
        void EndSession();
    }
}