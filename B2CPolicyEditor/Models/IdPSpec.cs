using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace B2CPolicyEditor.Models
{
    public class IdPSpec
    {
        public enum Sources { Local, AAD, OIDC };
        public Sources Source { get; set; }
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                ProfileId = $"{_name}Profile";
            }
        }

        public string FederationNameInIdP { get; set; }
        public Uri LoginUrl { get; set; }
        public bool IsMultiTenant { get; set; } // for AAD IdP only
        public string AADAppId { get; set; }
        public string Domain { get; set; }
        public string ProfileId { get; set; }
        public string TenantId { get; set; }
    }
}
