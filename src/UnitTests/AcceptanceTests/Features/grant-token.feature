@featureSet=access
Feature: Grant an access token
  As a PubNub customer I want to restrict and allow access to
  specific PubNub resources (channels, channel groups, uuids)
  by my user base (both people and devices) which are each
  identified by a unique UUID.

  Background: I have enabled access manager
    Given I have a keyset with access manager enabled

  @contract=grantAllPermissions
  Scenario: Grant an access token with all permissions on all resource types with authorized uuid
    # Ensure the grant request supports the correct set of access permissions for each resource type
    Given the authorized UUID "test-authorized-uuid"
    Given the TTL 60
    Given the 'channel-1' CHANNEL resource access permissions
    * grant resource permission READ
    * grant resource permission WRITE
    * grant resource permission GET
    * grant resource permission MANAGE
    * grant resource permission UPDATE
    * grant resource permission JOIN
    * grant resource permission DELETE
    Given the 'channel_group-1' CHANNEL_GROUP resource access permissions
    * grant resource permission READ
    * grant resource permission MANAGE
    Given the 'uuid-1' UUID resource access permissions
    * grant resource permission GET
    * grant resource permission UPDATE
    * grant resource permission DELETE
    Given the '^channel-\S*$' CHANNEL pattern access permissions
    * grant pattern permission READ
    * grant pattern permission WRITE
    * grant pattern permission GET
    * grant pattern permission MANAGE
    * grant pattern permission UPDATE
    * grant pattern permission JOIN
    * grant pattern permission DELETE
    Given the '^:channel_group-\S*$' CHANNEL_GROUP pattern access permissions
    * grant pattern permission READ
    * grant pattern permission MANAGE
    Given the '^uuid-\S*$' UUID pattern access permissions
    * grant pattern permission GET
    * grant pattern permission UPDATE
    * grant pattern permission DELETE
    When I grant a token specifying those permissions
    Then the token contains the authorized UUID "test-authorized-uuid"
    Then the token contains the TTL 60
    Then the token has 'channel-1' CHANNEL resource access permissions
    * token resource permission READ
    * token resource permission WRITE
    * token resource permission GET
    * token resource permission MANAGE
    * token resource permission UPDATE
    * token resource permission JOIN
    * token resource permission DELETE
    Then the token has 'channel_group-1' CHANNEL_GROUP resource access permissions
    * token resource permission READ
    * token resource permission MANAGE
    Then the token has 'uuid-1' UUID resource access permissions
    * token resource permission GET
    * token resource permission UPDATE
    * token resource permission DELETE
    Then the token has '^channel-\S*$' CHANNEL pattern access permissions
    * token pattern permission READ
    * token pattern permission WRITE
    * token pattern permission GET
    * token pattern permission MANAGE
    * token pattern permission UPDATE
    * token pattern permission JOIN
    * token pattern permission DELETE
    Then the token has '^:channel_group-\S*$' CHANNEL_GROUP pattern access permissions
    * token pattern permission READ
    * token pattern permission MANAGE
    Then the token has '^uuid-\S*$' UUID pattern access permissions
    * token pattern permission GET
    * token pattern permission UPDATE
    * token pattern permission DELETE

  @contract=grantWithoutAuthorizedUUID
  Scenario: Grant an access token without an authorized uuid
    Given the TTL 60
    Given the 'channel-1' CHANNEL resource access permissions
    * grant resource permission READ
    When I grant a token specifying those permissions
    Then the token contains the TTL 60
    Then the token does not contain an authorized uuid
    Then the token has 'channel-1' CHANNEL resource access permissions
    * token resource permission READ

  @contract=grantWithAuthorizedUUID
  Scenario: Grant an access token successfully with an authorized uuid
    Given the authorized UUID "test-authorized-uuid"
    Given the TTL 60
    Given the 'channel-1' CHANNEL resource access permissions
    * grant resource permission READ
    When I grant a token specifying those permissions
    Then the token contains the TTL 60
    Then the token contains the authorized UUID "test-authorized-uuid"
    Then the token has 'channel-1' CHANNEL resource access permissions
    * token resource permission READ

  @contract=grantWithoutAnyPermissionsError
  Scenario: Attempt to grant an access token with all permissions empty or false and expect a server error
    Given the TTL 60
    Given the 'uuid-1' UUID resource access permissions
    * deny resource permission GET
    When I attempt to grant a token specifying those permissions
    Then an error is returned
    * the error status code is 400
    * the error message is 'Invalid permissions'
    * the error source is 'grant'
    * the error detail message is 'Unexpected value: `permissions.resources.uuids.uuid-1` must be positive and non-zero.'
    * the error detail location is 'permissions.resources.uuids.uuid-1'
    * the error detail location type is 'body'

  @contract=grantWithRegExpSyntaxError
  Scenario: Attempt to grant an access token with a regular expression containing a syntax error and expect a server error
    Given the TTL 60
    Given the '!<[^>]+>++' UUID pattern access permissions
    * grant pattern permission GET
    When I attempt to grant a token specifying those permissions
    Then an error is returned
    * the error status code is 400
    * the error message is 'Invalid RegExp'
    * the error source is 'grant'
    * the error detail message is 'Syntax error: multiple repeat.'
    * the error detail location is 'permissions.patterns.uuids.!<[^>]+>++'
    * the error detail location type is 'body'

  @contract=grantWithRegExpNonCapturingError
  Scenario: Attempt to grant an access token with a regular expression containing capturing groups and expect a server error
    Given the TTL 60
    Given the '(!<[^>]+>)+' UUID pattern access permissions
    * grant pattern permission GET
    When I attempt to grant a token specifying those permissions
    Then an error is returned
    * the error status code is 400
    * the error message is 'Invalid RegExp'
    * the error source is 'grant'
    * the error detail message is 'Only non-capturing groups are allowed. Try replacing `(` with `(?:`.'
    * the error detail location is 'permissions.patterns.uuids.(!<[^>]+>)+'
    * the error detail location type is 'body'

  Scenario: Validate that a token containing authorized uuid can be parsed correctly
    Given I have a known token containing an authorized UUID
    When I parse the token
    Then the parsed token output contains the authorized UUID "test-authorized-uuid"

  Scenario: Validate that a token containing uuid resource permissions can be parsed correctly
    Given I have a known token containing UUID resource permissions
    When I parse the token
    Then the token has 'uuid-1' UUID resource access permissions
    * token resource permission GET

  Scenario: Validate that a token containing uuid pattern permissions can be parsed correctly
    Given I have a known token containing UUID pattern Permissions
    When I parse the token
    Then the token has '^uuid-\S*$' UUID pattern access permissions
    * token pattern permission GET
