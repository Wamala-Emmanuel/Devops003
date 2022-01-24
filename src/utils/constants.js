import axios from "axios";
import {releaseNumber} from '../../package.json';


const evVar = process.env.NODE_ENV || 'dev'
export const environment = evVar.trim()

console.log("Environment: ", environment);

const env = {
    Auth: process.env.VUE_APP_Auth,
    clientId: process.env.VUE_APP_clientId
}

export const base_url = env.Auth

export const base_client = env.clientId

export const uiVersion = releaseNumber

export const endpoints = {
    user_details: '/api/users',
    client_details: '/api/clients',
    apiresource_details: '/api/apiresources',
    identityresource_details: '/api/identityresources'
}

export const user_labels = {
    id: 'ID',
    name: 'Full Name',
    email: 'Email',
    user: 'Name',
    phoneNumber: 'Contact',
    participant: 'Participant',
    role: 'Role',
    lockedOut: 'Status',
}

export const client_labels = {
    id: 'Client ID',
    client_id: 'Client Name',
    grant_type: 'Grant Types',
    claims: 'Claims',
    scopes: 'Scopes',
    redirect_uri: 'Redirect URI',
    post_logout_redirect_uri: 'Post Logout Redirect URI',
    client: 'Client'
}

export const resource_labels = {
    id: 'ID',
    name: 'Name',
    displayName: 'Display Name',
    description: 'Description',
    enabled: 'Enabled',
    actions: 'Actions',
    resource: 'resource'
}

export const ACCESS_TOKEN = "ACCESS_TOKEN"
export const CALLBACK_PATH = "CALLBACK_PATH"
export const request = axios.create({
    baseURL: base_url,
    timeout: 10000,
    headers: {
        'Authorization': `Bearer ${localStorage.getItem(ACCESS_TOKEN)}`
    }
})

export const api_resource_model = {
    "enabled": true,
    "name": "string",
    "displayName": "string",
    "description": "string",
    "secrets": [{
        "description": "string",
        "value": "string",
        "expiration": "2019-06-19T08:12:06.508Z",
        "type": "string",
        "created": "2019-06-19T08:12:06.508Z"
    }],
    "scopes": [{
        "name": "string",
        "displayName": "string",
        "description": "string",
        "required": true,
        "emphasize": true,
        "showInDiscoveryDocument": true,
        "userClaims": [{
            "type": "string"
        }],
    }],
    "userClaims": [{
        "type": "string"
    }],
    "properties": [{
        "key": "string",
        "value": "string"
    }],
    "nonEditable": true
}

export const identity_resource_model = {
    "enabled": true,
    "name": "string",
    "displayName": "string",
    "description": "string",
    "required": true,
    "emphasize": true,
    "showInDiscoveryDocument": true,
    "userClaims": [{
        "type": "string"
    }],
    "properties": [{
        "key": "string",
        "value": "string"
    }],
    "nonEditable": true
}

// Client model
export const api_client_model = {
    "enabled": true,
    "clientId": "emata-auth",
    "protocolType": "oidc",
    "clientSecrets": null,
    "requireClientSecret": false,
    "clientName": "Emata Auth Identity",
    "description": "Emata Auth Identity",
    "clientUri": null,
    "logoUri": null,
    "requireConsent": false,
    "allowRememberConsent": true,
    "alwaysIncludeUserClaimsInIdToken": true,
    "allowedGrantTypes": null,
    "requirePkce": true,
    "allowPlainTextPkce": false,
    "allowAccessTokensViaBrowser": true,
    "redirectUris": null,
    "postLogoutRedirectUris": null,
    "frontChannelLogoutUri": null,
    "frontChannelLogoutSessionRequired": true,
    "backChannelLogoutUri": null,
    "backChannelLogoutSessionRequired": true,
    "allowOfflineAccess": true,
    "allowedScopes": null,
    "identityTokenLifetime": 3600,
    "accessTokenLifetime": 3600,
    "authorizationCodeLifetime": 3600,
    "consentLifetime": null,
    "absoluteRefreshTokenLifetime": 2592000,
    "slidingRefreshTokenLifetime": 1296000,
    "refreshTokenUsage": 1,
    "updateAccessTokenClaimsOnRefresh": true,
    "refreshTokenExpiration": 1,
    "accessTokenType": 0,
    "enableLocalLogin": true,
    "identityProviderRestrictions": null,
    "includeJwtId": false,
    "claims": null,
    "alwaysSendClientClaims": false,
    "clientClaimsPrefix": null,
    "pairWiseSubjectSalt": null,
    "allowedCorsOrigins": null,
    "properties": null
}
