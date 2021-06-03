using System;
using System.Collections;

namespace WPM {

    public class CallbackHandlerForFlyTo : CallbackHandler {

        public CallbackHandlerForFlyTo(WorldMapGlobe instance) : base(instance) { }

        public override void Then(Action action) {
            if (map.isFlyingToActive) {
                map.StartCoroutine(WaitForEnd(action));
            } else {
                action();
            }
        }

        protected override IEnumerator WaitForEnd(Action action) {
            while(map.isFlyingToActive) {
                yield return null;
            }
            if (map.isFlyingComplete) {
                action();
            }
        }
    }

}
