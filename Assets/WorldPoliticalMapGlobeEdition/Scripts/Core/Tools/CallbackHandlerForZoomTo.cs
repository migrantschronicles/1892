using System;
using System.Collections;

namespace WPM {

    public class CallbackHandlerForZoomTo : CallbackHandler {

        public CallbackHandlerForZoomTo(WorldMapGlobe instance) : base(instance) { }

        public override void Then(Action action) {
            if (map.isZoomToActive) {
                map.StartCoroutine(WaitForEnd(action));
            } else {
                action();
            }
        }

        protected override IEnumerator WaitForEnd(Action action) {
            while (map.isZoomToActive) {
                yield return null;
            }
            if (map.isZoomComplete) {
                action();
            }
        }
    }

}
