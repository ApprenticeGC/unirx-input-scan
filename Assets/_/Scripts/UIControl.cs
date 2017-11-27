using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using UniRx;
using UniRx.Triggers;

namespace Demo.InputScan
{
    public enum ButtonState
    {
        None,
        Down,
        Up
    }

    public class UIControl : MonoBehaviour
    {
        public UnityEngine.UI.Button button;
        public UnityEngine.UI.Text label;

        //
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        void Start()
        {
            // Down stream
            var buttonDownObservable = button.OnPointerDownAsObservable().Select(x => ButtonState.Down);
            // Up stream
            var buttonUpObservable = button.OnPointerUpAsObservable().Select(x => ButtonState.Up);

            // Combined stream, up and down
            var buttonObservable = buttonDownObservable.Merge(buttonUpObservable);

            buttonObservable
                // Use Anonymous Types for storing state
                .Scan(new { accTimer = 0.0f, previousState = ButtonState.None }, (acc, next) => {
                    var newAccTimer = 0.0f;
                    var state = ButtonState.None;
                    if (next == ButtonState.Down)
                    {
                        newAccTimer = Time.timeSinceLevelLoad;
                        state = ButtonState.Down;
                    }
                    else if (next == ButtonState.Up)
                    {
                        newAccTimer = Time.timeSinceLevelLoad - acc.accTimer;
                        state = ButtonState.Up;
                    }
                    // Debug.Log(next.ToString());
                    return new { accTimer = newAccTimer, previousState = state };
                })
                // Filter, only interested in up event
                .Where(x => x.previousState == ButtonState.Up)
                .Subscribe(x => {
                    // Debug.Log(x.accTimer);
                    label.text = $"Pressed for {x.accTimer} seconds";
                })
                .AddTo(_compositeDisposable);
        }
    }
}
