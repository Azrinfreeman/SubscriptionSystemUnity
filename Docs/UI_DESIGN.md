# Game menu design

The account screens use the existing illustrated world, with a compact menu, readable white forms and tactile teal controls. Layout lives in the two original scenes; no UI is generated at runtime.

| Role | Value | Editable source |
| --- | --- | --- |
| Text | #203645 | DefaultGameUITheme.ink |
| Main action | #205F75 | DefaultGameUITheme.primary |
| Panel | #FFFFFF | DefaultGameUITheme.surface |
| Input/detail row | #EAF3F6 | DefaultGameUITheme.field |
| Focus | #F2BF50 | DefaultGameUITheme.highlight |
| Error | #A83242 | DefaultGameUITheme.error |

Theme asset: `Assets/UI/Settings/DefaultGameUITheme.asset`. Button visual source: `Assets/UI/Prefabs/MenuButtonFace.prefab`. Text uses the existing LiberationSans SDF font. Scene overrides hold button labels and primary/secondary theme roles. Each button keeps its existing onClick bindings and fixed hit area; only its Face scales.

Both canvases use a 1920 by 1080 reference and a 0.5 width/height match. Functional content sits under SafeArea; backgrounds bleed to the viewport. Forms use bounded centered panels and anchor-relative rows. At the tested sizes the content fits without scrolling; this implementation does not introduce a separate portrait layout or scroll container.

Buttons ease toward 1.025 on hover/focus and 0.96 when pressed, using unscaled time. Panels fade and scale from 0.98 over 0.22 seconds. Decorative SkySpark objects drift vertically. Reduced motion removes those transforms and panel fades immediately; the preference survives logout. No UI sound was added.

Panel roots own visibility. Existing GameObject.SetActive events remain bound; GameUIPanelFlow owns the panel child's scale, opacity and focus. Replaced panel Animators remain disabled. Loading and response notices are inline and leave retry/Back reachable. Informational legacy notification clips remain available; account state remains owned by the existing controllers and PlayerManager.

For human editing steps and verification limits, use [the customization tutorial](tutorial/game-ui-customization.md).
