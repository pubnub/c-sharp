@featureSet=eventEngine
Feature: Event Engine
  This is a description of the feature

  Background:
    Given the demo keyset with event engine enabled

  @contract=simpleSubscribe @beta
  Scenario: Successfully receive messages
    When I subscribe
    When I publish a message
    Then I receive the message in my subscribe response
    And I observe the following:
      | type       | name                  |
      | event      | SUBSCRIPTION_CHANGED  |
      | invocation | HANDSHAKE             |
      | event      | HANDSHAKE_SUCCESS     |
      | invocation | CANCEL_HANDSHAKE      |
      | invocation | EMIT_STATUS           |
      | invocation | RECEIVE_EVENTS        |
      | event      | RECEIVE_SUCCESS       |
      | invocation | CANCEL_RECEIVE_EVENTS |
      | invocation | EMIT_STATUS           |
      | invocation | EMIT_EVENTS           |

  @contract=subscribeHandshakeFailure @beta
  Scenario: Complete handshake failure
    Given a linear reconnection policy with 3 retries
    When I subscribe
    Then I receive an error
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

  @contract=subscribeHandshakeRecovery @beta
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
      | invocation | RECEIVE_EVENTS              |
      | event      | RECEIVE_SUCCESS             |
      | invocation | CANCEL_RECEIVE_EVENTS       |
      | invocation | EMIT_STATUS                 |
      | invocation | EMIT_EVENTS                 |

  @contract=subscribeReceivingRecovery @beta
  Scenario: Receiving failure recovery
    When I subscribe
    Then I receive the message in my subscribe response
    And I observe the following:
      | type       | name                      |
      | event      | SUBSCRIPTION_CHANGED      |
      | invocation | HANDSHAKE                 |
      | event      | HANDSHAKE_SUCCESS         |
      | invocation | CANCEL_HANDSHAKE          |
      | invocation | EMIT_STATUS               |
      | invocation | RECEIVE_EVENTS            |
      | event      | RECEIVE_FAILURE           |
      | invocation | CANCEL_RECEIVE_EVENTS     |
      | invocation | RECEIVE_RECONNECT         |
      | event      | RECEIVE_RECONNECT_SUCCESS |
      | invocation | CANCEL_RECEIVE_RECONNECT  |
      | invocation | EMIT_STATUS               |
      | invocation | EMIT_EVENTS               |
      | invocation | RECEIVE_EVENTS            |