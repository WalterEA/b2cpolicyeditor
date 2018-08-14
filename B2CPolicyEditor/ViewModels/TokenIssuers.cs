using DataToolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace B2CPolicyEditor.ViewModels
{
    public class TokenIssuers: ObservableObject
    {
        public TokenIssuers()
        {

        }
        private List<XElement> _issuers;

        public List<XElement> Issuers
        {
            get
            {
                return _issuers;
            }
        }
    }
}
