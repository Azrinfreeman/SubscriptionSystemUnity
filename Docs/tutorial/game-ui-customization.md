# Customize the game menus

Edit the theme asset, shared button face and the two original scenes to change the finished menus. Unity 2022.3.62f3, uGUI and TextMeshPro are already configured; no new package is required.

## Contents

- [Find the source](#find-the-source)
- [Change a color and a menu label](#change-a-color-and-a-menu-label)
- [Keep color roles readable](#keep-color-roles-readable)
- [Understand navigation and feedback](#understand-navigation-and-feedback)
- [Run and verify](#run-and-verify)
- [Export this tutorial](#export-this-tutorial)

## Find the source

| Source | Responsibility |
| --- | --- |
| Assets/Scenes/LogMasuk.unity | Entry, login, registration and device-conflict presentation |
| Assets/Scenes/LamanUtama.unity | Player menu, profile, subscription and logout confirmation |
| Assets/UI/Settings/DefaultGameUITheme.asset | Colors and feedback/panel durations, with Inspector role/tooltips |
| Assets/UI/Prefabs/MenuButtonFace.prefab | Shared button surface, label, focus outline and lower edge |
| Assets/UI/Scripts/ | Theme bindings, input feedback, panel transitions, safe area, preferences and access summary |
| Assets/LoginController.cs and Assets/RegisterUserController.cs | Validation, request guards and persistent inline responses |
| Assets/Editor/SubscriptionPlayChecks.cs | Offline account and panel interaction checks |

The scene hierarchy under Canvas/SafeArea remains editable in the normal RectTransform Inspector. Buttons keep their original controller events. Their Face child is a prefab instance. Profile and subscription fields remain read-only TMP_InputFields because the existing loaders bind to those components.

GameUIRevampTools.Apply is a one-time migration, guarded against reapplication. Routine customization uses the scene, prefab and theme directly. Capture is an editor helper for offscreen screenshots. Generated PNGs, HTML and the Windows build are outputs, not layout sources.

## Change a color and a menu label

1. Open LogMasuk. Select Assets/UI/Settings/DefaultGameUITheme.asset and change Primary to #286C55. Loaded theme bindings update; unopened scenes apply it on enable. The main buttons should become green while their text stays white. This example gives white labels a measured contrast of 6.24:1; recalculate when choosing another color.
2. Expand Canvas/SafeArea/UtkSignIn/Button/Face/Label and edit its TMP text to Masuk. Keep that label as a scene override so the shared prefab does not rename every action.
3. Select the Button object. GameUIButtonFeedback points to Face; the button's own rectangle stays fixed. Adjust Feedback Seconds on the theme from 0.12 to 0.18 to soften hover/press response. Panel Seconds separately controls opening transitions.
4. Enter Play mode and open the login form. Confirm the changed button, then inspect the same theme in LamanUtama. Save intentional scene/asset changes after leaving Play mode.

The default title is Dunia Anda. Change Canvas/SafeArea/title in LogMasuk to your product name. It is separate from SubscriptionSettings.gameIdentifier, which remains an integration setting. Replace existing flattened background PNGs through their Image references; editable illustration originals were not invented.

To add an action, duplicate an existing scene Button, edit Face/Label and assign its onClick event in the Inspector. Check the target component and method rather than copying an unrelated billing action. Keep decorative graphics' Raycast Target off. Add reusable visual changes in Prefab Mode; preserve scene-specific text/theme-role overrides.

## Keep color roles readable

`GameUIThemeBinding.Role` selects one color from the assigned theme for its Graphic or TMP component. Keep the same theme in both scenes. The enum values are explicit so existing scene bindings retain their meaning; do not reorder them when extending the system.

| Theme field | Role and default |
| --- | --- |
| Ink | Body text and secondary focus, #203645 |
| Primary | Main action surface with white label, #205F75 |
| Surface / Field | White panels / pale #EAF3F6 input and detail rows |
| Highlight | Gold #F2BF50 focus outline on primary controls |
| Error | Error text, #A83242, paired with a useful message |

White on Primary measures 7.11:1, Ink on Field 11.13:1 and Error on white 6.56:1. Gold on Primary is 4.18:1, suitable for the focus indicator but below the 4.5:1 target for normal-sized text. Use color with labels or focus outlines rather than as the only status signal. These menus retain their existing light surfaces; no dark mode is implemented.

`GameUIButtonFeedback` caches the assigned Face's stable components in Awake. Assign a replacement Face in the Inspector before Play, then test its outline and fixed button hit area. Edit Prefab Mode for shared visuals and scene overrides for action labels. For profile rows and validation policy, use [the account customization tutorial](account-flow-customization.md).

## Understand navigation and feedback

Existing buttons activate or deactivate panel roots. GameUIPanelFlow handles the panel child's fade, scale and initial focus; Back closes through the existing button event. Tab and Shift+Tab traverse currently usable controls. Escape activates the top panel's Back action. Pointer/focus feedback never delays the account request. Hidden panels release their CanvasGroup raycasts and reset their scale.

The Animasi control stores GameUI.ReducedMotion in PlayerPrefs. Minimum motion removes panel fades/scales and decorative drift. It does not change authentication or entitlement state. No audio cues or mute control are included.

GameUIAccessSummary subscribes to PlayerManager.StateChanged. It displays refreshing, unavailable, active or inactive access using real manager state. Profile/subscription Muat semula buttons call the existing coalesced RequestRefresh. Billing still uses the existing authenticated handoff; opening or closing a browser never grants access.

## Run and verify

Open LogMasuk and press Play. Try empty fields, malformed email, Back, rapid reopen, Tab, pointer press and Animasi. For real registration and billing, configure the backend using [the existing setup guide](../SETUP.md).

Outside Play mode, save scenes and run Tools > Subscription System > Validate Current Scene in both scenes. With the shipped empty backend URL, the expected setup message is Set an HTTPS backend base URL on SubscriptionSettings. Missing references or invalid events are additional failures to fix.

Run Run Offline Regression Checks, then Run Offline Scene Checks from the same Tools menu. These use local fixtures; scene checks temporarily snapshot and restore account and motion preferences. The 3 November 2026 maintainability run passed 27 data checks and 30 scene checks, including duplicate submission, account isolation, verification cleanup, focus inside an opened form, interrupted transition reset and reduced motion. The build outcome and remaining limits are recorded in [VERIFICATION.md](../VERIFICATION.md#maintainability-refactor).

For a Windows build, use the project's existing SubscriptionTemplateTools.BuildWindowsValidation editor helper or Build Settings with both original scenes enabled. The helper writes Build/SubscriptionValidation/SubscriptionSystem.exe. It does not publish a release.

Main menus and panel captures were reviewed at 1920x1080, 1280x720, 1024x768, 2560x1080 and 720x1280. Captures are under Docs/UIRevamp/after. These offscreen renders verify layout, not a real touch keyboard or device safe-area workflow. Real gamepad input, mobile keyboard/WebView, hosted email delivery and Stripe transactions remain untested. Screen-reader support is not implemented by the standard uGUI controls.

The color, label and feedback-duration examples were applied programmatically and restored in both scenes during the maintainability pass. Default entry/home renders were also inspected at the five sizes; 1920x1080 renders matched the baseline pixel-for-pixel. The full manual Inspector walkthrough remains a human review step.

If a form is invisible, check its root's active state, Panel CanvasGroup and disabled legacy Animator before changing references. If clicks fail, check the EventSystem, GraphicRaycaster and any newly added raycast-enabled decoration. Keep the account controllers and loaders assigned when editing hierarchy.

## Export this tutorial

From the repository root, run `python Docs/tutorial/export_game_ui_tutorial.py` for both screen HTML companions. This command uses only Python's standard library.

Run `python Docs/tutorial/export_tutorial_pdfs.py` for both PDF companions and their static print HTML. This command requires the Python Playwright package and an installed Edge or Chrome browser; use `--browser PATH` for another Chromium executable. It does not download a browser. If Playwright is missing, install it with `python -m pip install playwright`.

Markdown owns the content. The exporters own presentation; `tutorial-print.css` controls paper size, fonts and colors, and `paginate-tutorial.js` controls measured page breaks. Edit these sources, then rebuild. The PDFs include clickable contents and printed repository references so they remain readable outside this checkout. The older setup-guide screenshots and trailer depict the previous UI and remain historical references.
