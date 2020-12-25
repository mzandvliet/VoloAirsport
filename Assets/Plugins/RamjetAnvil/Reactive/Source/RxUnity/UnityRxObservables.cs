using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

namespace RamjetAnvil.Reactive
{
    public static class UnityRxObservables
    {
        public static IObservable<float> UpdateTicks(Func<float> deltaTime)
        {
            return UnityObservable.CreateUpdate<float>(observer => observer.OnNext(deltaTime()));
        }

        public static IObservable<float> TimeElapsed(IObservable<float> deltas)
        {
            return deltas.Scan(0f, (timePassed, deltaTime) => timePassed + deltaTime);
        } 
        
        public static IObservable<Unit> UpdateTicks()
        {
            return UnityObservable.CreateUpdate<Unit>(observer => observer.OnNext(Unit.Default));
        }

        public static IObservable<SubjectSelection> RaycastSelection(IObservable<RaycastHit> raycasts, 
            IEnumerable<GameObject> gameObjects)
        {
            return gameObjects.Select(g => RaycastSelection(raycasts, g)).Merge();
        }

        public struct SubjectSelection
        {
            public sealed class Event
            {
                private Event() { }
                public static Event Selected = new Event();
                public static Event Deselected = new Event();
            }

            private readonly Event _eventType;
            private readonly GameObject _subject;

            public SubjectSelection(Event eventType, GameObject subject)
            {
                _eventType = eventType;
                _subject = subject;
            }

            public Event EventType
            {
                get { return _eventType; }
            }

            public GameObject Subject
            {
                get { return _subject; }
            }
        }

        public static IObservable<SubjectSelection> RaycastSelection(IObservable<RaycastHit> raycasts, GameObject subject)
        {
            var selectionEvents = raycasts
                .Select(raycastHit =>
                {
                    var isSubjectHit = raycastHit.collider != null && raycastHit.collider.gameObject.Equals(subject);
                    return isSubjectHit ? SubjectSelection.Event.Selected : SubjectSelection.Event.Deselected;
                });

            // Capture deselection once
            var deselection = selectionEvents
                .DistinctUntilChanged(EqualityOperatorComparer<SubjectSelection.Event>.Instance)
                .Where(@event => @event == SubjectSelection.Event.Deselected);
            // Capture selection every time there is a new raycast that points to the subject
            var selection = selectionEvents.Where(@event => @event == SubjectSelection.Event.Selected);
            return selection.Merge(deselection)
                .Select(@event => new SubjectSelection(@event, subject));
        }

        public static IObservable<RaycastHit> Raycasts(IObservable<Ray> rays, int layerMask, float distance = Mathf.Infinity)
        {
            return rays.Select(ray =>
            {
                RaycastHit hit;
                Physics.Raycast(ray, out hit, distance, layerMask);
                return hit;
            })
            // Make sure we don't do any unnecessary raycasts
            .Publish().RefCount();
        }

        public static IObservable<Vector3> MouseMove()
        {
            return UnityObservable.CreateUpdate<Vector3>(observer => observer.OnNext(Input.mousePosition))
                .DistinctUntilChanged(Vector3Comparer.Instance);
        }

        /// <summary>
        /// Can be useful for determining mouse movement velocity or any other movement velocity for that matter.
        /// </summary>
        /// <param name="movement"></param>
        /// <returns>the delta between the current and previous vector</returns>
        public static IObservable<Vector3> MovementDelta(IObservable<Vector3> movement)
        {
            return movement.Scan((prevPosition, newPosition) => newPosition - prevPosition);
        }
    }
}
