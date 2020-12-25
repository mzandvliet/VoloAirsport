/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using RamjetAnvil.Gui;
using RamjetAnvil.Reactive;
using RamjetAnvil.Unity.Utility;
using UnityEngine;
using Environment = RamjetAnvil.Volo.GameManagement.Environment;
using Object = UnityEngine.Object;

namespace RamjetAnvil.Volo.Menu
{
    public class SpawnpointGui
    {
        private readonly Spawnpoint _spawnpoint;
        private readonly IAnimation _highlightAnimation;
        private readonly GameObject _instance;

        public SpawnpointGui(Spawnpoint spawnpoint, IAnimation highlightAnimation, GameObject instance)
        {
            _spawnpoint = spawnpoint;
            _highlightAnimation = highlightAnimation;
            _instance = instance;
        }

        public Spawnpoint Spawnpoint
        {
            get { return _spawnpoint; }
        }

        public IAnimation HighlightAnimation
        {
            get { return _highlightAnimation; }
        }

        public GameObject Instance
        {
            get { return _instance; }
        }

        public void Show() {
            _instance.SetActive(true);
        }

        public void Hide() {
            _instance.SetActive(false);
        }
    }
    
    public static class SpawnpointMenu
    {
        public static IList<SpawnpointGui> OpenMenu(IClock menuClock, Environment environment,
            SpawnpointMenuData spawnpointMenuData, CameraManager cameraManager) {
                var cameraRig = cameraManager.Rig;

            // Find spawnpoints
            var spawnpoints = GameObject.FindGameObjectsWithTag("Spawnpoint")
                .Select(s => s.GetComponent<Spawnpoint>()).ToList();
            var spawnpointGuis = new SpawnpointGui[spawnpoints.Count];

            for (int i = 0; i < spawnpoints.Count; i++)
            {
                var spawnpoint = spawnpoints[i];
                Debug.Log("Spawnpoint " + spawnpoint + " prefab " + spawnpointMenuData.SpawnpointGuiPrefab);

                // TODO Unique to a spawnpoint type is its prefab and the highlight animation, so make them configurable!
                var spawnPointGui = CreateGuiSpawnpoint(spawnpointMenuData.SpawnpointGuiPrefab, spawnpoint);

                var initialScale = spawnPointGui.transform.localScale;
               
                // TODO Paramaterize these vars:
                var scaleSpeed = 10f;
                var scaleFactor = 1.3f;

                var animationTime = 0f;

                var renderer = spawnPointGui.GetComponent<MeshRenderer>();
                AnonymousMonoBehaviours.UpdateLocal(spawnPointGui, () =>
                {
                    var spawnPointDistance = GuiPlacement.RelativeScale(spawnpointMenuData.MenuCameraTransform, spawnpoint.transform.position);
                    var spawnpointWorldPosition = spawnpoint.transform.position;
                    var spawnpointGuiScale = initialScale * spawnPointDistance;
                    spawnpointGuiScale = ScaleAnimation(scaleSpeed, animationTime, spawnpointGuiScale, spawnpointGuiScale * scaleFactor);
                    spawnPointGui.transform.localScale = spawnpointGuiScale;
                    GuiPlacement.FaceTransform(spawnPointGui.transform, cameraRig.transform);
                    // Set the bottom position of the GUI element as anchor point
                    spawnPointGui.transform.position = spawnpointWorldPosition + spawnpointMenuData.MenuCameraTransform.up * (renderer.bounds.size.y / 2);
                    spawnPointGui.transform.Rotate(0, 180, 0, Space.Self);
                });

                var unityAnimation = new UnityAnimation(
                    animation: UnityAnimation.AddAnimation(spawnPointGui, () =>
                    {
                        animationTime += menuClock.DeltaTime;
                    }),
                    resetAnimation: () =>
                    {
                        animationTime = 0f;
                    });
                spawnpointGuis[i] = new SpawnpointGui(spawnpoint, unityAnimation, spawnPointGui);
            }
            return spawnpointGuis;
        }

        // keep track of time after animation started and use that to lerp

        public static Vector3 ScaleAnimation(float speed, float deltaTime, Vector3 initialSize, Vector3 scaledSize)
        {
            return Vector3.Lerp(initialSize, scaledSize, speed * deltaTime);
        }

        public static GameObject CreateGuiSpawnpoint(GameObject spawnpointPrefab, Spawnpoint spawnpoint)
        {
            var guiSpawnpoint = (GameObject)Object.Instantiate(spawnpointPrefab,
                spawnpoint.transform.position, Quaternion.identity);
            guiSpawnpoint.name = "Spawnpoint_" + spawnpoint.Id + "_Gui";
            guiSpawnpoint.layer = LayerMask.NameToLayer("Gui");
            return guiSpawnpoint;
        }

        public static SpawnpointMenuContext ShowMenu(SpawnpointMenuData spawnpointMenuData, 
            Environment environment, IEnumerable<SpawnpointGui> spawnpoints)
        {
            var cameraRig = ServiceLocator.Instance.CameraManager.Rig;
            var cam = cameraRig.Cameras[0];
            var menuInput = environment.Buttons;

            var spawnpointGuis = spawnpoints.ToList();

            var mouseOverSpawnPoint = RaycastSubjectSelection(environment.MouseMovement, cameraRig, spawnpointGuis.Select(s => s.Instance),
                LayerMaskUtil.CreateLayerMask("Gui"));

            var spawnpointHighlighter = Lang.Let(() => {
                var mouseSpawnpointHighlight = mouseOverSpawnPoint
                    .Select(spawnpointSelection => spawnpointGuis.Find(spawnpointGui => spawnpointGui.Instance.Equals(spawnpointSelection.Subject)))
                    .Select(selectedSpawnpoint => spawnpointGuis.IndexOf(selectedSpawnpoint));
                var menuHighlightEvents = menuInput
                    .Where(menuEvent => menuEvent != MenuAction.Confirm && menuEvent != MenuAction.Back)
                    .Select(menuEvent => {
                        switch (menuEvent) {
                            case MenuAction.Left:
                                return new Vector2(-1, 0);
                            case MenuAction.Right:
                                return new Vector2(1, 0);
                            case MenuAction.Up:
                                return new Vector2(0, 1);
                            case MenuAction.Down:
                                return new Vector2(0, -1);
                        }
                        return Vector2.zero;
                    });

                return Observable.Create<int>(observer => {
                    var selectedSpawnpointIndex = 0;

                    observer.OnNext(selectedSpawnpointIndex);

                    var menuEventDisposable = menuHighlightEvents.Subscribe(navigationVector => {
                        int nextSpawnpoint = selectedSpawnpointIndex;
                        float nextSpawnpointDistance = Mathf.Infinity;
                        
                        var currentSpawnpointPosition = cam.WorldToScreenPoint(spawnpointGuis[selectedSpawnpointIndex].Spawnpoint.transform.position);
                        for (int i = 0; i < spawnpointGuis.Count; i++) {
                            var spawnpointPosition =
                                cam.WorldToScreenPoint(spawnpointGuis[i].Spawnpoint.transform.position);
                            var spawnpointDelta = spawnpointPosition - currentSpawnpointPosition;
                            var spawnpointDistance = spawnpointDelta.magnitude;
                            if (Vector2.Angle(navigationVector, spawnpointDelta) < 45 &&
                                spawnpointDistance < nextSpawnpointDistance && i != selectedSpawnpointIndex) {
                                nextSpawnpoint = i;
                                nextSpawnpointDistance = spawnpointDistance;
                            }
                        }
                        selectedSpawnpointIndex = nextSpawnpoint;
                        observer.OnNext(selectedSpawnpointIndex);
                    }, observer.OnError, observer.OnCompleted);

                    var mouseOverDisposable = mouseSpawnpointHighlight.Subscribe(spawnpointIndex => {
                        selectedSpawnpointIndex = spawnpointIndex;
                        observer.OnNext(selectedSpawnpointIndex);
                    }, observer.OnError, observer.OnCompleted);

                    return new CompositeDisposable(menuEventDisposable, mouseOverDisposable);
                });
            });

            var spawnPointSelection = Lang.Let(() =>
            {
                var selection = GuiComponents.MenuSelection(
                    spawnpointHighlighter, 
                    UnityObservable.CreateUpdate<Unit>(observer =>
                    {
                        if (UnityEngine.Input.GetKeyDown(KeyCode.Mouse0))
                        {
                            observer.OnNext(Unit.Default);
                        }
                    }),
                    menuInput, 
                    mouseOverSpawnPoint);
                return
                    selection.Select(spawnpointsIndex => spawnpointGuis[spawnpointsIndex].Spawnpoint);
            });

            var animations = spawnpointGuis.Select(s => s.HighlightAnimation).ToList();
            var highlightSpawnpoint = GuiComponents.Highlight(spawnpointHighlighter, new AnimationHighlighter(animations));

            var disposeMenu = highlightSpawnpoint;

            return new SpawnpointMenuContext(spawnPointSelection, disposeMenu);
        }

        private static IObservable<UnityRxObservables.SubjectSelection> RaycastSubjectSelection(IObservable<Vector3> mouseMovement, 
            ICameraRig rig, IEnumerable<GameObject> navigationList, int layerMask)
        {
            // Todo: If the game throws exceptions on the following line, it might be the gamesettings.json file is out of date.
            var rays = mouseMovement.Select(mousePosition => rig.Cameras[0].ScreenPointToRay(mousePosition));

            var raycasts = UnityRxObservables.Raycasts(rays, layerMask);
            return UnityRxObservables.RaycastSelection(raycasts, navigationList);
        }

        private static IObservable<ListNavigationEvent> ToListNavigation(
            IObservable<UnityRxObservables.SubjectSelection> selection,
            IList<GameObject> navigationList)
        {
            return selection.Select(e => ListNavigationEvent.Choose(navigationList.IndexOf(e.Subject)));
        }

        /// <summary>
        /// Sorts the vectors high to low and left to right based on two-dimensional properties.
        /// </summary>
        public static IOrderedEnumerable<T> SortSpawnpointsOn2DPosition<T>(
            Func<T, Vector3> selectVector, IEnumerable<T> vectors)
        {
            return vectors.OrderBy(v => selectVector(v).x);
        }
    }
}
*/
