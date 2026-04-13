import Keycloak from 'keycloak-js';

const keycloakConfig = {
  url: import.meta.env.VITE_KEYCLOAK_URL || 'http://localhost:8081',
  realm: import.meta.env.VITE_KEYCLOAK_REALM || 'tweb-realm',
  clientId: import.meta.env.VITE_KEYCLOAK_CLIENT_ID || 'tweb-client',
};

const keycloak = new Keycloak(keycloakConfig);

export const initKeycloak = async (
  onAuthenticatedCallback: () => void
): Promise<boolean> => {
  try {
    const authenticated = await keycloak.init({
      onLoad: 'check-sso',
      pkceMethod: 'S256',
      silentCheckSsoRedirectUri: window.location.origin + '/silent-check-sso.html',
    });

    if (authenticated) {
      onAuthenticatedCallback();
    }

    // Token refresh
    keycloak.onTokenExpired = () => {
      keycloak
        .updateToken(30)
        .then((refreshed) => {
          if (refreshed) {
            console.log('Token refreshed');
          }
        })
        .catch(() => {
          console.error('Failed to refresh token');
        });
    };

    return authenticated;
  } catch (error) {
    console.error('Failed to initialize Keycloak:', error);
    return false;
  }
};

export const login = (): void => {
  keycloak.login({
    redirectUri: window.location.origin + '/products',
  });
};

export const logout = (): void => {
  keycloak.logout({
    redirectUri: window.location.origin,
  });
};

export const register = (): void => {
  keycloak.register({
    redirectUri: window.location.origin + '/products',
  });
};

export const getToken = (): string | undefined => {
  return keycloak.token;
};

export const isAuthenticated = (): boolean => {
  return !!keycloak.authenticated;
};

export const hasRole = (role: string): boolean => {
  return keycloak.hasRealmRole(role);
};

export const getUserInfo = () => {
  return keycloak.tokenParsed;
};

export default keycloak;
