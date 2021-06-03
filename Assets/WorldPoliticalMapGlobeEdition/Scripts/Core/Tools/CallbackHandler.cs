using System;
using System.Collections;

namespace WPM {

    public abstract class CallbackHandler {

        public static CallbackHandler Null = new CallbackHandlerNop(null);

        protected WorldMapGlobe map;

        protected CallbackHandler(WorldMapGlobe instance) {
            map = instance;
        }

        public abstract void Then(Action action);

        protected virtual IEnumerator WaitForEnd(Action action) { yield break; }
    }

}