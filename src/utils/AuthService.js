// eslint-disable-next-line
import { User, UserManager, WebStorageStateStore } from "oidc-client";
import { base_url, base_client } from "./constants";

class AuthService {
    userManager;

    constructor() {
        const AUTH0_DOMAIN = base_url; // e.g. https://jerrie.auth0.com
        const appUrl = `${window.location.protocol}//${window.location.hostname}${window.location.port ? `:${window.location.port}` : ''}`
        const settings = {
            userStore: new WebStorageStateStore({ store: window.localStorage }),
            authority: AUTH0_DOMAIN,
            client_id: base_client,
            redirect_uri: `${appUrl}/callback.html`,
            response_type: "id_token token",
            scope: "openid profile IdentityServerApi offline_access",
            post_logout_redirect_uri: appUrl,
            silent_redirect_uri: `${appUrl}/silent_renew.html`,
            filterProtocolClaims: true,
            metadata: {
                issuer: AUTH0_DOMAIN + "/",
                authorization_endpoint: AUTH0_DOMAIN + "/connect/authorize",
                userinfo_endpoint: AUTH0_DOMAIN + "/connect/userinfo",
                end_session_endpoint: AUTH0_DOMAIN + "/connect/endsession",
                jwks_uri: AUTH0_DOMAIN + "/.well-known/openid-configuration/jwks",
            }
        };

        this.userManager = new UserManager(settings);
    }

    getUser() {
        return this.userManager.getUser();
    }
    login() {
        return this.userManager.signinRedirect();
    }
    logout() {
        return this.userManager.signoutRedirect();
    }
}
export default new AuthService();
