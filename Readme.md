# Purpose

A WPF app for manipulating Azure AD B2C Identity Experience Framework Xml files.

# Contributing

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

# Changes

02/08/2019 - Fixed 'change to use user id'

# Supported scenarios

## Create from Starter Pack

Downloads selected IEF starter pack (from https://github.com/Azure-Samples/active-directory-b2c-custom-policy-starterpack/) and provisions it to use a B2C tenant.
User needs to sign in with local admin credentials (e.g. admin@myb2c.onmicrosoft.com). If needed, the app adds the B2C App, Proxy App
will create the new apps required by B2C, add their ids to the policies. It will also add the id of the B2C-extensions app
needed to create and maintain custom claims.

## Save/load policies

Saves policies loaded into the app to a disk folder or loads them from the folder. Noe that the app does not use the
IEF Extension policy format - it only handles the Base file and individual rRelying Party policies.

## Add custom claims

User can create and define new claim types and add them to existing input/output claim lists.

## Add IdP

Provides UI to define a new OIDC IdP and optionally add it to selected user journeys.

## Add REST API

Provides UI to configure a new REST API Technical Profile

## Convert to use local user with user id

Adds technical profiles and modifies user journeys to use user id instead of email. Existing (email) profiles are not removes.

## Add Terms of Use acceptance

Adds claims, user journey steps and modifies existing profiles to require user to accept specified version of
Terms of Use. If the policy designer changes the version, the modified user journey will require existing users to re-accept the terms.

# Notes

The application does NOT use the IEF Extension xml file at all - all definitions are included in either the Base file or individual journey policy files
