# Don't Forget Me

An experiment. `unity6-port-roadmap.md` is the technical source of truth, but it's long
(900+ lines) and organized as a chronological log — good for "what happened," less good
for "what's easy to get subtly wrong if you're reconstructing context from a compressed
summary instead of the real history." This file is that second thing: a short, standing
list of facts and rules that are cheap to state plainly but expensive to get wrong,
written for whoever (human or AI) picks this project up next — including a future me,
after this session's context gets compacted.

If you're an AI assistant reading this cold: read this file, then
`unity6-port-roadmap.md` in full, before touching code. Don't reconstruct project state
from a conversation summary alone if these files are available — they're authoritative,
a summary is not.

## Facts that are easy to accidentally regress to an earlier, wrong belief

- **The flight model does not need retuning.** Early in the port, "PhysX has moved
  versions, expect to retune the flight model" was a *reasonable prediction*, stated
  confidently, and written into the roadmap's original scope. It turned out to be
  wrong — checked directly, not assumed: Mar flew the original Unity 5.5 build and the
  Unity 6 port side by side and the wingsuit/parachute flight model feels intact. If
  you're regenerating a task list from a stale memory of this project, do not
  resurrect "retune flight model against PhysX" as a task. The doc has been corrected
  in three places; if you see the old framing anywhere else, it's stale.
- **RamNet (custom networking) is not stripped, and should not be.** An early
  assessment called it unused and safe to delete. Also wrong, also corrected: singleplayer
  routes through it (`HostAsSingleplayer`) to spawn the player and replicate
  pre-existing networked objects. Deleting it breaks entering the game.
- **The parachute-explosion bug is not a flight-model problem.** It's a distinct,
  narrower thing — an impulse cascade, happens abruptly, flight feels normal right up
  until it does. Don't fold it into "the physics needs work" generally; that framing
  has already caused confusion once above.
- **CoroutineScheduler, StateMachine, and PadroneClient are no longer closed-source
  DLLs.** They were, for most of this port, and a lot of debugging technique
  ("can't see inside a precompiled plugin") was built around that constraint. That
  constraint is gone — they're plain project source under
  `Assets/Plugins/RamjetAnvil/*/Source/` now. If a bug traces into one of these and the
  instinct is "we can't see in there, need to guess" — that instinct is now wrong for
  these three specifically. Read the source.

## Workflow rules that came from real, sometimes expensive mistakes

- **Check for a running Unity process before a headless batch-mode compile.** Unity
  locks a project to one running instance; if Mar's own Editor has it open, a headless
  run either collides or silently fails. On Windows, `tasklist //FI "IMAGENAME eq
  Unity.exe"` (or `wmic process where "name='Unity.exe'"`) and check the command line
  actually references *this* project path — Mar sometimes has an unrelated Unity
  project open too, which looks alarming in a raw process list but isn't a conflict.
- **Never hand-edit a `.unity`/`.prefab` scene file while the Editor has it open.**
  Reading scene YAML is extremely safe and useful (see below); writing to it while
  Unity might overwrite it on next save is not. If a scene needs a change and the
  Editor is open, either ask Mar to make it (it's usually a 2-click Inspector change)
  or write an Editor menu-item tool that makes the change through Unity's own APIs.
- **Unity serializes component *types* by GUID, not by class name.** When a
  `[SerializeField]` reference "looks assigned" in the Inspector but behaves null, or
  you need to find every instance of a component type across scenes: `grep "^guid:"
  Foo.cs.meta` to get the type's GUID, grep scene/prefab files for that GUID, then
  follow a field's `{fileID: N}` to a `--- !u!114 &N` anchor in the same file to see
  whether it resolves to a real object or is dangling. This has been the single most
  reliable diagnostic technique this whole port — reach for it before guessing.
- **After two failed theories, stop reasoning and instrument.** Established the hard
  way during a coroutine-scheduler hang: two plausible, code-reading-derived theories
  in a row were both wrong, and acting on the first one caused a real regression
  (a silent black-screen hang). One round of actual logging at the suspect boundary
  settled it immediately. A theory that "sounds right" after reading the code is weak
  evidence in a codebase this size with this much implicit state (pooling, multiple
  schedulers, DI timing). If a fix doesn't change the symptom, the next move is a log
  statement, not a third theory.
- **Before deleting something because it "looks disposable,"** check what actually
  consumes it — grep for real usages, don't infer from naming or visual proximity to
  other things being removed. This project has a lot of half-stripped commercial
  assets (Time of Day, old Impero/InControl) where the disposable-looking and the
  load-bearing sit right next to each other in the same GameObject hierarchy or file.
  Concretely: the `EcologyEffects` prefab had a `sky` dependency that was genuinely
  dead, sitting one component away from the game's *only* directional light, which
  was not.
- **The multi-clock/multi-scheduler architecture is deliberate, not cruft.** Built for
  a still-pending multiplayer mode: singleplayer should freeze on pause, a live server
  can't. Don't consolidate schedulers across state machines to "simplify" — tried
  once, caused a silent black-screen regression via reentrant `Update()` calls. Full
  rationale is in the roadmap's "Clocks and coroutine schedulers" section; read it
  before touching anything scheduler-related.
- **`[Dependency]`-consuming `MonoBehaviour`s in this codebase are authored disabled**
  (`m_Enabled: 0`) in the Inspector, and get enabled by the DI injector only after
  successful injection. If one of these NREs on `OnEnable()`/`Start()` and the
  dependency wiring itself checks out, check `m_Enabled` in the scene YAML before
  looking further — it's a startup-order bug, not a wiring bug, and looks identical to
  one from the stack trace alone.
- **A pooled object handed out to external code must survive being recycled and
  reused for something unrelated.** If you see a "generation counter" or similar
  pattern guarding an object pool's handles (`CoroutineScheduler.cs`'s
  `RoutineHandle`), don't simplify it away — it's there because recycling silently
  *un-signals* completion to anything still holding the old handle, and that failure
  mode is a hang with no error, not a crash.
- **Shader/C# math against planet-scale world coordinates needs the numerically
  stable form.** `dot(o,o) - r*r` for a large radius catastrophically cancels in
  float32. Use `(|o|-r)*(|o|+r)` instead. Showed up as a jittering horizon line, not
  an obvious error — easy to miss if you're not looking for it.

## Collaboration notes

- Mar wants findings grounded in actually reading the code, not speculation dressed up
  as confidence — see the earlier "PhysX will need retuning" and "RamNet is unused"
  mistakes above. Both were stated with more certainty than the evidence supported at
  the time, and both turned out wrong. Prefer "I read X and found Y" over "this
  probably needs Z."
- Mar drives the Unity Editor himself (GUI-only actions: scene wiring, re-attaching
  components, running diagnostic menu items); I diagnose and fix code, and read (never
  write) scene files while his Editor might have them open. This split has worked well
  across the whole port — don't try to take over Editor-side actions.
- Never `git push` or commit unless explicitly asked, same as the general standing
  rule — this project is no exception, and Mar has been doing pushes himself.
- This file and `unity6-port-roadmap.md` both live in `Documentation/` and are
  intended to be committed to the repo — they're readable by any future contributor,
  not just a future instance of me. Write for that audience.

## Open threads, as of the last session before this note was written

- Parachute impulse-cascade explosion: reported, not reproduced, not investigated. The
  failsafe that hard-quits the game on this is itself worth understanding before the
  underlying physics bug can be chased.
- Rebinding UI is still the largest functional gap (inert stub, no live rebind, no
  persistence, no controller detection).
- `VoloModule.Run()`'s `FindObjectOfType<UnityCoroutineScheduler>()` is ambiguous
  (three scheduler instances exist in the scene). Currently lands on a working one by
  luck. Known, low-priority, deliberately not "fixed" again after the first fix
  attempt caused a regression — the correct fix is an explicit serialized reference,
  not another `FindObjectOfType` variant.
- GitHub community outreach: repo has 57 stars / 10 forks but only 8 watchers, no
  Discussions or Releases enabled. Discussed, not acted on — Mar's call whether to
  enable Discussions (owner-only setting) or cut a Release.
