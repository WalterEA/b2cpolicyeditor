using B2CPolicyEditor.Models;
using DataToolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;

namespace B2CPolicyEditor.ViewModels
{
    public class TOUSettings: ObservableObject
    {
        public TOUSettings(XDocument policy)
        {
            try
            {
                CurrVersionId = App.PolicySet.Base.Root
                    .Element(Constants.dflt + "BuildingBlocks")
                        .Element(Constants.dflt + "ClaimsTransformations")
                            .Elements(Constants.dflt + "ClaimsTransformation")
                                .First(t => t.Attribute("Id").Value == "UpdateTOUVersion")
                                    .Element(Constants.dflt + "InputParameters")
                                        .Element(Constants.dflt + "InputParameter")
                                            .Attribute("Value").Value;
            }
            catch
            {
                MainWindow.Trace.Add(new TraceItem() { Msg = "No existing version id found." });
            }
            OnOK = new DelegateCommand(() =>
            {
                if (!String.IsNullOrEmpty(NewVersionId) && String.Compare(_CurrVersionId, NewVersionId) != 0)
                    Done(true);
            });
        }
        private string _CurrVersionId;

        public string CurrVersionId
        {
            get { return _CurrVersionId; }
            private set { _CurrVersionId = value; }
        }
        private string _NewVersionId;

        public string NewVersionId
        {
            get { return _NewVersionId; }
            set { _NewVersionId = value; }
        }
        public ICommand OnOK { get; set; }

        public event Action<bool> Done;
    }
}
