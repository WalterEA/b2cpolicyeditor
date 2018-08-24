using DataToolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B2CPolicyEditor.ViewModels
{
    public class NewJourneyTypeWizard: ObservableObject
    {
        public NewJourneyTypeWizard()
        {
            TechnicalProfiles = new List<SignInSource>()
            {
                new SignInSource() { IsSelected = true, TPId = "" }
            };
        }
        public IEnumerable<SignInSource> TechnicalProfiles { get; private set; } 
    }

    public class SignInSource: ObservableObject
    {
        public bool IsLocal
        {
            get { return _IsLocal; }
            set
            {
                Set(ref _IsLocal, value);
            }
        }
        private bool _IsLocal;
        public string TPId
        {
            get { return _TPId; }
            set
            {
                Set(ref _TPId, value);
            }
        }
        private string _TPId;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                Set(ref _IsSelected, value);
            }
        }
        private bool _IsSelected;

    }
}
