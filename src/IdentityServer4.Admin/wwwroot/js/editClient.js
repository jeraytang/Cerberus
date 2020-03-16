$(function () {
    new Vue({
        el: '#app',
        data: {
            protocolTypes: [
                {id: 'oidc', text: 'OpenIdConnect'},
                {id: 'wsfed', text: 'WsFederation'},
                {id: 'saml2p', text: 'Saml2p'}
            ],
            trueOrFalse: [
                {id: 'True', text: 'True'},
                {id: 'False', text: 'False'}
            ],
            accessTokenTypes: [
                {id: 'Jwt', text: 'Jwt'},
                {id: 'Reference', text: 'Reference'},
            ],
            refreshTokenUsages: [
                {id: 'OneTimeOnly', text: 'OneTimeOnly'},
                {id: 'ReUse', text: 'ReUse'}
            ],
            refreshTokenExpirations: [
                {id: 'Sliding', text: 'Sliding'},
                {id: 'Absolute', text: 'Absolute'}
            ],
            grantTypes: [
                {id: 'code', text: 'Code'},
                {id: 'implicit', text: 'Implicit'},
                {id: 'hybrid', text: 'Hybrid'},
                {id: 'client_credentials', text: 'ClientCredentials'},
                {id: 'urn:ietf:params:oauth:grant-type:device_code', text: 'DeviceFlow'},
                {id: 'password', text: 'ResourceOwnerPassword'},
                {id: 'code client_credentials', text: 'CodeAndClientCredentials'},
                {id: 'hybrid client_credentials', text: 'HybridAndClientCredentials'},
                {id: 'implicit client_credentials', text: 'ImplicitAndClientCredentials'},
                {id: 'password client_credentials', text: 'ResourceOwnerPasswordAndClientCredentials'}
            ],
            client: {
                enabled: true,

                // client
                clientId: '',
                clientName: '',
                allowedGrantTypes: 'implicit',
                allowedScopes: 'openid profile',
                protocolType: 'oidc',
                allowAccessTokensViaBrowser: 'False',
                requireClientSecret: 'False',
                clientClaimsPrefix: 'client_',
                redirectUris: '',
                clientSecrets: '',
                postLogoutRedirectUris: '',
                clientUri: '',
                logoUri: '',
                description: '',

                // auth
                allowPlainTextPkce: 'False',
                allowOfflineAccess: 'False',
                requirePkce: 'False',
                claims: '',
                requireConsent: 'True',
                allowRememberConsent: 'True',
                consentLifetime: null,
                alwaysSendClientClaims: 'False',
                enableLocalLogin: 'False',
                backChannelLogoutUri: '',
                frontChannelLogoutSessionRequired: 'False',
                frontChannelLogoutUri: '',
                identityProviderRestrictions: '',
                userSsoLifetime: null,
                backChannelLogoutSessionRequired: 'False',
                properties: '',
                allowedCorsOrigins: '',

                // token
                pairWiseSubjectSalt: '',
                accessTokenType: 'Jwt',
                alwaysIncludeUserClaimsInIdToken: 'False',
                identityTokenLifetime: 300,
                accessTokenLifetime: 3600,
                refreshTokenExpiration: 'Absolute',
                includeJwtId: 'False',
                authorizationCodeLifetime: 300,
                absoluteRefreshTokenLifetime: 2592000,
                slidingRefreshTokenLifetime: 1296000,
                refreshTokenUsage: 'OneTimeOnly',
                updateAccessTokenClaimsOnRefresh: 'False',

                // device flow
                userCodeType: '',
                deviceCodeLifetime: 300,
            }
        },
        mounted: function () {
            let that = this;
            this.load();
        }, methods: {
            load: function () {
                let that = this;
                let clientId = this.getClientId();
                http.get(`/api/v1.0/clients/${clientId}`, function (result) {
                    that.client = result.data;
                });
            },
            getClientId: function () {
                let data = window.location.href.split('/');
                return data[data.length - 1];
            },
            save: function () {
                let clientId = this.getClientId();
                http.put(`/api/v1.0/clients/${clientId}`, this.client, function (result) {
                    if (result.data) {
                        window.location.href = '/clients';
                    } else {
                        swal('Error', 'Update client failed', "error");
                    }
                });
            }
        }
    });
});