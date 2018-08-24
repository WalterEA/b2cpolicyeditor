using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace B2CPolicyEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Models.PolicySet PolicySet = new Models.PolicySet();
        public static MRUData MRU;

        protected override void OnExit(ExitEventArgs e)
        {
            using (var str = File.CreateText("mru.json"))
            {
                str.Write(JsonConvert.SerializeObject(MRU));
            }
            base.OnExit(e);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                using (var str = File.OpenText("mru.json"))
                {
                    MRU = JsonConvert.DeserializeObject<MRUData>(str.ReadToEnd());
                }
            } catch(FileNotFoundException)
            {
                MRU = new MRUData();
            }
            base.OnStartup(e);
        }
    }

    public class MRUData
    {
        public string ProjectFolder { get; set; }
    }
}
