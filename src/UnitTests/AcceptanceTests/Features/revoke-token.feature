@featureSet=access @beta
Feature: Revoke an access token
  As a PubNub customer I want to withdraw existing permission for
  specific PubNub resources by revoking corresponding tokens.

  Background: I have enabled access manager
    Given I have a keyset with access manager enabled

  @contract=revokeValidToken
  Scenario: Revoke existing valid token
    Given a token
    When I revoke a token
    Then I get confirmation that token has been revoked

  @contract=revokeInvalidToken
  Scenario: Revoke invalid token
    Given a token
    When I revoke a token
    Then an error is returned
    * the error status code is 400
    * the error message is 'Invalid token'
    * the error source is 'revoke'
    * the error detail message is not empty
    * the error detail location is 'token'
    * the error detail location type is 'path'
    * the error service is 'Access Manager'

  @contract=revokeFeatureDisabled
  Scenario: Revoke a token while it is disabled on a server
    Given a token
    When I revoke a token
    Then an error is returned
    * the error status code is 403
    * the error message is 'Feature disabled'
    * the error source is 'revoke'
    * the error detail message is 'Token revocation is disabled.'
    * the error detail location is 'subscribe-key'
    * the error detail location type is 'path'
    * the error service is 'Access Manager'

  @contract=revokeEncodePathParameter
  Scenario: Revoke a token with characters that require url encoding
    Given the token string 'unescaped-_.ABCabc123 escaped;,/?:@&=+$#'
    When I revoke a token
    Then I get confirmation that token has been revoked