using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace slotsi_citas.Services
{
    public static class MongoDbSettings
    {

        public const string ConnectionString = "mongodb://Solare3e4:Clamore1Q2W3E@ac-lp14nfq-shard-00-00.wxaz72n.mongodb.net:27017,ac-lp14nfq-shard-00-01.wxaz72n.mongodb.net:27017,ac-lp14nfq-shard-00-02.wxaz72n.mongodb.net:27017/?ssl=true&replicaSet=atlas-rmoj6p-shard-0&authSource=admin&retryWrites=true&w=majority&appName=slotsi-cluster";

        public const string DatabaseName = "SlotsiCitasDB";
    }
}
