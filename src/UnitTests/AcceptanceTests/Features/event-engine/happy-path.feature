@featureSet=eventEngine @beta
Feature: Event Engine
  This is a description of the feature

  Background:
    Given the demo keyset with event engine enabled

  @contract=simpleSubscribe
  Scenario: Successfully receive messages
    When I subscribe
    Then I receive the message in my subscribe response
    And I observe the following:
      | type       | name                    |
      | event      | SUBSCRIPTION_CHANGED    |
      | invocation | HANDSHAKE               |
      | event      | HANDSHAKE_SUCCESS       |
      | invocation | CANCEL_HANDSHAKE        |
      | invocation | EMIT_STATUS             |
      | invocation | RECEIVE_MESSAGES        |
      | event      | RECEIVE_SUCCESS         |
      | invocation | CANCEL_RECEIVE_MESSAGES |
      | invocation | EMIT_MESSAGES           |
      | invocation | EMIT_STATUS             |
      | invocation | RECEIVE_MESSAGES        |

  @contract=subscribeHandshakeFailure
  Scenario: Complete handshake failure
    Given a linear reconnection policy with 3 retries
    When I subscribe
    Then I receive an error in my subscribe response
    And I observe the following:
      | type       | name                        |
      | event      | SUBSCRIPTION_CHANGED        |
      | invocation | HANDSHAKE                   |
      | event      | HANDSHAKE_FAILURE           |
      | invocation | CANCEL_HANDSHAKE            |
      | invocation | HANDSHAKE_RECONNECT         |
      | event      | HANDSHAKE_RECONNECT_FAILURE |
      | invocation | CANCEL_HANDSHAKE_RECONNECT  |
      | invocation | HANDSHAKE_RECONNECT         |
      | event      | HANDSHAKE_RECONNECT_FAILURE |
      | invocation | CANCEL_HANDSHAKE_RECONNECT  |
      | invocation | HANDSHAKE_RECONNECT         |
      | event      | HANDSHAKE_RECONNECT_FAILURE |
      | invocation | CANCEL_HANDSHAKE_RECONNECT  |
      | invocation | HANDSHAKE_RECONNECT         |
      | event      | HANDSHAKE_RECONNECT_GIVEUP  |
      | invocation | CANCEL_HANDSHAKE_RECONNECT  |
      | invocation | EMIT_STATUS                 |

  @contract=subscribeHandshakeRecovery
  Scenario: Handshake failure recovery
    Given a linear reconnection policy with 3 retries
    When I subscribe
    Then I receive the message in my subscribe response
    And I observe the following:
      | type       | name                        |
      | event      | SUBSCRIPTION_CHANGED        |
      | invocation | HANDSHAKE                   |
      | event      | HANDSHAKE_FAILURE           |
      | invocation | CANCEL_HANDSHAKE            |
      | invocation | HANDSHAKE_RECONNECT         |
      | event      | HANDSHAKE_RECONNECT_FAILURE |
      | invocation | CANCEL_HANDSHAKE_RECONNECT  |
      | invocation | HANDSHAKE_RECONNECT         |
      | event      | HANDSHAKE_RECONNECT_SUCCESS |
      | invocation | CANCEL_HANDSHAKE_RECONNECT  |
      | invocation | EMIT_STATUS                 |
      | invocation | RECEIVE_MESSAGES            |
      | event      | RECEIVE_SUCCESS             |
      | invocation | CANCEL_RECEIVE_MESSAGES     |
      | invocation | EMIT_MESSAGES               |
      | invocation | EMIT_STATUS                 |
      | invocation | RECEIVE_MESSAGES            |

  @contract=subscribeReceivingRecovery
  Scenario: Receiving failure recovery
    Given a linear reconnection policy with 3 retries
    When I subscribe
    Then I receive the message in my subscribe response
    And I observe the following:
      | type       | name                      |
      | event      | SUBSCRIPTION_CHANGED      |
      | invocation | HANDSHAKE                 |
      | event      | HANDSHAKE_SUCCESS         |
      | invocation | CANCEL_HANDSHAKE          |
      | invocation | EMIT_STATUS               |
      | invocation | RECEIVE_MESSAGES          |
      | event      | RECEIVE_FAILURE           |
      | invocation | CANCEL_RECEIVE_MESSAGES   |
      | invocation | RECEIVE_RECONNECT         |
      | event      | RECEIVE_RECONNECT_SUCCESS |
      | invocation | CANCEL_RECEIVE_RECONNECT  |
      | invocation | EMIT_MESSAGES             |
      | invocation | EMIT_STATUS               |
      | invocation | RECEIVE_MESSAGES          |