// World Political Map - Globe Edition for Unity - Main Script
// Created by Ramiro Oliva (Kronnect)
// Don't modify this script - changes could be lost if you upgrade to a more recent version of WPM

using System;
using UnityEngine;

namespace WPM {

    public class CallbackHandlerNop : CallbackHandler {

        public CallbackHandlerNop(WorldMapGlobe instance) : base(instance) { }

        public override void Then(Action action) {
        }

    }

}