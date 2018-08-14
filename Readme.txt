1. Works only with Base and journey xmls - does not use Extensions (all changes done in Base)
2. All policies must be in a single folder - single base and any number of journey policies.
3. To configure IEF/Proxy apps needs admin login to B2C tenant
4. To configure AAD as IdP needs admin login to the AAD tenant
5. Scenarios tested (at some stage):
   1. Download and configure one of existing Starter Pack journeys to use a B2C tenant
   2. Add/change claims
   3. Add/change claims for a TP in ClaimsProvider or RelyingParty
   4. Add and configure social, AAD or SAML IdPs