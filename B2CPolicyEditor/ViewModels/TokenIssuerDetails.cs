using B2CPolicyEditor.Models;
using DataToolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using B2CPolicyEditor.Extensions;

namespace B2CPolicyEditor.ViewModels
{
    public class TokenIssuerDetails: ObservableObject
    {
        public TokenIssuerDetails()
        {

        }
        private XElement _metadata;
        private XElement _source;
        public XElement Source
        {
            get { return _source; }
            set
            {
                _source = value;
                _metadata = Source.Element(Constants.dflt + "Metadata");
            }
        }

        public string AccessTokenLifetime
        {
            get => _metadata.GetMetadataValue("token_lifetime_secs");
            set => _metadata.SetMetadataValue("token_lifetime_secs", value);
        }
        public string IDTokenLifetime
        {
            get => _metadata.GetMetadataValue("id_token_lifetime_secs");
            set => _metadata.SetMetadataValue("id_token_lifetime_secs", value);
        }
        public string RefreshTokenLifetime
        {
            get => _metadata.GetMetadataValue("refresh_token_lifetime_secs");
            set => _metadata.SetMetadataValue("refresh_token_lifetime_secs", value);
        }
        public string RefreshTokenRollingLifetime
        {
            get => _metadata.GetMetadataValue("rolling_refresh_token_lifetime_secs");
            set => _metadata.SetMetadataValue("rolling_refresh_token_lifetime_secs", value);
        }
    }
}
