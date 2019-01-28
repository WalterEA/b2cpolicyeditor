A WPF app for editing Azure AD B2C Identity Experience Framework Xml files.
Note: this application does NOT use the IEF Extension xml file at all - all definitions are included in either the Base file or individual journey policy files.

Functionality:
1. New... - loads IEF files from the Starter Pack. Allows user to define a prefix to be used in all policy names. A general options page, described in detail below is displayed.
2. Load... - loads policies local folder.
3. Save... - saved all policies to local folder updating policy names with the chosen profix and ensuring that base policy names are set correctly.
4. Add IdP - displays dialog to select IdP type and collects relevant client id/secret. Adds appropriate profile to Base policy and selected user journeys.
5. Add REST Api... - collects data re REST Api and creates an appropriate profile in Base.
6. Receipies menu has options to add policies to support additional functionality:
   a) userid based local users (existing email option is not removed).
   b) requirement to have user acknowldge Terms of Use conditions on signup and subsequentkly on signin IF the version of ToU has changed.
7. The Policy setup page (displayed when New.. policies are created or by selecting the Policy set setup in left-hand pane) allows you to create or retrieve IEF app and proxy ids to connect your policies to a specific B2C tenant. Make sure to log in with a admin account that is local to your B2C tenant (e.g. admin@xyz.onmicrosoft.com).

The navigation pane on the left-hand side allows further editing on objects it displays. In particular:
1. Claims - you can add new claim types (extension_ prefix will be added by the app)
2. IdPs/journeys, etc - you can change input/output/persisted claims lists for these objects
