// src/ReSys.Shop.Admin/src/models/accounts/auth/externals/external.model.ts

import { AuthenticationResult, UserProfile } from '../auth.model';

export interface GetOAuthConfigQuery {
  provider?: string | null;
}

export interface GetOAuthConfigResult {
  provider: string;
  clientId: string;
  authorizationUrl: string;
  tokenUrl: string;
  scopes: string[];
  responseType: string;
  additionalParameters: { [key: string]: string };
  tokenExchangeUrl: string;
  providerName: string;
  requiresPKCE: boolean;
}

export interface ExternalProvider {
  provider: string;
  displayName: string;
  loginUrl: string;
  iconUrl?: string | null;
  isEnabled: boolean;
  requiredScopes: string[];
  configurationUrl: string;
}

export interface GetExternalProvidersResult extends ExternalProvider {}

export interface ExchangeTokenParam {
  provider: string;
  accessToken?: string | null;
  idToken?: string | null;
  authorizationCode?: string | null;
  redirectUri?: string | null;
  rememberMe: boolean;
}

export interface ExchangeTokenResult extends AuthenticationResult {
  isNewUser: boolean;
  isNewLogin: boolean;
  userProfile?: UserProfile | null;
}

export interface VerifyExternalTokenParam {
  accessToken?: string | null;
  idToken?: string | null;
}

export interface ExternalUserTransfer {
  provider: string;
  providerUserId: string;
  email: string;
  firstName?: string | null;
  lastName?: string | null;
  profilePictureUrl?: string | null;
  additionalClaims: { [key: string]: string };
}

export interface VerifyExternalTokenResult extends ExternalUserTransfer {}
