﻿using PubnubApi.EventEngine.Core;
using PubnubApi.EventEngine.Presence.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi.EventEngine.Presence.States
{
    public class HeartbeatCooldownState : PresenceState
    {
        public override TransitionResult Transition(IEvent e)
        {
            throw new NotImplementedException();
        }
    }
}
