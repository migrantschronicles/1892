using System;
using System.Collections;

namespace WPM {

    public class CallbackHandlerForOrbitRotateTo : CallbackHandler {

        public CallbackHandlerForOrbitRotateTo(WorldMapGlobe instance) : base(instance) { }

        public override void Then(Action action) {
            if (map.isOrbitRotateToActive) {
                map.StartCoroutine(WaitForEnd(action));
            } else {
                action();
            }
        }

        protected override IEnumerator WaitForEnd(Action action) {
            while (map.isOrbitRotateToActive) {
                yield return null;
            }
            if (map.isOrbitRotateToComplete) {
                action();
            }
        }
    }

}
